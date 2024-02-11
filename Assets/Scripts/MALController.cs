using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MALCOntroller : MonoBehaviour
{
    [SerializeField]
    private string clientId = "1d5580f5cd0d5d6d142bb0f777fc3ff3"; // Set this in the Unity Inspector or through your application's initialization process
    [SerializeField]
    private string clientSecret = "2f13274dfba2bdef15de7ad49d79d18d0d0da6126751d0a144749c129ff26992"; // Use only if necessary and secure
    private string redirectUri = "https://github.com/colewilshire/MAL-To-Spotify"; // Ensure this matches the redirect URI registered with MyAnimeList
    private string state = "THIS_IS_ARBITRARY"; // A unique state value for CSRF protection
    private PKCEHelper pkceHelper; // For handling PKCE challenge and verifier

    void Start()
    {
        // Initialize PKCE helper and start the authorization process
        pkceHelper = new PKCEHelper();
        StartAuthorizationRequest();
    }

    public void StartAuthorizationRequest()
    {
        // Generate the authorization URL with PKCE and state parameters
        string scopes = "user:read"; // Specify the required scopes here
        string authUrl = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope={scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=S256";

        // Open the authorization URL in the user's web browser
        Application.OpenURL(authUrl);
    }

    public IEnumerator ExchangeCodeForTokenCoroutine(string authorizationCode)
    {
        // Exchange the authorization code for an access token
        string tokenUrl = "https://myanimelist.net/v1/oauth2/token";
        WWWForm form = new WWWForm();
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret); // Handle with care
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
            Debug.LogError("Token exchange failed: " + request.error);
        }
    }

    // Call this method with the authorization code to start the token exchange process
    public void ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
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
        CodeChallenge = GenerateCodeChallenge(CodeVerifier);
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

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(hash)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
