using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MALAuthenticator : MonoBehaviour
{
    [SerializeField] private string clientId;
    [SerializeField] private string clientSecret; // Note: Typically not used directly in WebGL due to security concerns
    [SerializeField] private string redirectUri;
    [SerializeField] private string state;
    private PKCEHelper pkceHelper;
    public TokenResponse TokenResponse { get; private set; }

    [SerializeField] private string insertAuthCodeHere; // For manual input in this example
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text code;

    private readonly HttpClient httpClient = new();

    private async void Start()
    {
        pkceHelper = new PKCEHelper();

        // Adjust to async lambda to await the token exchange process
        button.onClick.AddListener(async () => await ExchangeAuthorizationCodeForToken(insertAuthCodeHere));
        LogDocumentReferrer();
        RetrievePageURL();

        string authenticationCode = RetrieveAuthCode();
        if (authenticationCode != null)
        {
            await ExchangeAuthorizationCodeForToken(authenticationCode);
        }
        else
        {
            StartAuthorizationRequest();
        }
    }

    private void StartAuthorizationRequest()
    {
        string scopes = "user:read";
        //string authUrl = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope={scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=plain";
        string authUrl = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&state={state}&scope={scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=plain";

        //#if UNITY_WEBGL && !UNITY_EDITOR
        // Call the imported JavaScript function directly
        //StartOAuthFlow(authUrl, redirectUri);
        //ChangeWindowLocation(authUrl);
        //#else
        // Fallback for editor and non-WebGL builds
        Application.OpenURL(authUrl);
        //#endif
    }

    // The JavaScript interop function declaration
    [DllImport("__Internal")]
    private static extern void StartOAuthFlow(string url, string redirectUri);

    [DllImport("__Internal")]
    private static extern void ChangeWindowLocation(string url);

    [DllImport("__Internal")]
    private static extern void LogDocumentReferrer();

    [DllImport("__Internal")]
    private static extern IntPtr GetPageURL();

    public static string RetrievePageURL()
    {
        IntPtr ptr = GetPageURL();
        string url = Marshal.PtrToStringAuto(ptr);
        return url;
    }

    [DllImport("__Internal")]
    private static extern IntPtr GetAuthCodeFromURL();

    // Use this method to call the JavaScript function and process the returned code
    public string RetrieveAuthCode()
    {
        IntPtr ptr = GetAuthCodeFromURL();
        
        // Check if the pointer is not null
        if (ptr != IntPtr.Zero)
        {
            string authCode = Marshal.PtrToStringAuto(ptr);
            Debug.Log("Authorization Code: " + authCode);
            // Further processing, like exchanging the auth code for a token
            return authCode;
        }
        else
        {
            Debug.Log("Authorization code not found in URL.");
            return null;
        }
    }

    // Method to receive the authorization code from JavaScript
    public async Task ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
        Debug.Log($"Authorization Code: {authorizationCode}");

        string tokenUrl = "https://myanimelist.net/v1/oauth2/token";
        FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("code_verifier", pkceHelper.CodeVerifier),
            //new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
        });

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Token exchange successful: " + responseBody);
                TokenResponse = JsonUtility.FromJson<TokenResponse>(responseBody);
                code.text = TokenResponse.access_token;
            }
            else
            {
                Debug.LogError("Token exchange failed: " + responseBody);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception during token exchange: {e.Message}");
        }
    }

    // For demonstration, this is a callback method expected to be called from JavaScript
    public void ReceiveAuthCode(string code)
    {
        Debug.Log("Received auth code from JavaScript: " + code);
        _ = ExchangeAuthorizationCodeForToken(code); // Trigger the token exchange process
    }
}
