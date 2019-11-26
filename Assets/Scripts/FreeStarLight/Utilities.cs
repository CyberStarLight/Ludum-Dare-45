using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public static partial class Utilities
{
    public const int MILI_SECOND = 1000;
    public const int MILI_MINUTE = MILI_SECOND * 60;
    public const int MILI_HOUR = MILI_MINUTE * 60;
    public const int MILI_DAY = MILI_HOUR * 24;
    
    public const string ASCII_PRINTABLE_CHARS = ASCII_NUMERIC_CHARS + ASCII_ALPHA_CHARS + ASCII_SPECIAL_CHARS + ASCII_WHITESPACE_CHARS;
    public const string ASCII_VISIBLE_CHARS = ASCII_NUMERIC_CHARS + ASCII_ALPHA_CHARS + ASCII_SPECIAL_CHARS;
    public const string ASCII_ALPHA_NUMERIC_CHARS = ASCII_NUMERIC_CHARS + ASCII_ALPHA_CHARS;
    public const string ASCII_NUMERIC_CHARS = "0123456789";
    public const string ASCII_ALPHA_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    public const string ASCII_SPECIAL_CHARS = "\"!#$%&'()*+-.\\/:;<=>?@[]^_`{|}~";
    public const string ASCII_WHITESPACE_CHARS = "\r\n\t ";

    //Strings
    public static string GetRandomString(int length, IList<char> source)
    {
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(source.GetRandom());
        }

        return result.ToString();
    }

    public static string GetRandomString(int length, string source)
    {
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(source[Random.Range(0, source.Length)]);
        }

        return result.ToString();
    }

    public static string GetRandomString(int seed, int length, string source)
    {
        System.Random rand = new System.Random(seed);

        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(source[rand.Next(0, source.Length)]);
        }

        return result.ToString();
    }

    public static string ToHEX(byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];

        byte b;

        for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
        {
            b = ((byte)(bytes[bx] >> 4));
            c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

            b = ((byte)(bytes[bx] & 0x0F));
            c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
        }

        return new string(c);
    }

    public static byte[] FromHEX(string str)
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

    public static string BuildPathFromParts(params string[] parts)
    {
        return BuildPathFromParts(null, parts);
    }

    public static string BuildPathFromParts(char? pathSeperator, params string[] parts)
    {
        StringBuilder result = new StringBuilder();

        char seperator = pathSeperator.HasValue ? pathSeperator.Value : Path.DirectorySeparatorChar;

        Func<string, string> replaceWrongSeperator;
        if (seperator == '\\')
            replaceWrongSeperator = (string str) => { return str.Replace('/', '\\'); };
        else if (seperator == '/')
            replaceWrongSeperator = (string str) => { return str.Replace('\\', '/'); };
        else
            replaceWrongSeperator = null;

        char? lastChar = null;
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            char first = part[0];

            if (lastChar.HasValue && lastChar.Value != seperator && first != seperator)
                result.Append(seperator);

            string fixedPart = replaceWrongSeperator != null ? replaceWrongSeperator(part) : part;
            result.Append(fixedPart);
            
            lastChar = part[part.Length-1];
        }

        return result.ToString();
    }
    
    //Date and time
    public static DateTime UnixEpoch()
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public static long UnixTimestamp()
    {
        return (long)((DateTime.UtcNow - UnixEpoch()).TotalSeconds);
    }

    public static long MinutesSinceEpoch()
    {
        return (long)((DateTime.UtcNow - UnixEpoch()).TotalMinutes);
    }

    public static long MilisecondsSinceEpoch()
    {
        return (long)((DateTime.UtcNow - UnixEpoch()).TotalMilliseconds);
    }

    public static DateTime FromMilisecondsSinceEpoch(long miliseconds)
    {
        return UnixEpoch().AddMilliseconds(miliseconds);
    }

    public static DateTime FromSecondsSinceEpoch(long seconds)
    {
        return UnixEpoch().AddSeconds(seconds);
    }

    //Data & Files
    public static byte[] GZipCompress(byte[] bytes)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (GZipStream zip = new GZipStream(stream, CompressionMode.Compress, false))
            {
                zip.Write(bytes, 0, bytes.Length);
            }

            return stream.ToArray();
        }
    }

    public static byte[] GZipDecompress(byte[] bytes)
    {
        using (MemoryStream stream = new MemoryStream(bytes))
        {
            using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress, false))
            {
                return zip.ReadAll();
            }
        }
    }

    //Unity
    public static Vector3? GetPointerScreenPosition()
    {
        Vector3? pointerScreenPosition = null;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            bool isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
            if (isMouseOverUI)
                return null;

            pointerScreenPosition = Input.mousePosition;
        }
        else
        {
            for (var i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                if (touch.phase == TouchPhase.Began)
                {
                    bool isFingerOverUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                    if (isFingerOverUI)
                        continue;

                    pointerScreenPosition = touch.position;
                    break;
                }
            }
        }

        return pointerScreenPosition;
    }

    public static RaycastHit? RaycastScreenPosition(Vector3 screenPoint, string layerName, float distance = 150f)
    {
        int layerMask = LayerMask.GetMask(layerName);

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out hit, distance, layerMask))
            return hit;
        else
            return null;
    }

    public static bool CheckClickWithPointer(string layerName, float distance = 150f)
    {
        Vector3? pointerScreenPosition = GetPointerScreenPosition();
        if (!pointerScreenPosition.HasValue)
            return false;

        RaycastHit? hit = RaycastScreenPosition(pointerScreenPosition.Value, layerName, distance);

        return hit.HasValue;
    }

    public static RaycastHit? GetClickHitWithPointer(string layerName, float distance = 150f)
    {
        Vector3? pointerScreenPosition = GetPointerScreenPosition();
        if (!pointerScreenPosition.HasValue)
            return null;

        return RaycastScreenPosition(pointerScreenPosition.Value, layerName, distance);
    }

    public static T TrySelectingWithPointer<T>(string layerName, float distance = 150f) where T : Component
    {
        Vector3? pointerScreenPosition = GetPointerScreenPosition();
        if (!pointerScreenPosition.HasValue)
            return null;

        //raycast from mouse on layermask
        RaycastHit? hit = RaycastScreenPosition(pointerScreenPosition.Value, layerName, distance);
        if (!hit.HasValue)
            return null;

        //get component, if smartobject get registered component, otherwise it'll use the normal "GetComponent<T>()"
        T result = SmartBeheviour.ComponentFromInstanceID<T>(hit.Value.transform);
        return result;
    }
}
