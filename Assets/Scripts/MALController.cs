using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;   //
using System.Text.Json;

public class MALCOntroller : MonoBehaviour
{
    [SerializeField] private string clientId; // Ensure this matches the Client ID registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string clientSecret; // Ensure this matches the Client Secret registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string redirectUri; // Ensure this matches the redirect URI registered with MAL (https://myanimelist.net/apiconfig)
    [SerializeField] private string state; // A unique state value for CSRF protection (the value of this string is arbitrary)
    private readonly string baseUrl = "https://api.myanimelist.net/v2/";
    private PKCEHelper pkceHelper; // For handling PKCE challenge and verifier
    private TokenResponse tokenResponse;

    //
    [SerializeField] private string insertAuthCodeHere;
    [SerializeField] private Button button;
    [SerializeField] private Button button2;
    //

    private void Start()
    {
        pkceHelper = new PKCEHelper();
        //
        button.onClick.AddListener(() => ExchangeAuthorizationCodeForToken(insertAuthCodeHere));
        button2.onClick.AddListener(() => StartCoroutine(GetUsersAnimeList()));
        //

        StartAuthorizationRequest();
    }

    private IEnumerator GetUsersAnimeList(string userName = "@me")
    {
        string url = $"{baseUrl}users/{userName}/animelist";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {tokenResponse.access_token}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(request.downloadHandler.text);
            DeserializeAnimeList(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"Failed to fetch data: {request.error}");
        }
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
            tokenResponse = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
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

    private void DeserializeAnimeList(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Helps with case sensitivity
        };

        AnimeListResponse animeListResponse = JsonSerializer.Deserialize<AnimeListResponse>(jsonString, options);

        // Example usage
        if (animeListResponse != null && animeListResponse.Data != null)
        {
            foreach (var animeData in animeListResponse.Data)
            {
                Debug.Log($"ID: {animeData.Node.Id}, Title: {animeData.Node.Title}");
            }
        }
    }
}
