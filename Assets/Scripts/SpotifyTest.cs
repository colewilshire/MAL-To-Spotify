using SpotifyAPI.Web;
using UnityEngine;

public class SpotifyTest : MonoBehaviour
{
    [SerializeField] private string clientId;
    [SerializeField] private string clientSecret;

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
    }
}
