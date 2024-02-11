using System;

public class PKCEHelper
{
    public string CodeVerifier { get; private set; }
    public string CodeChallenge { get; private set; }

    public PKCEHelper()
    {
        CodeVerifier = GenerateCodeVerifier();
        CodeChallenge = CodeVerifier;   // For Plain method, the CodeChallenge is the same as the CodeVerifier
    }

    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];   // 256 bits
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}