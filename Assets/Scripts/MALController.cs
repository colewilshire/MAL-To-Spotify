using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;   //

public class MALCOntroller : MonoBehaviour
{
    [SerializeField] private string clientId; // Ensure this matches the Client ID registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string clientSecret; // Ensure this matches the Client Secret registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string redirectUri; // Ensure this matches the redirect URI registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string state; // A unique state value for CSRF protection (the value of this string is arbitrary)
    private PKCEHelper pkceHelper; // For handling PKCE challenge and verifier

    //
    [SerializeField] private string insertAuthCodeHere;
    [SerializeField] private Button button;
    //

    private void LogDebugInformation(string authorizationCode)
    {
        Debug.Log($"[OAuth Debug] Client ID: {clientId}");
        // Be cautious about logging the clientSecret, especially in production or shared environments
        Debug.Log($"[OAuth Debug] Client Secret: {clientSecret}");
        Debug.Log($"[OAuth Debug] Redirect URI: {redirectUri}");
        Debug.Log($"[OAuth Debug] Authorization Code: {authorizationCode}");
        Debug.Log($"[OAuth Debug] Code Verifier: {pkceHelper.CodeVerifier}");
        Debug.Log($"[OAuth Debug] Code Challenge: {pkceHelper.CodeChallenge}");
    }

    private void Start()
    {
        pkceHelper = new PKCEHelper();
        //
        button.onClick.AddListener(() => ExchangeAuthorizationCodeForToken(insertAuthCodeHere));
        //

        StartAuthorizationRequest();
    }

    public void StartAuthorizationRequest()
    {
        // Generate the authorization URL with PKCE and state parameters
        string scopes = "user:read";
        string authUrl = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope={scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=plain";

        // Open the authorization URL in the user's web browser
        Application.OpenURL(authUrl);
    }

    public IEnumerator ExchangeCodeForTokenCoroutine(string authorizationCode)
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
            // Parse and store the access token for future API calls
        }
        else
        {
            // If the request failed, log the error and the response body for more details
            Debug.LogError("Token exchange failed: " + request.error);
            // Check if there's a response body and log it
            if (request.downloadHandler != null)
            {
                string responseBody = request.downloadHandler.text;
                Debug.LogError("Response Body: " + responseBody);
            }
        }
    }

    // Call this method with the authorization code to start the token exchange process
    public void ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
        LogDebugInformation(authorizationCode);

        StartCoroutine(ExchangeCodeForTokenCoroutine(authorizationCode));
    }
}

public class PKCEHelper
{
    public string CodeVerifier { get; private set; }
    public string CodeChallenge { get; private set; }

    public PKCEHelper()
    {
        CodeVerifier = GenerateCodeVerifier();
        // For Plain method, the CodeChallenge is the same as the CodeVerifier
        CodeChallenge = CodeVerifier; // Adjusted for Plain method
    }

    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32]; // 256 bits
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    // No need to hash the code_verifier for the Plain method, so this method is adjusted
    // Removed GenerateCodeChallenge method since it's not needed for Plain method
}
