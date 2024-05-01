public class OAuthConfig
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AuthorizationEndpoint { get; set; }
    public string TokenEndpoint { get; set; }
    public string RedirectUri { get; set; }
    public string Scopes { get; set; }
    public bool UsePkce { get; set; }
}
