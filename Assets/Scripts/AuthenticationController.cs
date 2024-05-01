using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using UnityEngine;
using TMPro;

public class AuthenticationController : MonoBehaviour
{
    [SerializeField] private TMP_InputField anime;
    [SerializeField] private string clientId;
    private MALAuthenticator malAuthenticator;
    private TaskCompletionSource<MALClient> tcs;
    private string savedTokenPath;
    private TokenResponse savedToken;

    private void Awake()
    {
        savedTokenPath = Path.Combine(Application.persistentDataPath, "Token.sav");
        LoadSavedToken();
    }

    private void SaveToken(TokenResponse tokenResponse)
    {
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        string serializedToken = JsonSerializer.Serialize(tokenResponse, jsonSerializerOptions);

        File.WriteAllText(savedTokenPath, serializedToken);
    }

    private void LoadSavedToken()
    {
        if (!File.Exists(savedTokenPath)) return;
        string serializedToken = File.ReadAllText(savedTokenPath);
        
        savedToken = JsonSerializer.Deserialize<TokenResponse>(serializedToken);
    }

    private async Task<bool> CreateMALClientFromSave(string token)
    {
        await malAuthenticator.RefreshAccessToken(token);

        if (malAuthenticator.TokenResponse != null)
        {
            SaveToken(malAuthenticator.TokenResponse);

            MALClient malClient = new(malAuthenticator.TokenResponse.AccessToken);
            tcs.TrySetResult(malClient);
            return true;
        }
        
        return false;
    }

    private async Task<bool> VerifySavedAccessToken(string token)
    {
        MALClient malClient = new(token);
        AnimeListResponse animeListResponse = await malClient.GetAnimeListAsync();

        if (animeListResponse != null)
        {
            tcs.TrySetResult(malClient);
            return true;
        }

        return false;
    }

    // Called by MALController to begin authentication process
    public async Task<MALClient> AuthenticateMALClient()
    {
        malAuthenticator = new(clientId);
        tcs = new();

        if (savedToken == null || (!await VerifySavedAccessToken(savedToken.AccessToken) && !await CreateMALClientFromSave(savedToken.RefreshToken)))
        {
            string authUrl = malAuthenticator.GetAuthorizationURL();
            Application.OpenURL(authUrl);
        }

        return await tcs.Task;
    }

    // Called by MALToSpotifyDeepLinkActivity.handleIntent automatically through a browser intent
    public async void CreateMALClient(string authorizationCode)
    {
        await malAuthenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        if (malAuthenticator.TokenResponse != null)
        {
            SaveToken(malAuthenticator.TokenResponse);

            MALClient malClient = new(malAuthenticator.TokenResponse.AccessToken);
            tcs.TrySetResult(malClient);
        }
    }
}
