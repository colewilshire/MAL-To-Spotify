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

    // Source: https://myanimelist.net/apiconfig/references/authorization, https://developer.spotify.com/documentation/web-api/tutorials/refreshing-tokens
    public async Task RefreshAccessToken(string refreshToken)
    {
        // Encode client_id and client_secret for Basic Authentication
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.ClientId}:{config.ClientSecret}"));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        // Define the request body parameters
        var keyValues = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("refresh_token", refreshToken)
        };

        // Add client_id if client_secret is not used (Spotify uses both in basic auth)
        if (string.IsNullOrEmpty(config.ClientSecret))
        {
            keyValues.Add(new KeyValuePair<string, string>("client_id", config.ClientId));
        }

        // Create the content for the POST request
        FormUrlEncodedContent content = new(keyValues);

        // Make the POST request to the token endpoint
        HttpResponseMessage response = await httpClient.PostAsync(config.TokenEndpoint, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            // Deserialize and store the token response
            TokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
        }
    }
}

