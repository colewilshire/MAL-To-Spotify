using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using UnityEngine;
using TMPro;

public class AuthenticationController : MonoBehaviour
{
    [SerializeField] private TMP_InputField anime;
    [SerializeField] private string clientId;
    [SerializeField] private string spotifyClientId;
    [SerializeField] private string spotifyClientSecret;
    //private MALAuthenticator malAuthenticator;
    private OAuthAuthenticator malAuthenticator;
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

    // private async Task<bool> CreateMALClientFromSave(string token)
    // {
    //     await malAuthenticator.RefreshAccessToken(token);

    //     if (malAuthenticator.TokenResponse != null)
    //     {
    //         SaveToken(malAuthenticator.TokenResponse);

    //         MALClient malClient = new(malAuthenticator.TokenResponse.AccessToken);
    //         tcs.TrySetResult(malClient);
    //         return true;
    //     }
        
    //     return false;
    // }

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
        OAuthConfig malConfig = new()
        {
            ClientId = clientId,
            AuthorizationEndpoint = "https://myanimelist.net/v1/oauth2/authorize",
            TokenEndpoint = "https://myanimelist.net/v1/oauth2/token",
            RedirectUri = "mal2spotify://auth",
            Scopes = "user:read",
            UsePkce = true
        };
        malAuthenticator = new(malConfig);
        tcs = new();

        //if (savedToken == null || (!await VerifySavedAccessToken(savedToken.AccessToken) && !await CreateMALClientFromSave(savedToken.RefreshToken)))
        //{
            string authUrl = malAuthenticator.GetAuthorizationURL();
            Application.OpenURL(authUrl);
        //}

        // https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id=MALClientId&redirect_uri=http://localhost/callback&scope=user:read&code_challenge=HK80hnBY22-qt0_RusKaqjiRE1T-E7L6jTARXj0AK_Q&code_challenge_method=plain
        
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

    // public async void Test()
    // {
    //     OAuthConfig malConfig = new()
    //     {
    //         ClientId = "MALClientId",
    //         AuthorizationEndpoint = "https://myanimelist.net/v1/oauth2/authorize",
    //         TokenEndpoint = "https://myanimelist.net/v1/oauth2/token",
    //         RedirectUri = "http://localhost/callback",
    //         Scopes = "user:read",
    //         UsePkce = true
    //     };

    //     // OAuthConfig spotifyConfig = new()
    //     // {
    //     //     ClientId = "SpotifyClientId",
    //     //     ClientSecret = "SpotifyClientSecret", // Spotify requires client secret
    //     //     AuthorizationEndpoint = "https://accounts.spotify.com/authorize",
    //     //     TokenEndpoint = "https://accounts.spotify.com/api/token",
    //     //     RedirectUri = "spotify2mal://auth",
    //     //     Scopes = "playlist-modify-public",
    //     //     UsePkce = false // Assuming PKCE is not used for Spotify in this example
    //     // };

    //     OAuthAuthenticator malAuthenticator = new(malConfig);
    //     //var spotifyAuthenticator = new OAuthAuthenticator(spotifyConfig);
    // }
}
