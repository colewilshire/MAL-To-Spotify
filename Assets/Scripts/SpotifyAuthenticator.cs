using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class SpotifyAuthenticator
{
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly HttpClient httpClient = new();
    public TokenResponse TokenResponse { get; private set; }

    public SpotifyAuthenticator(string clientId, string clientSecret)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
    }

    // The authorization code will be the Return URI's "code" parameter
    public string GetAuthorizationURL(string scopes = "playlist-modify-public") // https://developer.spotify.com/documentation/web-api/concepts/scopes
    {
        string t = "spotify2mal://auth";
        return $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={t}";
    }

    // Source: https://developer.spotify.com/documentation/web-api/tutorials/code-flow
    public async Task ExchangeAuthorizationCodeForToken(string authorizationCode)
    {
        string tokenUrl = "https://accounts.spotify.com/api/token";

        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        FormUrlEncodedContent content = new(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", "spotify2mal://auth")
        });

        HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            TokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
        }
    }
}
