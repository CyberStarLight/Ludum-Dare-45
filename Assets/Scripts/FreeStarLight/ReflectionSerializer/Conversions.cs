using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

public static class Conversions
{
    public static Dictionary<Type, Dictionary<Type, Func<object, object>>> reflectionConversionDic = new Dictionary<Type, Dictionary<Type, Func<object, object>>>() {
        {
            typeof(byte),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => x },
                { typeof(sbyte), x => ToSbyte((byte)x) },
                { typeof(bool), x => ToBool((byte)x) },
                { typeof(char), x => ToChar((byte)x) },
                { typeof(short), x => ToShort((byte)x) },
                { typeof(ushort), x => ToUShort((byte)x) },
                { typeof(int), x => ToInt((byte)x) },
                { typeof(uint), x => ToUInt((byte)x) },
                { typeof(long), x => ToLong((byte)x) },
                { typeof(ulong), x => ToULong((byte)x) },
                { typeof(float), x => ToFloat((byte)x) },
                { typeof(double), x => ToDouble((byte)x) },
                { typeof(decimal), x => ToDecimal((byte)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(sbyte),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((sbyte)x) },
                { typeof(sbyte), x => x },
                { typeof(bool), x => ToBool((sbyte)x) },
                { typeof(char), x => ToChar((sbyte)x) },
                { typeof(short), x => ToShort((sbyte)x) },
                { typeof(ushort), x => ToUShort((sbyte)x) },
                { typeof(int), x => ToInt((sbyte)x) },
                { typeof(uint), x => ToUInt((sbyte)x) },
                { typeof(long), x => ToLong((sbyte)x) },
                { typeof(ulong), x => ToULong((sbyte)x) },
                { typeof(float), x => ToFloat((sbyte)x) },
                { typeof(double), x => ToDouble((sbyte)x) },
                { typeof(decimal), x => ToDecimal((sbyte)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(bool),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((bool)x) },
                { typeof(sbyte), x => ToSbyte((bool)x) },
                { typeof(bool), x => x },
                { typeof(char), x => ToChar((bool)x) },
                { typeof(short), x => ToShort((bool)x) },
                { typeof(ushort), x => ToUShort((bool)x) },
                { typeof(int), x => ToInt((bool)x) },
                { typeof(uint), x => ToUInt((bool)x) },
                { typeof(long), x => ToLong((bool)x) },
                { typeof(ulong), x => ToULong((bool)x) },
                { typeof(float), x => ToFloat((bool)x) },
                { typeof(double), x => ToDouble((bool)x) },
                { typeof(decimal), x => ToDecimal((bool)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(char),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((char)x) },
                { typeof(sbyte), x => ToSbyte((char)x) },
                { typeof(bool), x => ToBool((char)x) },
                { typeof(char), x => x },
                { typeof(short), x => ToShort((char)x) },
                { typeof(ushort), x => ToUShort((char)x) },
                { typeof(int), x => ToInt((char)x) },
                { typeof(uint), x => ToUInt((char)x) },
                { typeof(long), x => ToLong((char)x) },
                { typeof(ulong), x => ToULong((char)x) },
                { typeof(float), x => ToFloat((char)x) },
                { typeof(double), x => ToDouble((char)x) },
                { typeof(decimal), x => ToDecimal((char)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(short),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((short)x) },
                { typeof(sbyte), x => ToSbyte((short)x) },
                { typeof(bool), x => ToBool((short)x) },
                { typeof(char), x => ToChar((short)x) },
                { typeof(short), x => x },
                { typeof(ushort), x => ToUShort((short)x) },
                { typeof(int), x => ToInt((short)x) },
                { typeof(uint), x => ToUInt((short)x) },
                { typeof(long), x => ToLong((short)x) },
                { typeof(ulong), x => ToULong((short)x) },
                { typeof(float), x => ToFloat((short)x) },
                { typeof(double), x => ToDouble((short)x) },
                { typeof(decimal), x => ToDecimal((short)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(ushort),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((ushort)x) },
                { typeof(sbyte), x => ToSbyte((ushort)x) },
                { typeof(bool), x => ToBool((ushort)x) },
                { typeof(char), x => ToChar((ushort)x) },
                { typeof(short), x => ToShort((ushort)x) },
                { typeof(ushort), x => x },
                { typeof(int), x => ToInt((ushort)x) },
                { typeof(uint), x => ToUInt((ushort)x) },
                { typeof(long), x => ToLong((ushort)x) },
                { typeof(ulong), x => ToULong((ushort)x) },
                { typeof(float), x => ToFloat((ushort)x) },
                { typeof(double), x => ToDouble((ushort)x) },
                { typeof(decimal), x => ToDecimal((ushort)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(int),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((int)x) },
                { typeof(sbyte), x => ToSbyte((int)x) },
                { typeof(bool), x => ToBool((int)x) },
                { typeof(char), x => ToChar((int)x) },
                { typeof(short), x => ToShort((int)x) },
                { typeof(ushort), x => ToUShort((int)x) },
                { typeof(int), x => x },
                { typeof(uint), x => ToUInt((int)x) },
                { typeof(long), x => ToLong((int)x) },
                { typeof(ulong), x => ToULong((int)x) },
                { typeof(float), x => ToFloat((int)x) },
                { typeof(double), x => ToDouble((int)x) },
                { typeof(decimal), x => ToDecimal((int)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(uint),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((uint)x) },
                { typeof(sbyte), x => ToSbyte((uint)x) },
                { typeof(bool), x => ToBool((uint)x) },
                { typeof(char), x => ToChar((uint)x) },
                { typeof(short), x => ToShort((uint)x) },
                { typeof(ushort), x => ToUShort((uint)x) },
                { typeof(int), x => ToInt((uint)x) },
                { typeof(uint), x => x },
                { typeof(long), x => ToLong((uint)x) },
                { typeof(ulong), x => ToULong((uint)x) },
                { typeof(float), x => ToFloat((uint)x) },
                { typeof(double), x => ToDouble((uint)x) },
                { typeof(decimal), x => ToDecimal((uint)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(long),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((long)x) },
                { typeof(sbyte), x => ToSbyte((long)x) },
                { typeof(bool), x => ToBool((long)x) },
                { typeof(char), x => ToChar((long)x) },
                { typeof(short), x => ToShort((long)x) },
                { typeof(ushort), x => ToUShort((long)x) },
                { typeof(int), x => ToInt((long)x) },
                { typeof(uint), x => ToUInt((long)x) },
                { typeof(long), x => x },
                { typeof(ulong), x => ToULong((long)x) },
                { typeof(float), x => ToFloat((long)x) },
                { typeof(double), x => ToDouble((long)x) },
                { typeof(decimal), x => ToDecimal((long)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(ulong),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((ulong)x) },
                { typeof(sbyte), x => ToSbyte((ulong)x) },
                { typeof(bool), x => ToBool((ulong)x) },
                { typeof(char), x => ToChar((ulong)x) },
                { typeof(short), x => ToShort((ulong)x) },
                { typeof(ushort), x => ToUShort((ulong)x) },
                { typeof(int), x => ToInt((ulong)x) },
                { typeof(uint), x => ToUInt((ulong)x) },
                { typeof(long), x => ToLong((ulong)x) },
                { typeof(ulong), x => x },
                { typeof(float), x => ToFloat((ulong)x) },
                { typeof(double), x => ToDouble((ulong)x) },
                { typeof(decimal), x => ToDecimal((ulong)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(float),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((float)x) },
                { typeof(sbyte), x => ToSbyte((float)x) },
                { typeof(bool), x => ToBool((float)x) },
                { typeof(char), x => ToChar((float)x) },
                { typeof(short), x => ToShort((float)x) },
                { typeof(ushort), x => ToUShort((float)x) },
                { typeof(int), x => ToInt((float)x) },
                { typeof(uint), x => ToUInt((float)x) },
                { typeof(long), x => ToLong((float)x) },
                { typeof(ulong), x => ToULong((float)x) },
                { typeof(float), x => x },
                { typeof(double), x => ToDouble((float)x) },
                { typeof(decimal), x => ToDecimal((float)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(double),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((double)x) },
                { typeof(sbyte), x => ToSbyte((double)x) },
                { typeof(bool), x => ToBool((double)x) },
                { typeof(char), x => ToChar((double)x) },
                { typeof(short), x => ToShort((double)x) },
                { typeof(ushort), x => ToUShort((double)x) },
                { typeof(int), x => ToInt((double)x) },
                { typeof(uint), x => ToUInt((double)x) },
                { typeof(long), x => ToLong((double)x) },
                { typeof(ulong), x => ToULong((double)x) },
                { typeof(float), x => ToFloat((double)x) },
                { typeof(double), x => x },
                { typeof(decimal), x => ToDecimal((double)x) },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(decimal),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((decimal)x) },
                { typeof(sbyte), x => ToSbyte((decimal)x) },
                { typeof(bool), x => ToBool((decimal)x) },
                { typeof(char), x => ToChar((decimal)x) },
                { typeof(short), x => ToShort((decimal)x) },
                { typeof(ushort), x => ToUShort((decimal)x) },
                { typeof(int), x => ToInt((decimal)x) },
                { typeof(uint), x => ToUInt((decimal)x) },
                { typeof(long), x => ToLong((decimal)x) },
                { typeof(ulong), x => ToULong((decimal)x) },
                { typeof(float), x => ToFloat((decimal)x) },
                { typeof(double), x => ToDouble((decimal)x) },
                { typeof(decimal), x => x },
                { typeof(string), x => x.ToString() },
            }
        },
        {
            typeof(string),
            new Dictionary<Type, Func<object, object>>() {
                { typeof(byte), x => ToByte((string)x) },
                { typeof(sbyte), x => ToSbyte((string)x) },
                { typeof(bool), x => ToBool((string)x) },
                { typeof(char), x => ToChar((string)x) },
                { typeof(short), x => ToShort((string)x) },
                { typeof(ushort), x => ToUShort((string)x) },
                { typeof(int), x => ToInt((string)x) },
                { typeof(uint), x => ToUInt((string)x) },
                { typeof(long), x => ToLong((string)x) },
                { typeof(ulong), x => ToULong((string)x) },
                { typeof(float), x => ToFloat((string)x) },
                { typeof(double), x => ToDouble((string)x) },
                { typeof(decimal), x => ToDecimal((string)x) },
                { typeof(string), x => x },
            }
        },
    };

    //Byte
    public static byte ToByte(byte data) { return data; }
    public static sbyte ToSbyte(byte data) { return data <= sbyte.MaxValue ? Convert.ToSByte(data) : sbyte.MaxValue; }
    public static bool ToBool(byte data) { return Convert.ToBoolean(data); }
    public static char ToChar(byte data) { return Convert.ToChar(data); }
    public static short ToShort(byte data) { return Convert.ToInt16(data); }
    public static ushort ToUShort(byte data) { return Convert.ToUInt16(data); }
    public static int ToInt(byte data) { return Convert.ToInt32(data); }
    public static uint ToUInt(byte data) { return Convert.ToUInt32(data); }
    public static long ToLong(byte data) { return Convert.ToInt64(data); }
    public static ulong ToULong(byte data) { return Convert.ToUInt64(data); }
    public static float ToFloat(byte data) { return Convert.ToSingle(data); }
    public static double ToDouble(byte data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(byte data) { return Convert.ToDecimal(data); }
    public static string ToString(byte data) { return data.ToString(); }

    //SByte
    public static byte ToByte(sbyte data) { return data < byte.MinValue ? byte.MinValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(sbyte data) { return data; }
    public static bool ToBool(sbyte data) { return Convert.ToBoolean(data); }
    public static char ToChar(sbyte data) { return data < 0 ? char.MinValue : Convert.ToChar(data); }
    public static short ToShort(sbyte data) { return Convert.ToInt16(data); }
    public static ushort ToUShort(sbyte data) { return data < ushort.MinValue ? ushort.MinValue : Convert.ToUInt16(data); }
    public static int ToInt(sbyte data) { return Convert.ToInt32(data); }
    public static uint ToUInt(sbyte data) { return data < uint.MinValue ? uint.MinValue : Convert.ToUInt32(data); }
    public static long ToLong(sbyte data) { return Convert.ToInt64(data); }
    public static ulong ToULong(sbyte data) { return data < 0 ? ulong.MinValue : Convert.ToUInt64(data); }
    public static float ToFloat(sbyte data) { return Convert.ToSingle(data); }
    public static double ToDouble(sbyte data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(sbyte data) { return Convert.ToDecimal(data); }
    public static string ToString(sbyte data) { return data.ToString(); }

    //Bool
    public static byte ToByte(bool data) { return data ? (byte)1 : (byte)0; }
    public static sbyte ToSbyte(bool data) { return data ? (sbyte)1 : (sbyte)0; }
    public static bool ToBool(bool data) { return data; }
    public static char ToChar(bool data) { return data ? (char)1 : (char)0; }
    public static short ToShort(bool data) { return data ? (short)1 : (short)0; }
    public static ushort ToUShort(bool data) { return data ? (ushort)1 : (ushort)0; }
    public static int ToInt(bool data) { return data ? 1 : 0; }
    public static uint ToUInt(bool data) { return data ? 1u : 0u; }
    public static long ToLong(bool data) { return data ? 1L : 0L; }
    public static ulong ToULong(bool data) { return data ? 1UL : 0UL; }
    public static float ToFloat(bool data) { return data ? 1f : 0f; }
    public static double ToDouble(bool data) { return data ? 1d : 0d; }
    public static decimal ToDecimal(bool data) { return data ? 1m : 0m; }
    public static string ToString(bool data) { return data ? "true" : "false"; }

    //Char
    public static byte ToByte(char data) { return data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(char data) { return data <= sbyte.MaxValue ? Convert.ToSByte(data) : sbyte.MaxValue; }
    public static bool ToBool(char data) { return data > 0 ? true : false; }
    public static char ToChar(char data) { return data; }
    public static short ToShort(char data) { return data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(char data) { return Convert.ToUInt16(data); }
    public static int ToInt(char data) { return Convert.ToInt32(data); }
    public static uint ToUInt(char data) { return Convert.ToUInt32(data); }
    public static long ToLong(char data) { return Convert.ToInt64(data); }
    public static ulong ToULong(char data) { return Convert.ToUInt64(data); }
    public static float ToFloat(char data) { return Convert.ToSingle(data); }
    public static double ToDouble(char data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(char data) { return Convert.ToDecimal(data); }
    public static string ToString(char data) { return data.ToString(); }

    //Short
    public static byte ToByte(short data) { return data < byte.MinValue ? byte.MinValue : data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(short data) { return data < sbyte.MinValue ? sbyte.MinValue : data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(short data) { return Convert.ToBoolean(data); }
    public static char ToChar(short data) { return data < 0 ? char.MinValue : Convert.ToChar(data); }
    public static short ToShort(short data) { return data; }
    public static ushort ToUShort(short data) { return data < ushort.MinValue ? ushort.MinValue : Convert.ToUInt16(data); }
    public static int ToInt(short data) { return Convert.ToInt32(data); }
    public static uint ToUInt(short data) { return data < uint.MinValue ? uint.MinValue : Convert.ToUInt32(data); }
    public static long ToLong(short data) { return Convert.ToInt64(data); }
    public static ulong ToULong(short data) { return data < 0 ? ulong.MinValue : Convert.ToUInt64(data); }
    public static float ToFloat(short data) { return Convert.ToSingle(data); }
    public static double ToDouble(short data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(short data) { return Convert.ToDecimal(data); }
    public static string ToString(short data) { return data.ToString(); }

    //UShort
    public static byte ToByte(ushort data) { return data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(ushort data) { return data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(ushort data) { return Convert.ToBoolean(data); }
    public static char ToChar(ushort data) { return data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(ushort data) { return data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(ushort data) { return data; }
    public static int ToInt(ushort data) { return Convert.ToInt32(data); }
    public static uint ToUInt(ushort data) { return Convert.ToUInt32(data); }
    public static long ToLong(ushort data) { return Convert.ToInt64(data); }
    public static ulong ToULong(ushort data) { return Convert.ToUInt64(data); }
    public static float ToFloat(ushort data) { return Convert.ToSingle(data); }
    public static double ToDouble(ushort data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(ushort data) { return Convert.ToDecimal(data); }
    public static string ToString(ushort data) { return data.ToString(); }

    //Int
    public static byte ToByte(int data) { return data < byte.MinValue ? byte.MinValue : data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(int data) { return data < sbyte.MinValue ? sbyte.MinValue : data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(int data) { return Convert.ToBoolean(data); }
    public static char ToChar(int data) { return data < char.MinValue ? char.MinValue : data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(int data) { return data < short.MinValue ? short.MinValue : data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(int data) { return data < ushort.MinValue ? ushort.MinValue : data > ushort.MaxValue ? ushort.MaxValue : Convert.ToUInt16(data); }
    public static int ToInt(int data) { return data; }
    public static uint ToUInt(int data) { return data < uint.MinValue ? uint.MinValue : Convert.ToUInt32(data); }
    public static long ToLong(int data) { return Convert.ToInt64(data); }
    public static ulong ToULong(int data) { return data < 0 ? ulong.MinValue : Convert.ToUInt64(data); }
    public static float ToFloat(int data) { return Convert.ToSingle(data); }
    public static double ToDouble(int data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(int data) { return Convert.ToDecimal(data); }
    public static string ToString(int data) { return data.ToString(); }

    //Uint
    public static byte ToByte(uint data) { return data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(uint data) { return data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(uint data) { return Convert.ToBoolean(data); }
    public static char ToChar(uint data) { return data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(uint data) { return data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(uint data) { return data > ushort.MaxValue ? ushort.MaxValue : Convert.ToUInt16(data); }
    public static int ToInt(uint data) { return data > int.MaxValue ? int.MaxValue : Convert.ToInt32(data); }
    public static uint ToUInt(uint data) { return data; }
    public static long ToLong(uint data) { return Convert.ToInt64(data); }
    public static ulong ToULong(uint data) { return Convert.ToUInt64(data); }
    public static float ToFloat(uint data) { return Convert.ToSingle(data); }
    public static double ToDouble(uint data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(uint data) { return Convert.ToDecimal(data); }
    public static string ToString(uint data) { return data.ToString(); }

    //Long
    public static byte ToByte(long data) { return data < byte.MinValue ? byte.MinValue : data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(long data) { return data < sbyte.MinValue ? sbyte.MinValue : data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(long data) { return Convert.ToBoolean(data); }
    public static char ToChar(long data) { return data < char.MinValue ? char.MinValue : data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(long data) { return data < short.MinValue ? short.MinValue : data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(long data) { return data < ushort.MinValue ? ushort.MinValue : data > ushort.MaxValue ? ushort.MaxValue : Convert.ToUInt16(data); }
    public static int ToInt(long data) { return data < int.MinValue ? int.MinValue : data > int.MaxValue ? int.MaxValue : Convert.ToInt32(data); }
    public static uint ToUInt(long data) { return data < uint.MinValue ? uint.MinValue : data > uint.MaxValue ? uint.MaxValue : Convert.ToUInt32(data); }
    public static long ToLong(long data) { return data; }
    public static ulong ToULong(long data) { return data < 0L ? ulong.MinValue : Convert.ToUInt64(data); }
    public static float ToFloat(long data) { return Convert.ToSingle(data); }
    public static double ToDouble(long data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(long data) { return Convert.ToDecimal(data); }
    public static string ToString(long data) { return data.ToString(); }

    //ULong
    public static byte ToByte(ulong data) { return data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(ulong data) { return data > 127UL ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(ulong data) { return Convert.ToBoolean(data); }
    public static char ToChar(ulong data) { return data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(ulong data) { return data > 32767UL ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(ulong data) { return data > ushort.MaxValue ? ushort.MaxValue : Convert.ToUInt16(data); }
    public static int ToInt(ulong data) { return data > int.MaxValue ? int.MaxValue : Convert.ToInt32(data); }
    public static uint ToUInt(ulong data) { return data > uint.MaxValue ? uint.MaxValue : Convert.ToUInt32(data); }
    public static long ToLong(ulong data) { return data > long.MaxValue ? long.MaxValue : Convert.ToInt64(data); }
    public static ulong ToULong(ulong data) { return data; }
    public static float ToFloat(ulong data) { return Convert.ToSingle(data); }
    public static double ToDouble(ulong data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(ulong data) { return Convert.ToDecimal(data); }
    public static string ToString(ulong data) { return data.ToString(); }

    //Float
    public static byte ToByte(float data) { return data < byte.MinValue ? byte.MinValue : data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(float data) { return data < sbyte.MinValue ? sbyte.MinValue : data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(float data) { return Convert.ToBoolean(data); }
    public static char ToChar(float data) { return data < char.MinValue ? char.MinValue : data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(float data) { return data < short.MinValue ? short.MinValue : data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(float data) { return data < ushort.MinValue ? ushort.MinValue : data > ushort.MaxValue ? ushort.MaxValue : Convert.ToUInt16(data); }
    public static int ToInt(float data) { return data < int.MinValue ? int.MinValue : data > int.MaxValue ? int.MaxValue : Convert.ToInt32(data); }
    public static uint ToUInt(float data) { return data < uint.MinValue ? uint.MinValue : data > uint.MaxValue ? uint.MaxValue : Convert.ToUInt32(data); }
    public static long ToLong(float data) { return data < long.MinValue ? long.MinValue : data > long.MaxValue ? long.MaxValue : Convert.ToInt64(data); }
    public static ulong ToULong(float data) { return data < ulong.MinValue ? ulong.MinValue : data > ulong.MaxValue ? ulong.MaxValue : Convert.ToUInt64(data); }
    public static float ToFloat(float data) { return data; }
    public static double ToDouble(float data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(float data) { return data < (float)decimal.MinValue ? decimal.MinValue : data > (float)decimal.MaxValue ? decimal.MaxValue : Convert.ToDecimal(data); }
    public static string ToString(float data) { return data.ToString(); }

    //Double
    public static byte ToByte(double data) { return data < byte.MinValue ? byte.MinValue : data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(double data) { return data < sbyte.MinValue ? sbyte.MinValue : data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(double data) { return Convert.ToBoolean(data); }
    public static char ToChar(double data) { return data < char.MinValue ? char.MinValue : data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(double data) { return data < short.MinValue ? short.MinValue : data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(double data) { return data < ushort.MinValue ? ushort.MinValue : data > ushort.MaxValue ? ushort.MaxValue : Convert.ToUInt16(data); }
    public static int ToInt(double data) { return data < int.MinValue ? int.MinValue : data > int.MaxValue ? int.MaxValue : Convert.ToInt32(data); }
    public static uint ToUInt(double data) { return data < uint.MinValue ? uint.MinValue : data > uint.MaxValue ? uint.MaxValue : Convert.ToUInt32(data); }
    public static long ToLong(double data) { return data < long.MinValue ? long.MinValue : data > long.MaxValue ? long.MaxValue : Convert.ToInt64(data); }
    public static ulong ToULong(double data) { return data < ulong.MinValue ? ulong.MinValue : data > ulong.MaxValue ? ulong.MaxValue : Convert.ToUInt64(data); }
    public static float ToFloat(double data) { return data < float.MinValue ? float.MinValue : data > float.MaxValue ? float.MaxValue : Convert.ToSingle(data); }
    public static double ToDouble(double data) { return data; }
    public static decimal ToDecimal(double data) { return data < (double)decimal.MinValue ? decimal.MinValue : data > (double)decimal.MaxValue ? decimal.MaxValue : Convert.ToDecimal(data); }
    public static string ToString(double data) { return data.ToString(); }

    //Decimal
    public static byte ToByte(decimal data) { return data < byte.MinValue ? byte.MinValue : data > byte.MaxValue ? byte.MaxValue : Convert.ToByte(data); }
    public static sbyte ToSbyte(decimal data) { return data < sbyte.MinValue ? sbyte.MinValue : data > sbyte.MaxValue ? sbyte.MaxValue : Convert.ToSByte(data); }
    public static bool ToBool(decimal data) { return Convert.ToBoolean(data); }
    public static char ToChar(decimal data) { return data < char.MinValue ? char.MinValue : data > char.MaxValue ? char.MaxValue : Convert.ToChar(data); }
    public static short ToShort(decimal data) { return data < short.MinValue ? short.MinValue : data > short.MaxValue ? short.MaxValue : Convert.ToInt16(data); }
    public static ushort ToUShort(decimal data) { return data < ushort.MinValue ? ushort.MinValue : data > ushort.MaxValue ? ushort.MaxValue : Convert.ToUInt16(data); }
    public static int ToInt(decimal data) { return data < int.MinValue ? int.MinValue : data > int.MaxValue ? int.MaxValue : Convert.ToInt32(data); }
    public static uint ToUInt(decimal data) { return data < uint.MinValue ? uint.MinValue : data > uint.MaxValue ? uint.MaxValue : Convert.ToUInt32(data); }
    public static long ToLong(decimal data) { return data < long.MinValue ? long.MinValue : data > long.MaxValue ? long.MaxValue : Convert.ToInt64(data); }
    public static ulong ToULong(decimal data) { return data < ulong.MinValue ? ulong.MinValue : data > ulong.MaxValue ? ulong.MaxValue : Convert.ToUInt64(data); }
    public static float ToFloat(decimal data) { return Convert.ToSingle(data); }
    public static double ToDouble(decimal data) { return Convert.ToDouble(data); }
    public static decimal ToDecimal(decimal data) { return data; }
    public static string ToString(decimal data) { return data.ToString(); }

    //String
    public static byte ToByte(string data) { return Parse.Byte(data); }
    public static sbyte ToSbyte(string data) { return Parse.SByte(data); }
    public static bool ToBool(string data) { return Parse.Bool(data); }
    public static char ToChar(string data) { return String.IsNullOrEmpty(data) ? default(char) : data[0]; }
    public static short ToShort(string data) { return Parse.Short(data); }
    public static ushort ToUShort(string data) { return Parse.UShort(data); }
    public static int ToInt(string data) { return Parse.Int(data); }
    public static uint ToUInt(string data) { return Parse.UInt(data); }
    public static long ToLong(string data) { return Parse.Long(data); }
    public static ulong ToULong(string data) { return Parse.ULong(data); }
    public static float ToFloat(string data) { return Parse.Float(data); }
    public static double ToDouble(string data) { return Parse.Double(data); }
    public static decimal ToDecimal(string data) { return Parse.Decimal(data); }
    public static string ToString(string data) { return data.ToString(); }

    //Dynamic
    //public static Result<object> DynamicConversion(object value, Type from, Type to)
    //{
    //    dynamic asDynamic = from;

    //    try
    //    {
    //        TTo to = (TTo)(dynamic)from;
    //        CanConvert = true;
    //    }
    //    catch
    //    {
    //        CanConvert = false;
    //    }

    //}
    
}
