using System;

public class CreationIdentifier : IComparable<CreationIdentifier>
{
    public ulong CreationTime { get; private set; }
    public ulong ID { get; private set; }
    private static Random rand = new Random();

    public CreationIdentifier()
    {
        CreationTime = CurrentUnixTimestampInMilliseconds_ulong();
        ID = NextUlong(0, 9007199254740991);
    }

    public CreationIdentifier(ulong creationTime, ulong id)
    {
        CreationTime = creationTime;
        ID = id;
    }

    //conversions
    public static implicit operator DateTime(CreationIdentifier id)
    {
        return FromUnixTimestampInMilliseconds(id.CreationTime);
    }

    //operators
    public static bool operator ==(CreationIdentifier a, CreationIdentifier b)
    {
        if (ReferenceEquals(a, null))
        {
            return ReferenceEquals(b, null);
        }
        else if (ReferenceEquals(b, null))
        {
            return false;
        }

        return a.CreationTime == b.CreationTime && a.ID == b.ID;
    }

    //Base overrides
    public static bool operator !=(CreationIdentifier a, CreationIdentifier b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(obj, null))
            return false;

        CreationIdentifier id = obj as CreationIdentifier;

        if (ReferenceEquals(id, null))
            return false;

        return id.CreationTime == CreationTime && id.ID == ID;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + CreationTime.GetHashCode();
            hash = hash * 23 + ID.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return CreationTime + "-" + ID;
    }

    public static CreationIdentifier FromString(string source)
    {
        if (source == null)
            return null;

        var parts = source.Split('-');

        if (parts.Length != 2)
            return null;

        ulong creationTime;
        ulong id;
        if (!ulong.TryParse(parts[0], out creationTime) || !ulong.TryParse(parts[1], out id))
            return null;

        return new CreationIdentifier(creationTime, id);
    }

    //Interfaces
    public int CompareTo(CreationIdentifier other)
    {
        return CreationTime.CompareTo(other.CreationTime);
    }

    //tools
    public static ulong NextUlong()
    {
        byte[] buf = new byte[8];
        rand.NextBytes(buf);
        return BitConverter.ToUInt64(buf, 0);
    }

    public static ulong NextUlong(ulong min, ulong max)
    {
        byte[] buf = new byte[8];
        rand.NextBytes(buf);
        ulong longRand = BitConverter.ToUInt64(buf, 0);

        return longRand % (max - min) + min;
    }

    public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static ulong CurrentUnixTimestampInMilliseconds_ulong()
    {
        return Convert.ToUInt64((DateTime.UtcNow - UnixEpoch).TotalMilliseconds);
    }
    public static DateTime FromUnixTimestampInMilliseconds(ulong milliseconds)
    {
        return UnixEpoch.AddMilliseconds(milliseconds);
    }
}