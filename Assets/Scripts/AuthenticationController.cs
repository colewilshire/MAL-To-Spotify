using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using UnityEngine;
using TMPro;
using SpotifyAPI.Web;

public class AuthenticationController : MonoBehaviour
{
    [SerializeField] private TMP_InputField malInputField;
    [SerializeField] private TMP_InputField spotifyInputField;
    [SerializeField] private string malClientId;
    [SerializeField] private string spotifyClientId;
    [SerializeField] private string spotifyClientSecret;

    [SerializeField] private string malTokenSaveName = "MALToken";
    [SerializeField] private string spotifyTokenSaveName = "SpotifyToken";
    [SerializeField] private string saveFileExtension = "sav";

    private OAuthAuthenticator malAuthenticator;
    private OAuthAuthenticator spotifyAuthenticator;
    private TaskCompletionSource<MALClient> malTcs;
    private TaskCompletionSource<SpotifyClient> spotifyTcs;
    private TokenResponse savedMALToken;
    private TokenResponse savedSpotifyToken;

    private void Awake()
    {
        InitializeAuthenticators();
        AttemptRefreshTokens();
    }

    private void InitializeAuthenticators()
    {
        OAuthConfig malConfig = new()
        {
            ClientId = malClientId,
            AuthorizationEndpoint = "https://myanimelist.net/v1/oauth2/authorize",
            TokenEndpoint = "https://myanimelist.net/v1/oauth2/token",
            RedirectUri = "mal2spotify://auth",
            Scopes = "user:read",
            UsePkce = true
        };
        OAuthConfig spotifyConfig = new()
        {
            ClientId = spotifyClientId,
            ClientSecret = spotifyClientSecret,
            AuthorizationEndpoint = "https://accounts.spotify.com/authorize",
            TokenEndpoint = "https://accounts.spotify.com/api/token",
            RedirectUri = "spotify2mal://auth",
            Scopes = "playlist-modify-public",
            UsePkce = false
        };

        malAuthenticator = new(malConfig);
        spotifyAuthenticator = new(spotifyConfig);
    }

    private async void AttemptRefreshTokens()
    {
        savedMALToken = LoadSavedToken(malTokenSaveName);
        savedSpotifyToken = LoadSavedToken(spotifyTokenSaveName);

        if (savedMALToken != null && await AttemptRefreshToken(savedMALToken.RefreshToken, malAuthenticator))
        {
            MALClient malClient = new(malAuthenticator.TokenResponse.AccessToken);
            //SaveToken(malAuthenticator.TokenResponse, malTokenSaveName);  // MAL will seemingly issue an infinite number of Refresh Tokens with token obtained using Refresh Tokens

            malTcs = new();
            malTcs.TrySetResult(malClient);
        }
        if (savedSpotifyToken != null && await AttemptRefreshToken(savedSpotifyToken.RefreshToken, spotifyAuthenticator))
        {
            SpotifyClient spotifyClient = new(spotifyAuthenticator.TokenResponse.AccessToken);
            //SaveToken(spotifyAuthenticator.TokenResponse, spotifyTokenSaveName);  // Spotify will only issue one RefreshToken. After using it to get a new token, said token will have a null Refresh Token value

            spotifyTcs = new();
            spotifyTcs.TrySetResult(spotifyClient);
        }
    }

    private async Task<bool> AttemptRefreshToken(string refreshToken, OAuthAuthenticator authenticator)
    {
        await authenticator.RefreshAccessToken(refreshToken);

        if (authenticator.TokenResponse != null)
        {
            return true;
        }
        
        return false;
    }

    private void SaveToken(TokenResponse tokenResponse, string saveName)
    {
        string savedTokenPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{saveFileExtension}");

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        string serializedToken = JsonSerializer.Serialize(tokenResponse, jsonSerializerOptions);

        File.WriteAllText(savedTokenPath, serializedToken);
    }

    private TokenResponse LoadSavedToken(string saveName)
    {
        string savedTokenPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{saveFileExtension}");
        if (!File.Exists(savedTokenPath)) return null;

        string serializedToken = File.ReadAllText(savedTokenPath);
        TokenResponse savedToken = JsonSerializer.Deserialize<TokenResponse>(serializedToken);

        return savedToken;
    }

    // Called by MALController to begin authentication process
    public async Task<MALClient> AuthenticateMALClient()
    {
        if (malTcs == null)
        {
            string authUrl = malAuthenticator.GetAuthorizationURL();
            malTcs = new();

            Application.OpenURL(authUrl);
        }
        
        return await malTcs.Task;
    }

    public async Task<SpotifyClient> AuthenticateSpotifyClient()
    {
        if (spotifyTcs == null)
        {
            string authUrl = spotifyAuthenticator.GetAuthorizationURL();
            spotifyTcs = new();

            Application.OpenURL(authUrl);
        }
        
        return await spotifyTcs.Task;
    }

    // Called by MALToSpotifyDeepLinkActivity.handleIntent automatically through a browser intent
    public async void CreateMALClient(string authorizationCode)
    {
        await malAuthenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        if (malAuthenticator.TokenResponse != null)
        {
            SaveToken(malAuthenticator.TokenResponse, malTokenSaveName);

            MALClient malClient = new(malAuthenticator.TokenResponse.AccessToken);
            malTcs.TrySetResult(malClient);
        }
    }

    // Called by MALToSpotifyDeepLinkActivity.handleIntent automatically through a browser intent
    public async void CreateSpotifyClient(string authorizationCode)
    {
        await spotifyAuthenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        if (spotifyAuthenticator.TokenResponse != null)
        {
            SaveToken(spotifyAuthenticator.TokenResponse, spotifyTokenSaveName);

            SpotifyClient spotifyClient = new(spotifyAuthenticator.TokenResponse.AccessToken);
            spotifyTcs.TrySetResult(spotifyClient);
        }
    }
}
