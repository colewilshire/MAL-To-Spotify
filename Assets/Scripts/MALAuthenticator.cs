using System.Threading.Tasks;
using UnityEngine;

public class MALAuthenticator : MonoBehaviour
{
    [SerializeField] private string clientId;
    private Authenticator Authenticator;
    private TaskCompletionSource<MALClient> tcs;

    // Called by MALController to begin authentication process
    public async Task<MALClient> AuthenticateMALClient()
    {
        Authenticator = new(clientId);
        tcs = new();
        string authUrl = Authenticator.GetAuthorizationURL();

        Application.OpenURL(authUrl);

        return await tcs.Task;
    }

    // Called by MALToSpotifyDeepLinkActivity.handleIntent automatically through a browser intent
    public async void CreateMALClient(string authorizationCode)
    {
        await Authenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        if (Authenticator.TokenResponse != null)
        {
            MALClient malClient = new(Authenticator.TokenResponse.AccessToken);
            tcs.TrySetResult(malClient);
        }
    }
}
