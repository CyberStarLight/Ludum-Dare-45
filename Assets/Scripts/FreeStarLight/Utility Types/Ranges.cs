using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Range
{
    public float Min;
    public float Max;

    public Range(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public float Clamp(float value)
    {
        if (value < Min)
            return Min;
        if (value > Max)
            return Max;
        else
            return value;
    }

    public float GetAsRatio(float valueInRange)
    {
        return (valueInRange - Min) / (Max - Min);
    }

    public float GetRandom()
    {
        return Random.Range(Min, Max);
    }
}

[System.Serializable]
public struct IntRange
{
    public int Min;
    public int Max;

    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public int Clamp(int value)
    {
        if (value < Min)
            return Min;
        if (value > Max)
            return Max;
        else
            return value;
    }

    public int GetRandom()
    {
        return Random.Range(Min, Max);
    }
}

[System.Serializable]
public struct UIntRange
{
    public uint Min;
    public uint Max;

    public UIntRange(uint min, uint max)
    {
        Min = min;
        Max = max;
    }

    public uint Clamp(uint value)
    {
        if (value < Min)
            return Min;
        if (value > Max)
            return Max;
        else
            return value;
    }

    public uint GetRandom()
    {
        var buffer = new byte[sizeof(uint)];
        new System.Random().NextBytes(buffer);
        uint result = System.BitConverter.ToUInt32(buffer, 0);

        result = (result % (Max - Min)) + Min;

        return result;
    }
}

[System.Serializable]
public struct Clamped
{
    private float _value;
    public float Value
    {
        get { return _value; }
        set { _value = Range.Clamp(value); }
    }
    private Range _range;
    public Range Range
    {
        get { return _range; }
        set
        {
            _range = value;
            clampValueToRange();
        }
    }

    public Clamped(float value, float min, float max)
    {
        var newRange = new Range(min, max);
        _range = newRange;
        _value = value;
        clampValueToRange();
    }

    private void clampValueToRange()
    {
        _value = _range.Clamp(_value);
    }
}

[System.Serializable]
public struct ClampedInt
{
    private int _value;
    public int Value
    {
        get { return _value; }
        set { _value = Range.Clamp(value); }
    }
    private IntRange _range;
    public IntRange Range
    {
        get { return _range; }
        set
        {
            _range = value;
            clampValueToRange();
        }
    }

    public ClampedInt(int value, int min, int max)
    {
        var newRange = new IntRange(min, max);
        _range = newRange;
        _value = value;
        clampValueToRange();
    }

    private void clampValueToRange()
    {
        _value = _range.Clamp(_value);
    }
}

[System.Serializable]
public struct ClampedUInt
{
    private uint _value;
    public uint Value
    {
        get { return _value; }
        set { _value = Range.Clamp(value); }
    }
    private UIntRange _range;
    public UIntRange Range
    {
        get { return _range; }
        set
        {
            _range = value;
            clampValueToRange();
        }
    }

    public ClampedUInt(uint value, uint min, uint max)
    {
        var newRange = new UIntRange(min, max);
        _range = newRange;
        _value = value;
        clampValueToRange();
    }

    private void clampValueToRange()
    {
        _value = _range.Clamp(_value);
    }
}
