using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public static partial class Extensions {

    //Floats, Doubles, and Decimals
    public static bool AlmostEquals(this float source, float value, float margin = 0.0001f)
    {
        return source > (value - margin) && source < (value + margin);
    }

    public static bool AlmostGreater(this float source, float value, float margin = 0.0001f)
    {
        return source > (value - margin);
    }

    public static bool AlmostEquals(this double source, double value, double margin = 0.0001d)
    {
        return source > (value - margin) && source < (value + margin);
    }
    
    //Strings
    public static StringBuilder AppendLine(this StringBuilder builder, object objToString)
    {
        builder.AppendLine(objToString.ToString());

        return builder;
    }

    public static StringBuilder Append(this StringBuilder builder, object objToString)
    {
        builder.Append(objToString.ToString());

        return builder;
    }

    //DateTime
    public static long MilisecondsSinceEpoch(this DateTime value)
    {
        return (long)((value.ToUniversalTime() - Utilities.UnixEpoch()).TotalMilliseconds);
    }

    public static long SecondsSinceEpoch(this DateTime value)
    {
        return (long)((value.ToUniversalTime() - Utilities.UnixEpoch()).TotalSeconds);
    }

    //Collections
    public static T[] CombinedWith<T>(this T[] first, T[] second)
    {
        T[] result = new T[first.Length + second.Length];
        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);

        return result;
    }

    public static T[] ShallowClone<T>(this T[] arr)
    {
        T[] result = new T[arr.Length];
        Array.Copy(arr, result, arr.Length);

        return result;
    }


    public static T[] GetRange<T>(this T[] source, int startIndex, int endIndex = -1)
    {
        if (endIndex < 0)
            endIndex = source.LastIndex();

        int resultLength = (endIndex - startIndex) + 1;
        T[] result = new T[resultLength];
        Array.Copy(source, startIndex, result, 0, resultLength);

        return result;
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static List<T> Shuffle<T>(this IList<T> list)
    {
        var newList = new List<T>(list);

        int n = newList.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = newList[k];
            newList[k] = newList[n];
            newList[n] = value;
        }

        return newList;
    }

    public static void ShuffleInPlace<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Override<T>(this HashSet<T> set, T item)
    {
        set.Remove(item);
        set.Add(item);
    }

    //Streams
    public static byte[] ReadAll(this Stream input, int bufferSize = 16 * 1024)
    {
        byte[] buffer = new byte[bufferSize];
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }

    //Unity classes
    public static List<Transform> GetChildren(this Transform source)
    {
        var children = new List<Transform>();

        foreach (Transform child in source)
        {
            children.Add(child);
        }

        return children;
    }

    public static List<Transform> GetHierarchy(this Transform source)
    {
        var result = new List<Transform>();
        var elementsToCheck = new Queue<Transform>();

        elementsToCheck.Enqueue(source);

        while (elementsToCheck.Count > 0)
        {
            var curr = elementsToCheck.Dequeue();

            if (curr.childCount > 0)
            {
                var children = curr.GetChildren();

                result.AddRange(children);
                elementsToCheck.EnqueueRange(children);
            }
        }

        return result;
    }

    public static Dictionary<string, Transform> GetHierarchyDictionary(this Transform source)
    {
        var result = new Dictionary<string, Transform>();
        var items = source.GetHierarchy();

        foreach (var item in items)
        {
            result.Add(item.name, item);
        }

        return result;
    }

    public static List<Transform> GetChildren(this GameObject source)
    {
        return source.transform.GetChildren();
    }

    public static void SetText<T>(this TextMeshProUGUI source, T obj)
    {
        source.text = obj.ToString();
    }

    public static Vector3 Clamp(this Bounds bounds, Vector3 point)
    {
        return new Vector3(
                Mathf.Clamp(point.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(point.y, bounds.min.y, bounds.max.y),
                Mathf.Clamp(point.z, bounds.min.z, bounds.max.z)
                );
    }

    public static void ClampPosition(this Transform transform, Bounds bounds)
    {
        transform.position = bounds.Clamp(transform.position);
    }

    public static void ClampPosition(this Transform transform, BoxCollider box)
    {
        transform.position = box.bounds.Clamp(transform.position);
    }
}