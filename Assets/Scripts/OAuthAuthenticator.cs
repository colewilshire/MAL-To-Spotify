using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OAuthAuthenticator
{
    private readonly OAuthConfig config;
    private readonly HttpClient httpClient = new();
    private readonly PKCEHelper pkceHelper = new();
    public TokenResponse TokenResponse { get; private set; }

    public OAuthAuthenticator(OAuthConfig config)
    {
        this.config = config;
    }

    public string GetAuthorizationURL()
    {
        if (config.UsePkce)
        {
            return $"{config.AuthorizationEndpoint}?response_type=code&client_id={config.ClientId}&redirect_uri={config.RedirectUri}&scope={config.Scopes}&code_challenge={pkceHelper.CodeChallenge}&code_challenge_method=plain";
        }
        else
        {
            return $"{config.AuthorizationEndpoint}?client_id={config.ClientId}&response_type=code&redirect_uri={config.RedirectUri}&scope={config.Scopes}";
        }
    }

    public async Task ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
        List<KeyValuePair<string, string>> keyValues = new()
        {
            new("grant_type", "authorization_code"),
            new("code", authorizationCode),
            new("redirect_uri", config.RedirectUri)
        };

        if (config.UsePkce)
        {
            keyValues.Add(new KeyValuePair<string, string>("code_verifier", pkceHelper.CodeVerifier));
            keyValues.Add(new KeyValuePair<string, string>("client_id", config.ClientId));
        }

        FormUrlEncodedContent content = new(keyValues);

        if (!string.IsNullOrEmpty(config.ClientSecret))
        {
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.ClientId}:{config.ClientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        }

        HttpResponseMessage response = await httpClient.PostAsync(config.TokenEndpoint, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            TokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
        }
    }

    // Source: https://myanimelist.net/apiconfig/references/authorization
    public async Task RefreshAccessToken(string refreshToken)
    {
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.ClientId}:"));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        FormUrlEncodedContent content = new(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
        });

        HttpResponseMessage response = await httpClient.PostAsync(config.TokenEndpoint, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            TokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
        }
    }
}

