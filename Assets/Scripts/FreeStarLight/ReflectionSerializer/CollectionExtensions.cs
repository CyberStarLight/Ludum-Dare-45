using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class CollectionExtensions
{
    public static int LastIndex<T>(this IList<T> source)
    {
        return source.Count - 1;
    }

    public static int LastIndex(this string source)
    {
        return source.Length - 1;
    }

    public static T GetRandom<T>(this IList<T> list, Random rand = null)
    {
        if (rand == null)
            rand = new Random();

        return list[rand.Next(0, list.Count)];
    }

    public static void Shuffle<T>(this IList<T> list, int? seed = null)
    {
        Random rng;

        if (seed.HasValue)
        {
            rng = new Random(seed.Value);
        }
        else
        {
            RNGCryptoServiceProvider seedGenerator = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];
            seedGenerator.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);

            rng = new Random(result);
        }

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void ShuffleInPlace<T>(this IList<T> list, Random rand = null)
    {
        if (rand == null)
            rand = new Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> set)
    {
        return new HashSet<T>(set);
    }

    //Other collections
    public static void EnqueueRange<T>(this Queue<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            source.Enqueue(item);
        }
    }

    public static void Override<T>(this HashSet<T> set, T item)
    {
        set.Remove(item);
        set.Add(item);
    }
}
