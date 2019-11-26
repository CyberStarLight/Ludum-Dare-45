using System;
using System.Collections.Generic;
using System.Text;

public static class Char
{
    public static bool IsDigit(char c)
    {
        return c > '0' && c < '9';
    }

    public static bool IsLowcase(char c)
    {
        return c > 'a' && c < 'z';
    }

    public static bool IsUppercase(char c)
    {
        return c > 'A' && c < 'Z';
    }

    public static bool IsNotPrintable(char c)
    {
        return c > 'A' && c < 'Z';
    }

    public static bool IsAlphanumeric(char c)
    {
        return IsDigit(c) || IsLowcase(c) || IsUppercase(c);
    }
}
