using System.Threading.Tasks;
using SpotifyAPI.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpotifyTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField anime;
    [SerializeField] private Button spotifyLoginButton;
    [SerializeField] private string clientId;
    [SerializeField] private string clientSecret;

    private AuthenticationController authenticationController;
    private SpotifyClient spotifyClient;

    private void Start()
    {
        authenticationController = GetComponent<AuthenticationController>();

        spotifyLoginButton.onClick.AddListener(async () => await Test());
    }

    private async Task Test()
    {
        spotifyClient = await authenticationController.AuthenticateSpotifyClient();

        PrivateUser current = await spotifyClient.UserProfile.Current();
        anime.text = current.Id;
    }
}
