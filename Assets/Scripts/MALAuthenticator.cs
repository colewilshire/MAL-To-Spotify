using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MALAuthenticator : MonoBehaviour
{
    [SerializeField] private string clientId;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text codeText; // Make sure to assign this in the inspector.
    [SerializeField] private string insertAuthCodeHere;

    public Authenticator Authenticator;

    private void Start()
    {
        Authenticator = new(clientId);

        button.onClick.AddListener(async () => await ButtonClicked(insertAuthCodeHere));

        string authUrl = Authenticator.GetAuthorizationURL();
        Application.OpenURL(authUrl);
    }

    private async Task ButtonClicked(string authorizationCode)
    {
        //string authUrl = mALAuthenticator.GetAuthorizationURL();
        //Application.OpenURL(authUrl); // Open the authorization URL in the user's browser.

        // Wait for the user to enter the authorization code manually or via some other mechanism
        // For demonstration purposes, let's assume 'insertAuthCodeHere' is obtained somehow.
        //string insertAuthCodeHere = await Task.FromResult("YourMethodToGetTheCode()");

        await Authenticator.ExchangeAuthorizationCodeForToken(authorizationCode);

        if (Authenticator.TokenResponse != null)
        {
            codeText.text = $"Access Token: {Authenticator.TokenResponse.AccessToken}";
            GetComponent<MALController>().MALClient = new MALClient(Authenticator.TokenResponse.AccessToken);
        }
    }
}
