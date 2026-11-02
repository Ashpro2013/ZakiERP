using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AshproSAASERP.POS.Services;

public static class RegistryHelper
{
    private const string SubKeyPath = @"SOFTWARE\Tech";
    private const string EncryptionKey = "MAKV2SPBNI99212";
    private static readonly byte[] Salt = [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76];

    public static string GetData(string key, string defaultValue = "", bool encrypt = false)
    {
        return GetRegistryData(key, defaultValue, encrypt);
    }

    public static void SetData(string key, string value, bool encrypt = false)
    {
        SetRegistryData(key, value, encrypt);
    }

    public static string GetRegistryData(string skey, string dValue = "", bool ApplyEncryption = false)
    {
        string value = dValue;
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(SubKeyPath, true);
            if (key != null)
            {
                var raw = ApplyEncryption 
                    ? key.GetValue(Encrypt(skey)) 
                    : key.GetValue(skey);

                if (raw != null)
                {
                    value = ApplyEncryption ? Decrypt(raw.ToString()!) : raw.ToString()!;
                }
            }
            else
            {
                using RegistryKey newKey = Registry.CurrentUser.CreateSubKey(SubKeyPath);
                if (ApplyEncryption)
                {
                    newKey.SetValue(Encrypt(skey), Encrypt(dValue));
                }
                else
                {
                    newKey.SetValue(skey, dValue);
                }
            }
        }
        catch (Exception)
        {
            try
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(SubKeyPath);
                if (ApplyEncryption)
                {
                    key.SetValue(Encrypt(skey), Encrypt(dValue));
                }
                else
                {
                    key.SetValue(skey, dValue);
                }
            }
            catch { }
        }
        return value;
    }

    public static void SetRegistryData(string skey, string svalue, bool ApplyEncryption = false)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(SubKeyPath, true);
            if (key != null)
            {
                if (ApplyEncryption)
                {
                    key.SetValue(Encrypt(skey), Encrypt(svalue), RegistryValueKind.String);
                }
                else
                {
                    key.SetValue(skey, svalue, RegistryValueKind.String);
                }
            }
            else
            {
                using RegistryKey newKey = Registry.CurrentUser.CreateSubKey(SubKeyPath);
                if (ApplyEncryption)
                {
                    newKey.SetValue(Encrypt(skey), Encrypt(svalue));
                }
                else
                {
                    newKey.SetValue(skey, svalue);
                }
            }
        }
        catch { }
    }

    public static string Encrypt(string clearText)
    {
        if (string.IsNullOrEmpty(clearText)) return string.Empty;
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        DeriveKeyAndIV(EncryptionKey, Salt, out var key, out var iv);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.FlushFinalBlock();
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            DeriveKeyAndIV(EncryptionKey, Salt, out var key, out var iv);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 0, cipherBytes.Length);
                cs.FlushFinalBlock();
            }

            return Encoding.Unicode.GetString(ms.ToArray());
        }
        catch
        {
            return cipherText;
        }
    }

    private static void DeriveKeyAndIV(string password, byte[] salt, out byte[] key, out byte[] iv)
    {
        byte[] derived = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: 1000,
            hashAlgorithm: HashAlgorithmName.SHA1,
            outputLength: 48
        );

        key = derived[..32];
        iv = derived[32..48];
    }
}

