using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MALAuthenticator : MonoBehaviour
{
    [SerializeField] private string clientId;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text animeText;
    [SerializeField] private string insertAuthCodeHere;
    [SerializeField] private TMP_InputField inputField;

    public Authenticator Authenticator;

    public void HandleCode(string code)
    {
        inputField.text = code;

        GetMALClient(code);
    }

    private void Start()
    {
        Authenticator = new(clientId);
        string authUrl = Authenticator.GetAuthorizationURL();

        Application.OpenURL(authUrl);
    }

    private async void GetMALClient(string authorizationCode)
    {
        await Authenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        if (Authenticator.TokenResponse != null)
        {
            codeText.text = $"Access Token: {Authenticator.TokenResponse.AccessToken}";
            GetComponent<MALController>().MALClient = new MALClient(Authenticator.TokenResponse.AccessToken);

            // Testing
            var animeList = await GetComponent<MALController>().MALClient.GetAnimeListAsync("@me");
            animeText.text = $"First anime in list: {animeList.Data[0].Node.Title}";
        }
    }
}
