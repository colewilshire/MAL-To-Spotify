using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MALAuthenticator : MonoBehaviour
{
    [SerializeField] private string clientId; // Ensure this matches the Client ID registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string clientSecret; // Ensure this matches the Client Secret registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string redirectUri; // Ensure this matches the redirect URI registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string state; // A unique state value for CSRF protection (the value of this string is arbitrary)
    private PKCEHelper pkceHelper; // For handling PKCE challenge and verifier
    public TokenResponse TokenResponse {get; private set;}

    //
    [SerializeField] private string insertAuthCodeHere;
    [SerializeField] private Button button;
    //

    private void Start()
    {
        pkceHelper = new PKCEHelper();

        button.onClick.AddListener(() => ExchangeAuthorizationCodeForToken(insertAuthCodeHere));

        StartAuthorizationRequest();
    }

    private void StartAuthorizationRequest()
    {
        // Generate the authorization URL with PKCE and state parameters
        string scopes = "user:read";
        string authUrl = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope={scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=plain";

        // Open the authorization URL in the user's web browser
        Application.OpenURL(authUrl);
    }

    private IEnumerator ExchangeCodeForTokenCoroutine(string authorizationCode)
    {
        Debug.Log(authorizationCode);

        // Exchange the authorization code for an access token
        string tokenUrl = "https://myanimelist.net/v1/oauth2/token";
        WWWForm form = new();
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret);
        form.AddField("code", authorizationCode);
        form.AddField("code_verifier", pkceHelper.CodeVerifier);
        form.AddField("redirect_uri", redirectUri);
        form.AddField("grant_type", "authorization_code");

        UnityWebRequest request = UnityWebRequest.Post(tokenUrl, form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Token exchange successful: " + request.downloadHandler.text);
            TokenResponse = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Token exchange failed: " + request.error);

            if (request.downloadHandler != null)
            {
                string responseBody = request.downloadHandler.text;
                Debug.LogError("Response Body: " + responseBody);
            }
        }
    }

    // Call this method with the authorization code to start the token exchange process
    private void ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
        StartCoroutine(ExchangeCodeForTokenCoroutine(authorizationCode));
    }
}
