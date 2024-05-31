using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using UnityEngine;
using TMPro;
using SpotifyAPI.Web;

public class AuthenticationController : MonoBehaviour
{
    [SerializeField] private TMP_InputField anime;
    [SerializeField] private string clientId;
    [SerializeField] private string spotifyClientId;
    [SerializeField] private string spotifyClientSecret;

    [SerializeField] private string malTokenSaveName = "MALToken";
    [SerializeField] private string spotifyTokenSaveName = "SpotifyToken";
    [SerializeField] private string saveFileExtension = "sav";

    private OAuthAuthenticator malAuthenticator;
    private OAuthAuthenticator spotifyAuthenticator;
    private TaskCompletionSource<MALClient> tcs;
    private TaskCompletionSource<SpotifyClient> spotifyTcs;
    //private string savedTokenPath;
    private TokenResponse savedMALToken;
    private TokenResponse savedSpotifyToken;

    private void Awake()
    {
        //savedTokenPath = Path.Combine(Application.persistentDataPath, "MALToken.sav");
        //savedMALToken = LoadSavedToken(malTokenSaveName);
        //savedSpotifyToken = LoadSavedToken(spotifyTokenSaveName);
    }

    private async void AttemptRefreshTokens()
    {
        savedMALToken = LoadSavedToken(malTokenSaveName);
        savedSpotifyToken = LoadSavedToken(spotifyTokenSaveName);

        if (savedMALToken != null && await VerifyRefreshToken(savedMALToken.RefreshToken, malAuthenticator))
        {
            // Create client from save
        }
        else
        {
            // Create client
            await AuthenticateMALClient();
        }

        if (savedSpotifyToken != null && await VerifyRefreshToken(savedSpotifyToken.RefreshToken, spotifyAuthenticator))
        {
            // Create client from save
        }
        else
        {
            // Create client
        }
    }

    // private async void VerifySavedToken(string tokenSaveName, OAuthAuthenticator authenticator)
    // {
    //     TokenResponse token = LoadSavedToken(tokenSaveName);
    //     if (token == null) return;

    //     if (tokenSaveName == malTokenSaveName && await VerifyMALAccessToken(token.AccessToken))
    //     {
    //         return; //true
    //     }
    //     else if (tokenSaveName == spotifyTokenSaveName && await VerifySpotifyAccessToken(token.AccessToken))
    //     {
    //         return; //true
    //     }
    //     else if (await VerifyRefreshToken(token.RefreshToken, authenticator))
    //     {
    //         return; //true
    //     }
        
    //     return; //false

    //     // if (!(tokenSaveName == malTokenSaveName && await VerifyMALAccessToken(token.AccessToken)) && !(tokenSaveName == spotifyTokenSaveName && await VerifySpotifyAccessToken(token.AccessToken)))
    //     // {
    //     //     await VerifyRefreshToken(token.RefreshToken, authenticator);
    //     // }
    // }

    // private async Task<bool> VerifyMALAccessToken(string token)
    // {
    //     MALClient malClient = new(token);
    //     AnimeListResponse animeListResponse = await malClient.GetAnimeListAsync();

    //     if (animeListResponse != null)
    //     {
    //         return true;
    //     }

    //     return false;
    // }

    // private async Task<bool> VerifySpotifyAccessToken(string token)
    // {
    //     SpotifyClient spotifyClient = new(spotifyAuthenticator.TokenResponse.AccessToken);
    //     PrivateUser currentUser = await spotifyClient.UserProfile.Current();

    //     if (currentUser != null)
    //     {
    //         return true;
    //     }

    //     return false;
    // }

    private async Task<bool> VerifyRefreshToken(string token, OAuthAuthenticator authenticator)
    {
        await authenticator.RefreshAccessToken(token);

        if (authenticator.TokenResponse != null)
        {
            SaveToken(authenticator.TokenResponse, malTokenSaveName);
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

    // private async Task<bool> CreateMALClientFromSave(string token)
    // {
    //     await malAuthenticator.RefreshAccessToken(token);

    //     if (malAuthenticator.TokenResponse != null)
    //     {
    //         SaveToken(malAuthenticator.TokenResponse, malTokenSaveName);

    //         MALClient malClient = new(malAuthenticator.TokenResponse.AccessToken);
    //         tcs.TrySetResult(malClient);
    //         return true;
    //     }
        
    //     return false;
    // }

    // private async Task<bool> VerifySavedAccessToken(string token)
    // {
    //     MALClient malClient = new(token);
    //     AnimeListResponse animeListResponse = await malClient.GetAnimeListAsync();

    //     if (animeListResponse != null)
    //     {
    //         tcs.TrySetResult(malClient);
    //         return true;
    //     }

    //     return false;
    // }

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

        //if (savedMALToken == null || (!await VerifySavedAccessToken(savedMALToken.AccessToken) && !await CreateMALClientFromSave(savedMALToken.RefreshToken)))
        //{
            string authUrl = malAuthenticator.GetAuthorizationURL();
            Application.OpenURL(authUrl);
        //}
        
        return await tcs.Task;
    }

    public async Task<SpotifyClient> AuthenticateSpotifyClient()
    {
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
        spotifyAuthenticator = new(spotifyConfig);
        spotifyTcs = new();

        //if (savedSpotifyToken == null || (!await VerifySavedAccessToken(savedSpotifyToken.AccessToken) && !await CreateMALClientFromSave(savedMALToken.RefreshToken)))
        //{
            string authUrl = spotifyAuthenticator.GetAuthorizationURL();
            Application.OpenURL(authUrl);
        //}
        
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
            tcs.TrySetResult(malClient);
        }
    }

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
