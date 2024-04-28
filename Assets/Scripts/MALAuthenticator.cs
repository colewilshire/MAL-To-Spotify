using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class MALAuthenticator : MonoBehaviour
{
    [SerializeField] private TMP_Text anime;
    [SerializeField] private string clientId;
    private Authenticator Authenticator;
    private TaskCompletionSource<MALClient> tcs;
    private string refreshTokenPath;
    private string refreshToken;

    private void Awake()
    {
        refreshTokenPath = Path.Combine(Application.persistentDataPath, "Refresh Token.sav");
        refreshToken = LoadRefreshToken();
    }

    private void SaveRefreshToken(string refreshToken)
    {
        //Directory.CreateDirectory(refreshTokenPath);
        try
        {
            File.WriteAllText(refreshTokenPath, refreshToken);
        }
        catch (System.Exception e)
        {
            anime.text = $"Save Error: {e}";
        } 
    }

    private string LoadRefreshToken()
    {
        if (!File.Exists(refreshTokenPath)) return null;
        
        try
        {
            refreshToken = File.ReadAllText(refreshTokenPath);
        }
        catch (System.Exception e)
        {
            anime.text = $"Load error: {e}";
        }

        return refreshToken;
    }

    // Called by MALController to begin authentication process
    public async Task<MALClient> AuthenticateMALClient()
    {
        Authenticator = new(clientId);
        tcs = new();

        if (refreshToken == null)
        {
            string authUrl = Authenticator.GetAuthorizationURL();
            Application.OpenURL(authUrl);
        }
        else
        {
            CreateMALClientFromSave(refreshToken);
        }

        return await tcs.Task;
    }

    // Called by MALToSpotifyDeepLinkActivity.handleIntent automatically through a browser intent
    public async void CreateMALClient(string authorizationCode)
    {
        await Authenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        if (Authenticator.TokenResponse != null)
        {
            SaveRefreshToken(Authenticator.TokenResponse.RefreshToken);
            anime.text = $"{Authenticator.TokenResponse.RefreshToken}";

            MALClient malClient = new(Authenticator.TokenResponse.AccessToken);
            tcs.TrySetResult(malClient);
        }
    }

    public async void CreateMALClientFromSave(string token)
    {
        await Authenticator.RefreshAccessToken(token);

        if (Authenticator.TokenResponse != null)
        {
            SaveRefreshToken(Authenticator.TokenResponse.RefreshToken);
            anime.text = $"{Authenticator.TokenResponse.RefreshToken}";

            MALClient malClient = new(Authenticator.TokenResponse.AccessToken);
            tcs.TrySetResult(malClient);
        }
    }
}
