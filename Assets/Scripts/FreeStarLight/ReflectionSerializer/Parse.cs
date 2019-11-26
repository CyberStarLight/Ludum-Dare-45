using System;
using System.Collections.Generic;
using System.Text;

public static class Parse
{
    public static byte Byte_Hex(string singleHex)
    {
        if (
            singleHex == null ||
            singleHex.Length < 2 ||
            !Char.IsAlphanumeric(singleHex[0]) ||
            !Char.IsAlphanumeric(singleHex[1])
            )
            return 0;

        char c = singleHex[0];
        byte result = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

        c = singleHex[1];
        result |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
        return result;
    }

    public static byte Byte(string str, byte defaultValue = default(byte))
    {
        if (byte.TryParse(str, out byte result))
            return result;
        else
            return defaultValue;
    }

    public static sbyte SByte(string str, sbyte defaultValue = default(sbyte))
    {
        if (sbyte.TryParse(str, out sbyte result))
            return result;
        else
            return defaultValue;
    }

    public static bool Bool(string str, bool defaultValue = default(bool))
    {
        if (String.IsNullOrEmpty(str))
            return defaultValue;

        string lower = str.ToLower();
        return (lower == "true" || lower == "yes" || lower == "on" || lower == "1");
    }

    public static short Short(string str, short defaultValue = default(short))
    {
        if (short.TryParse(str, out short result))
            return result;
        else
            return defaultValue;
    }

    public static ushort UShort(string str, ushort defaultValue = default(ushort))
    {
        if (ushort.TryParse(str, out ushort result))
            return result;
        else
            return defaultValue;
    }

    public static int Int(string str, int defaultValue = default(int))
    {
        if (int.TryParse(str, out int result))
            return result;
        else
            return defaultValue;
    }

    public static uint UInt(string str, uint defaultValue = default(uint))
    {
        if (uint.TryParse(str, out uint result))
            return result;
        else
            return defaultValue;
    }

    public static long Long(string str, long defaultValue = default(long))
    {
        if (long.TryParse(str, out long result))
            return result;
        else
            return defaultValue;
    }

    public static ulong ULong(string str, ulong defaultValue = default(ulong))
    {
        if (ulong.TryParse(str, out ulong result))
            return result;
        else
            return defaultValue;
    }

    public static float Float(string str, float defaultValue = default(float))
    {
        if (float.TryParse(str, out float result))
            return result;
        else
            return defaultValue;
    }

    public static double Double(string str, double defaultValue = default(double))
    {
        if (double.TryParse(str, out double result))
            return result;
        else
            return defaultValue;
    }

    public static decimal Decimal(string str, decimal defaultValue = default(decimal))
    {
        if (decimal.TryParse(str, out decimal result))
            return result;
        else
            return defaultValue;
    }
}
