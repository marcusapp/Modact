using System.Security.Cryptography;
using System.Text;

public class EncryptSymmetric
{
    public static string EncryptToBase64(string plainText, string key)
    {
        return EncryptToBase64(Encoding.UTF8.GetBytes(plainText), key);
    }
    public static string EncryptToBase64(byte[] plainData, string key)
    {
        byte[] keyBytes;
        using (var sha256 = new SHA256CryptoServiceProvider())
        {
            keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
        return EncryptToBase64(plainData, keyBytes);
    }
    public static string EncryptToBase64(string plainText, byte[] key)
    {
        return EncryptToBase64(Encoding.UTF8.GetBytes(plainText), key);
    }
    public static string EncryptToBase64(byte[] plainData, byte[] key)
    {
        return Convert.ToBase64String(Encrypt(plainData, key));
    }
    public static byte[] Encrypt(string plainText, string key)
    {
        return Encrypt(Encoding.UTF8.GetBytes(plainText), key);
    }
    public static byte[] Encrypt(byte[] plainData, string key)
    {
        byte[] keyBytes;
        using (var sha256 = new SHA256CryptoServiceProvider())
        {
            keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
        return Encrypt(plainData, keyBytes);
    }
    public static byte[] Encrypt(string plainText, byte[] key)
    {
        return Encrypt(Encoding.UTF8.GetBytes(plainText), key);
    }
    public static byte[] Encrypt(byte[] plainData, byte[] key)
    {
        byte[] iv = Guid.NewGuid().ToByteArray();
        using (var aes = new AesCryptoServiceProvider())
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainData, 0, plainData.Length);
                    cs.FlushFinalBlock();
                    byte[] encryptedByteArray = ms.ToArray();
                    byte[] outputByteArray = new byte[encryptedByteArray.Length + iv.Length];
                    Buffer.BlockCopy(iv, 0, outputByteArray, 0, iv.Length);
                    Buffer.BlockCopy(encryptedByteArray, 0, outputByteArray, iv.Length, encryptedByteArray.Length);
                    return outputByteArray;
                }
            }
        }
    }
    public static string DecryptToString(string base64CipherText, string key)
    {
        return DecryptToString(Convert.FromBase64String(base64CipherText), key);
    }
    public static string DecryptToString(byte[] cipherData, string key)
    {
        byte[] keyBytes;
        using (var sha256 = new SHA256CryptoServiceProvider())
        {
            keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
        return DecryptToString(cipherData, keyBytes);
    }
    public static string DecryptToString(string base64CipherText, byte[] key)
    {
        return DecryptToString(Convert.FromBase64String(base64CipherText), key);
    }
    public static string DecryptToString(byte[] cipherData, byte[] key)
    {
        return Encoding.UTF8.GetString(Decrypt(cipherData, key));
    }
    public static byte[] Decrypt(string base64CipherText, string key)
    {
        return Decrypt(Convert.FromBase64String(base64CipherText), key);
    }
    public static byte[] Decrypt(byte[] cipherData, string key)
    {
        byte[] keyBytes;
        using (var sha256 = new SHA256CryptoServiceProvider())
        {
            keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
        return Decrypt(cipherData, keyBytes);
    }
    public static byte[] Decrypt(string base64CipherText, byte[] key)
    {
        return Decrypt(Convert.FromBase64String(base64CipherText), key);
    }
    private static byte[] Decrypt(byte[] cipherData, byte[] key)
    {
        byte[] iv = new byte[16];
        byte[] valueData = new byte[cipherData.Length - iv.Length];
        Buffer.BlockCopy(cipherData, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(cipherData, iv.Length, valueData, 0, cipherData.Length - iv.Length);
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(valueData, 0, valueData.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }
    }
}