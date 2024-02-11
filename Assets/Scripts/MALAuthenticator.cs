using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MALAuthenticator : MonoBehaviour
{
    [SerializeField] private string clientId;
    [SerializeField] private string clientSecret;
    [SerializeField] private string redirectUri;
    [SerializeField] private string state;
    private PKCEHelper pkceHelper;
    public TokenResponse TokenResponse { get; private set; }

    [SerializeField] private string insertAuthCodeHere;
    [SerializeField] private Button button;

    private readonly HttpClient httpClient = new();

    private void Start()
    {
        pkceHelper = new PKCEHelper();

        button.onClick.AddListener(async () => await ExchangeAuthorizationCodeForToken(insertAuthCodeHere));

        StartAuthorizationRequest();
    }

    private void StartAuthorizationRequest()
    {
        string scopes = "user:read";
        string authUrl = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope={scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=plain";

        Application.OpenURL(authUrl);
    }

    private async Task ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
        Debug.Log(authorizationCode);

        string tokenUrl = "https://myanimelist.net/v1/oauth2/token";

        FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("code_verifier", pkceHelper.CodeVerifier),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
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
}
