using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MD5provider = System.Security.Cryptography.MD5;
using SHA256provider = System.Security.Cryptography.SHA256;

public class HashManager
{
    public static string MD5(string str)
    {
        using (var provider = MD5provider.Create())
        {
            byte[] data = provider.ComputeHash(Encoding.UTF8.GetBytes(str));

            return Utilities.ToHEX(data);
        }
    }

    public static string SHA256(string str)
    {
        using (var provider = SHA256provider.Create())
        {
            byte[] data = provider.ComputeHash(Encoding.UTF8.GetBytes(str));

            return Utilities.ToHEX(data);
        }
    }
}

