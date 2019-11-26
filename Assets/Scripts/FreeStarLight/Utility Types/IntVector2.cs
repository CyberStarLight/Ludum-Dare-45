using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IntVector2
{
    public int X;
    public int Y;

    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(obj, null))
            return false;

        if (!(obj is IntVector2))
            return false;

        IntVector2 other = (IntVector2)obj;

        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return string.Format("({0},{1})", X, Y);
    }
}
