using System;
using SpotifyAPI.Web;
using UnityEngine;
using TMPro;

public class SpotifyTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField anime;
    [SerializeField] private string clientId;
    [SerializeField] private string clientSecret;
    private SpotifyAuthenticator spotifyAuthenticator;

    private async void Start()
    {
        SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();
        ClientCredentialsRequest request = new ClientCredentialsRequest(clientId, clientSecret);
        try
        {
            ClientCredentialsTokenResponse response = await new OAuthClient(config).RequestToken(request);
            SpotifyClient spotify = new(config.WithToken(response.AccessToken));
            Debug.Log("Spotify API successfully connected!");
        }
        catch (APIException e)
        {
            Debug.LogError($"Failed to connect to Spotify API: {e.Message}");
        }

        Test();
    }

    private void Test()
    {
        spotifyAuthenticator = new(clientId, clientSecret);
        string s = spotifyAuthenticator.GetAuthorizationURL();
        print(s);
        Application.OpenURL(s);

        // AQBx9msQ28vUjsfROZIrf2PUi6S-nabTGTybqh0JXKLPgV3RQpiQs84AbHWQgfgGQcvdLSQhZ-zrorTqx5ATkAsMwZhr85OEAEAvdR4LB_oDiQwVcU2x3F9K0Wwpv3R2AuKT73wnunWM9_z1nnyZbc6gCUXCnA
        // def50200a7c76456725a74ab643fd20efbb0836088e9f5e53cd8814a319c69da18b21803525db3a8bc68e3ed7fbe1ee9e64740a5172bf94b1c7fdcb412a923887d9177ea2413d2a84040f729419c838e8bbf4cb496650421ea935727b57712de88983dc135f2a86dc6d383d117f5cad5a7a8cef829524484e452cd1361e2ccc148b3a679e0bf42238f507865a49a53fd0f60952f3e6fe9f8e0bb30455c8d0ca0e632ec3d157798ca7926b41cfe9b4926a38ca137cef843604b81f940e4299741cd9468825d2467dfc27b88224040c8be5401117e1c4f8a4b34c9cbeb31a9491fb919c8c6202fad556e79a15c69ef2d1537e84bed8f7ef23be4e198d50b4a26a0781cd21df8c2b90b51d539e2a046c1eeb4dcc170a4405cd26903892b806885e8ba0be795b6852ccddbcc09a19abf9fe24384396b168d330ba961e4a743d2a9efd83521e9668dd092c608b478192475a58a5f0bed85b9d116e2d5d8f7de318662f700bd8661c2f1d9bfc3854ee340a60a2163cff4a5b1d83ad9ac445d9eff6e81f34c32a21e1e383af8600955016a1745bd6ec633b187e896a8
        // def5020011abbd1f295d1d2142be04844fe436efd90af9d34869f5eb6d66132cd73a1f01aac7c311e3563eb3f267b23a3ced6b69d7867d4925410d0e61960d3b1ae1717cb312d1df7fb1d6173f0b4d993281391b3d93219b5b42a9adc5d3e0a524fc03431dacc90cc722ddd2fbcfd5d7bbe4bbb3e5d5a50cd44a3b3790b839aae3af2e1933fdc5392afcfe86d4c57c9f82cc0bd96772fb56d85cc9defa33748b5d37fd40e1fa4083906a1a40ae09a249ec7f92346046af40c737621addbbee3993311c7611b220fa5dfe69336da1b81fe3fa36df5c74123f386b4c47d49456a22a16af06a1689d5ec20a58cf38eda33a86b396bf49f7e79cd5c8e3cedb1dda72fc081e022f1db8ad4145eb780f44f5558d52d67363360f9cdd43ee9999ef8866644c91f87797808020679695e6850fc25c799fab3ccc6b75cba8d3f787f2c7e523c2fa4fe9f622e1d67b9786923f26eb3f5a6d58969942458a9544f1c1b92078f82a848e8c79ed1ed1d69df346286acb979f415d4b2429c06b741e27cecca7abfd74a79dc3475f3ee7173967c625b86c2c9c65b147cc5e5a81
    }

    public async void Test2(string authorizationCode)
    {
        anime.text = "starting";

        await spotifyAuthenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        anime.text = "finishing";

        if (spotifyAuthenticator.TokenResponse != null)
        {
            //anime.text = spotifyAuthenticator.TokenResponse.AccessToken;

            SpotifyClient spotifyClient = new(spotifyAuthenticator.TokenResponse.AccessToken);
            PrivateUser current = await spotifyClient.UserProfile.Current();

            anime.text = current.Id;
        }
    }

    // private void Test()
    // {
    //     // var client_id = 'CLIENT_ID';
    //     // var redirect_uri = "http://localhost:8888/callback";

    //     // var state = generateRandomString(16);

    //     // localStorage.setItem(stateKey, state);
    //     // var scope = 'user-read-private user-read-email';

    //     // var url = 'https://accounts.spotify.com/authorize';
    //     // url += '?response_type=token';
    //     // url += '&client_id=' + encodeURIComponent(client_id);
    //     // url += '&scope=' + encodeURIComponent(scope);
    //     // url += '&redirect_uri=' + encodeURIComponent(redirect_uri);
    //     // url += '&state=' + encodeURIComponent(state);
    // }

    // private void Test()
    // {
    //     // Make sure "spotifyapi.web.oauth://token" is in your applications redirect URIs!
    //     var loginRequest = new LoginRequest(
    //     new Uri("mal2spotify://auth"), clientId, LoginRequest.ResponseType.Token)
    //     {
    //         Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative }
    //     };
    //     var uri = loginRequest.ToUri();
    //     string redirect = "";
    //     string uri0 = "https://accounts.spotify.com/authorize?client_id=37a5e94c391748f6b24c4b2dd181c9b0&response_type=token&redirect_uri=mal2spotify:%2f%2fauth%2f&scope=playlist-read-private+playlist-read-collaborative";
    //     string uri1 = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirect}/&scope=user-read-currently-playing%20user-top-read";
    //     string uri2 = "https://accounts.spotify.com/authorize?client_id=37a5e94c391748f6b24c4b2dd181c9b0&response_type=token&redirect_uri=mal2spotify://auth&scope=playlist-read-private+playlist-read-collaborative";

    //     // This call requires Spotify.Web.Auth
    //     //Application.OpenURL(uri.ToString());
    //     //string redirect = "mal2spotify://auth";
    //     //string url = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirect}/&scope=user-read-currently-playing%20user-top-read";
    //     //Application.OpenURL(url);
    //     Application.OpenURL(uri2);
    // }
}
