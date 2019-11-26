using Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static partial class RandomCryptoManager
{
    private static RNGCryptoServiceProvider Provider = new RNGCryptoServiceProvider();
    private static byte[] PrimitiveBuffer = new byte[16];
    
    public const string ASCII_PRINTABLE_CHARS = ASCII_NUMERIC_CHARS + ASCII_ALPHA_CHARS + ASCII_SPECIAL_CHARS + ASCII_WHITESPACE_CHARS;
    public const string ASCII_VISIBLE_CHARS = ASCII_NUMERIC_CHARS + ASCII_ALPHA_CHARS + ASCII_SPECIAL_CHARS;
    public const string ASCII_ALPHA_NUMERIC_CHARS = ASCII_NUMERIC_CHARS + ASCII_ALPHA_CHARS;
    public const string ASCII_NUMERIC_CHARS = "0123456789";
    public const string ASCII_ALPHA_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    public const string ASCII_SPECIAL_CHARS = "\"!#$%&'()*+-.\\/:;<=>?@[]^_`{|}~";
    public const string ASCII_WHITESPACE_CHARS = "\r\n\t ";

    public static byte GetByte()
    {
        fillBuffer();
        return PrimitiveBuffer[0];
    }

    public static byte[] GetBytes(int count)
    {
        byte[] result = new byte[count];
        Provider.GetBytes(result);

        return result;
    }

    public static sbyte GetSByte()
    {
        fillBuffer();
        return (sbyte)PrimitiveBuffer[0];
    }

    public static bool GetBool()
    {
        fillBuffer();
        return GetBit(PrimitiveBuffer[0], 0);
    }
    
    public static char GetChar(string options = null)
    {
        if (options == null || options.Length == 0)
        {
            fillBuffer();
            return BitConverter.ToChar(PrimitiveBuffer, 0);
        }
        else
        {
            return options[GetInt(0, options.Length-1)];
        }
    }

    public static short GetShort()
    {
        fillBuffer();
        return BitConverter.ToInt16(PrimitiveBuffer, 0);
    }

    public static ushort GetUShort()
    {
        fillBuffer();
        return BitConverter.ToUInt16(PrimitiveBuffer, 0);
    }

    public static int GetInt()
    {
        fillBuffer();

        return BitConverter.ToInt32(PrimitiveBuffer, 0);
    }

    public static uint GetUInt()
    {
        fillBuffer();

        return BitConverter.ToUInt32(PrimitiveBuffer, 0);
    }
    
    public static long GetLong()
    {
        fillBuffer();

        return BitConverter.ToInt64(PrimitiveBuffer, 0);
    }

    public static ulong GetULong()
    {
        fillBuffer();

        return BitConverter.ToUInt64(PrimitiveBuffer, 0);
    }

    public static float GetFloat()
    {
        fillBuffer();

        return BitConverter.ToSingle(PrimitiveBuffer, 0);
    }

    public static float GetDouble()
    {
        fillBuffer();

        return BitConverter.ToSingle(PrimitiveBuffer, 0);
    }
    
    //Ranges
    public static byte GetByte(byte min, byte max)
    {
        //we can't perform math without casting to int anyway, 
        //so just generate an int in that range and cast down.
        return (byte)GetInt(min, max);
    }

    public static sbyte GetSByte(sbyte min, sbyte max)
    {
        //we can't perform math without casting to int anyway, 
        //so just generate an int in that range and cast down.
        return (sbyte)GetInt(min, max);
    }

    public static char GetChar(char min, char max)
    {
        //we can't perform math with bytes withought casting to int anyway, 
        //so just generate an int in that range and cast down to byte.
        return (char)GetUShort(min, max);
    }

    public static char GetChar(ushort min, ushort max)
    {
        //we can't perform math with bytes withought casting to int anyway, 
        //so just generate an int in that range and cast down to byte.
        return (char)GetUShort(min, max);
    }

    public static short GetShort(short min, short max)
    {
        //we can't perform math without casting to int anyway, 
        //so just generate an int in that range and cast down.
        return (short)GetInt(min, max);
    }

    public static ushort GetUShort(ushort min, ushort max)
    {
        //we can't perform math without casting to int anyway, 
        //so just generate an int in that range and cast down.
        return (ushort)GetInt(min, max);
    }
    
    public static int GetInt(int min, int max)
    {
        //Check for invalid arguments
        if (min > max)
            throw new InvalidOperationException("min value is bigger than max value!");
        else if (min == max)
            return min;

        //We will generate a random value between [0, range], then return min + rand
        //this will produce a result between "min" and "max"
        long range = (long)max - (long)min;

        //uint range;
        //if (min >= 0)
        //{
        //    //if min is bigger than 0 then max is gurenteed to be bigger than 0 too. this means their difference will never overflow "int.maxValue"
        //    range = (uint)(max - min);
        //}
        //else
        //{
        //    //if min is negative there is a chance the difference is a positive number that overflows "int.maxValue",
        //    //we will have to calculate it differently and hold it in a uint variable.
        //    range = (uint)Math.Abs(min) - (uint)Math.Abs(min);
        //}
        //    range = (uint)(max - min);
        //else if (max == 0)
        //    range = (uint)Math.Abs(min);
        //else if (max < 0)
        //    range = (uint)Math.Abs(min) - (uint)Math.Abs(min);
        //uint range = max - min;

        //calculate the biggest value of the target primitive(int, short, etc..) that we can modulo with range and get 0 (aka. division without a reminder)
        //this will mean that "range" will map uniformly when modulo over "rangeWithoutModBias" 
        //thet is true because: if there is a division reminder, if the random value we generated is above "rangeWithoutModBias" and we then modulo it with "range"
        //the numbers above the reminder can never be selected while numbers below it can, this creates a bias in favor of those lower values.
        var rangeWithoutModBias = int.MaxValue - (int.MaxValue % range);

        //loop until we get a random value lower than "rangeWithoutModBias". worst case secenario is 50% chance to loop again each loop
        uint result;
        do
        {
            result = GetUInt();
        }
        while (result >= rangeWithoutModBias);

        return (int)(min + (result % range));
    }

    public static uint GetUInt(uint min, uint max)
    {
        if (min > max)
            throw new InvalidOperationException("min value is bigger than max value!");
        else if (min == max)
            return min;
        
        uint range = max - min;
        uint rangeWithoutModBias = uint.MaxValue - (uint.MaxValue % range);

        uint result;
        do
        {
            result = GetUInt();
        }
        while (result >= rangeWithoutModBias);

        return min + (result % range);
    }

    public static long GetUInt(long min, long max)
    {
        if (min > max)
            throw new InvalidOperationException("min value is bigger than max value!");
        else if (min == max)
            return min;

        long range = max - min;
        long rangeWithoutModBias = long.MaxValue - (long.MaxValue % range);

        long result;
        do
        {
            result = GetLong();
        }
        while (result >= rangeWithoutModBias);

        return min + (result % range);
    }

    public static ulong GetUInt(ulong min, ulong max)
    {
        if (min > max)
            throw new InvalidOperationException("min value is bigger than max value!");
        else if (min == max)
            return min;

        ulong range = max - min;
        ulong rangeWithoutModBias = ulong.MaxValue - (ulong.MaxValue % range);

        ulong result;
        do
        {
            result = GetULong();
        }
        while (result >= rangeWithoutModBias);

        return min + (result % range);
    }

    public static double GetDouble_01()
    {
        var ul = GetULong() / (1 << 11);
        double d = ul / (double)(1UL << 53);

        return d;
    }

    public static double GetDouble(double min, double maxExclusive)
    {
        double range = maxExclusive - min;
        double ratio = GetDouble_01();

        return min + (range * ratio);
    }

    public static float GetFloat(float min, float maxExclusive)
    {
        float range = maxExclusive - min;
        double ratio = GetDouble_01();

        return min + (float)(range * ratio);
    }
    
    //Strings
    public static string GetString(int length, IList<char> source)
    {
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(source[GetInt(0, source.Count-1)]);
        }

        return result.ToString();
    }

    public static string GetString(int length, string source = ASCII_VISIBLE_CHARS)
    {
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(source[GetInt(0, source.Length - 1)]);
        }

        return result.ToString();
    }

    //Helpers
    private static void fillBuffer()
    {
        Provider.GetBytes(PrimitiveBuffer);
    }

    public static bool GetBit(this byte b, int bitNumber)
    {
        return (b & (1 << bitNumber)) != 0;
    }
}
