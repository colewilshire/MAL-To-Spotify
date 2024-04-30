using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class Authenticator
{
    private readonly string clientId;
    private readonly PKCEHelper pkceHelper = new();
    private readonly HttpClient httpClient = new();
    public TokenResponse TokenResponse { get; private set; }

    public Authenticator(string clientId)
    {
        this.clientId = clientId;
    }

    // The authorization code will be the Return URI's "code" parameter
    public string GetAuthorizationURL(string scopes = "user:read")
    {
        return $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&scope={scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=plain";
    }

    // Source: https://myanimelist.net/apiconfig/references/authorization
    public async Task ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
        string tokenUrl = "https://myanimelist.net/v1/oauth2/token";
        FormUrlEncodedContent content = new(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("code_verifier", pkceHelper.CodeVerifier),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
        });
        HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            TokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
        }
    }

    // Source: https://myanimelist.net/apiconfig/references/authorization
    public async Task RefreshAccessToken(string refreshToken)
    {
        string tokenUrl = "https://myanimelist.net/v1/oauth2/token";

        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:"));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        FormUrlEncodedContent content = new(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
        });

        HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            TokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
        }
    }
}
