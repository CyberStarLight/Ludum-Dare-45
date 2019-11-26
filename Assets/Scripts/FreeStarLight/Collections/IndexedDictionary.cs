using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IndexedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    public new List<TValue> Values = new List<TValue>();
    private Dictionary<TKey, int> keyIndexes = new Dictionary<TKey, int>();
    
    public TValue this[int index]
    {
        get
        {
            return Values[index];
        }
    }

    public new TValue this[TKey key]
    {
        get
        {
            return base[key];
        }
        set
        {
            if (ContainsKey(key))
            {
                base[key] = value;
                Values[keyIndexes[key]] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    public new void Add(TKey key, TValue value)
    {
        base.Add(key, value);

        Values.Add(value);
        keyIndexes[key] = Values.Count - 1;
    }

    public new void Remove(TKey key)
    {
        base.Remove(key);
        
        //get new items and keys (the two lists are ordered with same indexes for each key/value pair)
        Values = base.Values.ToList();
        var currKeys = base.Keys.ToArray();

        //Clear all saved indexes, as the removed item might have changed all indexes after it
        keyIndexes.Clear();

        for (int i = 0; i < currKeys.Length; i++)
        {
            keyIndexes[currKeys[i]] = i;
        }
    }

    public int GetIndexFromKey(TKey key)
    {
        return keyIndexes[key];
    }
}
