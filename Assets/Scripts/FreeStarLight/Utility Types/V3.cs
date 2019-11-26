using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class V3 //Vector3 Utilities
{
    public static readonly Vector3 Zero = new Vector3(0f, 0f, 0f);
    public static readonly Vector3 One = new Vector3(1f, 1f, 1f);
    public static readonly Vector3 Half = new Vector3(0.5f, 0.5f, 0.5f);

    public static Vector3 All(float value)
    {
        return new Vector3(value, value, value);
    }

    public static Vector3 X(float x)
    {
        return new Vector3(x, 0f, 0f);
    }
    public static Vector3 Y(float y)
    {
        return new Vector3(0f, y, 0f);
    }
    public static Vector3 Z(float z)
    {
        return new Vector3(0f, 0f, z);
    }

    public static Vector3 XY(float x, float y)
    {
        return new Vector3(x, y, 0f);
    }

    public static Vector3 XZ(float x, float z)
    {
        return new Vector3(x, 0f, z);
    }

    public static Vector3 YZ(float y, float z)
    {
        return new Vector3(0f, y, z);
    }

    public static Vector3 XYZ(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    public static Vector3 SetX(Vector3 origin, float x)
    {
        return new Vector3(x, origin.y, origin.z);
    }

    public static Vector3 SetY(Vector3 origin, float y)
    {
        return new Vector3(origin.x, y, origin.z);
    }

    public static Vector3 SetZ(Vector3 origin, float z)
    {
        return new Vector3(origin.x, origin.y, z);
    }
}