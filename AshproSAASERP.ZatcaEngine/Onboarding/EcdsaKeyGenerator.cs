using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace AshproSAASERP.ZatcaEngine.Onboarding;

public class EcdsaKeyGenerator
{
    public (string PrivateKeyPem, string PublicKeyPem, AsymmetricCipherKeyPair KeyPair)
        GenerateSecp256k1KeyPair()
    {
        var curve = SecNamedCurves.GetByName("secp256k1");
        var domainParams = new ECDomainParameters(
            curve.Curve,
            curve.G,
            curve.N,
            curve.H,
            curve.GetSeed());

        var keyGen = new ECKeyPairGenerator();
        var secureRandom = new SecureRandom();

        var keyGenParam = new ECKeyGenerationParameters(domainParams, secureRandom);
        keyGen.Init(keyGenParam);

        var keyPair = keyGen.GenerateKeyPair();

        string privateKeyPem = ExportPrivateKeyPem(keyPair.Private);
        string publicKeyPem = ExportPublicKeyPem(keyPair.Public);

        return (privateKeyPem, publicKeyPem, keyPair);
    }

    private string ExportPrivateKeyPem(AsymmetricKeyParameter privateKey)
    {
        using var sw = new StringWriter();
        var pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(privateKey);
        pemWriter.Writer.Flush();
        return sw.ToString();
    }

    private string ExportPublicKeyPem(AsymmetricKeyParameter publicKey)
    {
        using var sw = new StringWriter();
        var pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(publicKey);
        pemWriter.Writer.Flush();
        return sw.ToString();
    }
}
