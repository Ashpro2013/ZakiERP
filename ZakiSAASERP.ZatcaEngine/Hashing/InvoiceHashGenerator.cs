using System.Security.Cryptography;

namespace ZakiSAASERP.ZatcaEngine.Hashing;

public class InvoiceHashGenerator
{
    public string GenerateHashBase64(byte[] canonicalBytes)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(canonicalBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
