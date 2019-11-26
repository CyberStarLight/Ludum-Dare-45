using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyValue<T1, T2>
{
    public T1 Key { get; set; }
    public T2 Value { get; set; }

    public KeyValue(T1 key, T2 value)
    {
        Key = key;
        Value = value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(obj, null))
            return false;

        KeyValue<T1, T2> other = obj as KeyValue<T1, T2>;
        if (other == null)
            return false;

        return Key.Equals(other.Key) && Value.Equals(other.Value);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Key.GetHashCode();
            hash = hash * 23 + Value.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return string.Format("({0},{1})", Key, Value);
    }
}
