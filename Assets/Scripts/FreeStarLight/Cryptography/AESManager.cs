using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class AES
{
    public static byte[] SALT = Encoding.UTF8.GetBytes("6oZRd4PLGIERvO1zGm20");
    public const int ITERATIONS = 549;
    public const int IV_SIZE = 16;
    public const int KEY_SIZE = 32;

    public static byte[] DeriveIVFromSeed(byte[] ivSeed)
    {
        Rfc2898DeriveBytes deriver = new Rfc2898DeriveBytes(ivSeed, SALT, ITERATIONS);
        return deriver.GetBytes(IV_SIZE);
    }
    public static byte[] DeriveKeyFromSeed(byte[] keySeed)
    {
        Rfc2898DeriveBytes deriver = new Rfc2898DeriveBytes(keySeed, SALT, ITERATIONS);
        return deriver.GetBytes(KEY_SIZE);
    }

    //Encryption
    public static byte[] Encrypt(byte[] data, byte[] keySeed, byte[] ivSeed)
    {
        if (data == null)
            throw new ArgumentNullException("data");
        if (data.Length == 0)
            return new byte[0];

        if (keySeed == null)
            throw new ArgumentNullException("keySeed");
        if (keySeed.Length == 0)
            throw new ArgumentException("keySeed length is zero!", "keySeed");

        if (ivSeed == null)
            throw new ArgumentNullException("IvSeed");
        if (ivSeed.Length == 0)
            throw new ArgumentException("IvSeed length is zero!", "IV");

        byte[] Key = DeriveKeyFromSeed(keySeed);
        byte[] IV = DeriveIVFromSeed(ivSeed);

        byte[] encrypted;
        using (RijndaelManaged rijAlg = new RijndaelManaged())
        {
            rijAlg.Key = Key;
            rijAlg.IV = IV;

            // Create a decrytor to perform the stream transform.
            ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(data, 0, data.Length);
                    csEncrypt.FlushFinalBlock();
                }

                encrypted = msEncrypt.ToArray();
            }
        }

        return encrypted;
    }

    public static byte[] Encrypt(byte[] data, string keySeed, string ivSeed)
    {
        return Encrypt(data, Encoding.UTF8.GetBytes(keySeed), Encoding.UTF8.GetBytes(ivSeed));
    }

    public static byte[] Encrypt(byte[] data, string keySeed, byte[] ivSeed)
    {
        return Encrypt(data, Encoding.UTF8.GetBytes(keySeed), ivSeed);
    }

    //Decryption
    public static byte[] Decrypt(byte[] data, byte[] keySeed, byte[] ivSeed)
    {
        if (data == null)
            throw new ArgumentNullException("data");
        if (data.Length == 0)
            return new byte[0];

        if (keySeed == null)
            throw new ArgumentNullException("keySeed");
        if (keySeed.Length == 0)
            throw new ArgumentException("keySeed length is zero!", "keySeed");

        if (ivSeed == null)
            throw new ArgumentNullException("IvSeed");
        if (ivSeed.Length == 0)
            throw new ArgumentException("IvSeed length is zero!", "IV");

        byte[] Key = DeriveKeyFromSeed(keySeed);
        byte[] IV = DeriveIVFromSeed(ivSeed);

        byte[] decrypted;
        using (RijndaelManaged rijAlg = new RijndaelManaged())
        {
            rijAlg.Key = Key;
            rijAlg.IV = IV;

            // Create a decrytor to perform the stream transform.
            ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(data))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    decrypted = csDecrypt.ReadAll();
                }
            }
        }

        return decrypted;
    }

    public static byte[] Decrypt(byte[] data, string keySeed, string ivSeed)
    {
        return Decrypt(data, Encoding.UTF8.GetBytes(keySeed), Encoding.UTF8.GetBytes(ivSeed));
    }

    public static byte[] Decrypt(byte[] data, string keySeed, byte[] ivSeed)
    {
        return Decrypt(data, Encoding.UTF8.GetBytes(keySeed), ivSeed);
    }
}

