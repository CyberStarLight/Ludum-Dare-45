using System;
using System.Collections.Generic;
using System.Text;
using MD5provider = System.Security.Cryptography.MD5;
using SHA256provider = System.Security.Cryptography.SHA256;

public static class Hash
{
    public static byte[] MD5(byte[] bytes)
    {
        using (var provider = MD5provider.Create())
        {
            return provider.ComputeHash(bytes);
        }
    }
    public static byte[] MD5(string str)
    {
        return MD5(Encoding.UTF8.GetBytes(str));
    }

    public static string MD5_Hex(byte[] bytes)
    {
        byte[] hash = MD5(bytes);
        return Hex.From(hash);
    }
    public static string MD5_Hex(string str)
    {
        return MD5_Hex(Encoding.UTF8.GetBytes(str));
    }

    public static string MD5_Base64(byte[] bytes)
    {
        byte[] hash = MD5(bytes);
        return Convert.ToBase64String(hash);
    }
    public static string MD5_Base64(string str)
    {
        return MD5_Base64(Encoding.UTF8.GetBytes(str));
    }

    public static string SHA256(string str)
    {
        using (var provider = SHA256provider.Create())
        {
            byte[] data = provider.ComputeHash(Encoding.UTF8.GetBytes(str));

            return Hex.From(data);
        }
    }
}
