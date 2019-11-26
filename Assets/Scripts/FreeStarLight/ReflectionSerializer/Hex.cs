using System;
using System.Collections.Generic;
using System.Text;

public static class Hex
{
    public static string From(byte[] data)
    {
        StringBuilder sBuilder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("X2"));
        }

        return sBuilder.ToString();
    }

    public static byte[] ToBytes(string str)
    {
        if (str.Length == 0 || str.Length % 2 != 0)
            return new byte[0];

        byte[] buffer = new byte[str.Length / 2];
        char c;
        for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
        {
            // Convert first half of byte
            c = str[sx];
            buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

            // Convert second half of byte
            c = str[++sx];
            buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
        }

        return buffer;
    }

}

