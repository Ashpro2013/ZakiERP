using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

namespace ZakiSAASERP.Mob.Services;

public static class RegistryHelper
{
    public static string GetData(string key, string defaultValue = "", bool encrypt = false)
    {
        string value;
        if (OperatingSystem.IsWindows())
        {
            value = GetRegistryData(key, defaultValue, encrypt);
        }
        else
        {
            value = Preferences.Get(key, defaultValue);
            if (encrypt && !string.IsNullOrEmpty(value))
            {
                value = RegistryHelper.Decrypt(value);
            }
        }

        if (key == "ConString" && !string.IsNullOrEmpty(value) && !OperatingSystem.IsWindows())
        {
            var dbProvider = GetData("DBProvider", "");
            if (dbProvider == "SQLite" || string.IsNullOrEmpty(dbProvider))
            {
                var parts = value.Split('=', 2);
                if (parts.Length == 2 && parts[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                {
                    var dbFile = parts[1].Trim();
                    if (!Path.IsPathRooted(dbFile))
                    {
                        var writablePath = Path.Combine(FileSystem.AppDataDirectory, dbFile);
                        var dir = Path.GetDirectoryName(writablePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        value = $"Data Source={writablePath}";
                    }
                }
            }
        }

        return value;
    }

    public static void SetData(string key, string value, bool encrypt = false)
    {
        if (OperatingSystem.IsWindows())
        {
            SetRegistryData(key, value, encrypt);
        }
        else
        {
            var data = encrypt ? RegistryHelper.Encrypt(value) : value;
            Preferences.Set(key, data);
        }
    }
    public static string GetRegistryData(string skey, string dValue = "", bool ApplyEncryption = false)
    {
        string? value = dValue;
        try
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Tech", true)!;
            if (key != null)
            {
                value = ApplyEncryption ? Decrypt(key.GetValue(Encrypt(skey))!.ToString()!) : key.GetValue(skey)!.ToString()!;
                key.Close();
            }
            else
            {
                key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Tech");
                if (ApplyEncryption)
                {
                    key.SetValue(Encrypt(skey), Encrypt(dValue));
                }
                else
                {
                    key.SetValue(skey, dValue);
                }
                key.Close();
            }
        }
        catch (NullReferenceException)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Tech");
            if (ApplyEncryption)
            {
                key.SetValue(Encrypt(skey), Encrypt(dValue));
            }
            else
            {
                key.SetValue(skey, dValue);
            }
            key.Close();
        }
        return value;
    }
    public static void SetRegistryData(string skey, string svalue, bool ApplyEncryption = false)
    {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Tech", true);
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
            key.Close();
        }
        else
        {
            key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Tech");
            if (ApplyEncryption)
            {
                key.SetValue(Encrypt(skey), Encrypt(svalue));
            }
            else
            {
                key.SetValue(skey, svalue);
            }
            key.Close();
        }
    }
    private static string Encrypt(string clearText)
    {
        string EncryptionKey = "MAKV2SPBNI99212";
        byte[] salt = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

        DeriveKeyAndIV(EncryptionKey, salt, out var key, out var iv);

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
    private static string Decrypt(string cipherText)
    {
        string EncryptionKey = "MAKV2SPBNI99212";
        byte[] salt = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        DeriveKeyAndIV(EncryptionKey, salt, out var key, out var iv);

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
    private static void DeriveKeyAndIV(string password, byte[] salt, out byte[] key, out byte[] iv)
    {
        byte[] derived = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: 1000,
            hashAlgorithm: HashAlgorithmName.SHA1,
            outputLength: 48   // 32 key + 16 IV
        );

        key = derived[..32];
        iv = derived[32..48];
    }
}
