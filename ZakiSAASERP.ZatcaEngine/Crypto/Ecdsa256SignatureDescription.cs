using System.Security.Cryptography;

namespace ZakiSAASERP.ZatcaEngine.Crypto;

public class Ecdsa256SignatureDescription : SignatureDescription
{
    public const string AlgorithmUri = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256";

    public Ecdsa256SignatureDescription()
    {
        KeyAlgorithm = typeof(ECDsa).AssemblyQualifiedName;
        DigestAlgorithm = typeof(SHA256).AssemblyQualifiedName;
        FormatterAlgorithm = typeof(EcdsaSignatureFormatter).AssemblyQualifiedName;
        DeformatterAlgorithm = typeof(EcdsaSignatureDeformatter).AssemblyQualifiedName;
    }

    public override HashAlgorithm CreateDigest() => SHA256.Create();

    public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key is not ECDsa ecdsa)
            throw new ArgumentException($"Key must be of type {nameof(ECDsa)}", nameof(key));

        var formatter = new EcdsaSignatureFormatter();
        formatter.SetKey(ecdsa);
        return formatter;
    }

    public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key is not ECDsa ecdsa)
            throw new ArgumentException($"Key must be of type {nameof(ECDsa)}", nameof(key));

        var deformatter = new EcdsaSignatureDeformatter();
        deformatter.SetKey(ecdsa);
        return deformatter;
    }

    private static bool _isRegistered;
    private static readonly object _lock = new();

    public static void Register()
    {
        if (_isRegistered)
            return;

        lock (_lock)
        {
            if (!_isRegistered)
            {
                CryptoConfig.AddAlgorithm(
                    typeof(Ecdsa256SignatureDescription),
                    AlgorithmUri);
                _isRegistered = true;
            }
        }
    }
}

public class EcdsaSignatureFormatter : AsymmetricSignatureFormatter
{
    private ECDsa? _key;

    public EcdsaSignatureFormatter() { }

    public EcdsaSignatureFormatter(AsymmetricAlgorithm key)
    {
        SetKey(key);
    }

    public override void SetKey(AsymmetricAlgorithm key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _key = key as ECDsa ?? throw new ArgumentException($"Key must be of type {nameof(ECDsa)}", nameof(key));
    }

    public override void SetHashAlgorithm(string strName)
    {
        // Fixed to SHA256 for ECDSA-SHA256
    }

    public override byte[] CreateSignature(byte[] rgbHash)
    {
        ArgumentNullException.ThrowIfNull(rgbHash);
        if (_key == null)
            throw new CryptographicUnexpectedOperationException("Key is not set.");

        // ZATCA & XAdES expect ASN.1 DER sequence for ECDSA signatures
        return _key.SignHash(rgbHash, DSASignatureFormat.Rfc3279DerSequence);
    }
}

public class EcdsaSignatureDeformatter : AsymmetricSignatureDeformatter
{
    private ECDsa? _key;

    public EcdsaSignatureDeformatter() { }

    public EcdsaSignatureDeformatter(AsymmetricAlgorithm key)
    {
        SetKey(key);
    }

    public override void SetKey(AsymmetricAlgorithm key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _key = key as ECDsa ?? throw new ArgumentException($"Key must be of type {nameof(ECDsa)}", nameof(key));
    }

    public override void SetHashAlgorithm(string strName)
    {
        // Fixed to SHA256 for ECDSA-SHA256
    }

    public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
    {
        ArgumentNullException.ThrowIfNull(rgbHash);
        ArgumentNullException.ThrowIfNull(rgbSignature);

        if (_key == null)
            throw new CryptographicUnexpectedOperationException("Key is not set.");

        // First try DER format (standard for XAdES / ZATCA)
        try
        {
            if (_key.VerifyHash(rgbHash, rgbSignature, DSASignatureFormat.Rfc3279DerSequence))
                return true;
        }
        catch
        {
            // Fall through to try IEEE P1363 format
        }

        try
        {
            return _key.VerifyHash(rgbHash, rgbSignature, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }
        catch
        {
            return false;
        }
    }
}
