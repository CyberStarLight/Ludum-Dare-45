using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

public class SimpleByteWriter : IDisposable
{
    public const byte FALSE_BYTE = 0x00;
    public const byte TRUE_BYTE = 0x01;

    protected MemoryStream memoryStream;

    protected static Dictionary<Type, Action<SimpleByteWriter, object>> writeFunctions = new Dictionary<Type, Action<SimpleByteWriter, object>>() {
            //Primitives
            { typeof(byte), (SimpleByteWriter writer, object o) => { writer.Write((byte)o); } },
            { typeof(sbyte), (SimpleByteWriter writer, object o) => { writer.Write((sbyte)o); } },
            { typeof(bool), (SimpleByteWriter writer, object o) => { writer.Write((bool)o); } },
            { typeof(char), (SimpleByteWriter writer, object o) => { writer.Write((char)o); } },
            { typeof(short), (SimpleByteWriter writer, object o) => { writer.Write((short)o); } },
            { typeof(ushort), (SimpleByteWriter writer, object o) => { writer.Write((ushort)o); } },
            { typeof(int), (SimpleByteWriter writer, object o) => { writer.Write((int)o); } },
            { typeof(uint), (SimpleByteWriter writer, object o) => { writer.Write((uint)o); } },
            { typeof(long), (SimpleByteWriter writer, object o) => { writer.Write((long)o); } },
            { typeof(ulong), (SimpleByteWriter writer, object o) => { writer.Write((ulong)o); } },
            { typeof(float), (SimpleByteWriter writer, object o) => { writer.Write((float)o); } },
            { typeof(double), (SimpleByteWriter writer, object o) => { writer.Write((double)o); } },
            { typeof(decimal), (SimpleByteWriter writer, object o) => { writer.Write((decimal)o); } },
            { typeof(string), (SimpleByteWriter writer, object o) => { writer.Write((string)o); } },
            { typeof(Type), (SimpleByteWriter writer, object o) => { writer.Write((Type)o); } },
            { typeof(DateTime), (SimpleByteWriter writer, object o) => { writer.Write((DateTime)o); } },
            //Nullables
            { typeof(byte?), (SimpleByteWriter writer, object o) => { writer.Write((byte?)o); } },
            { typeof(sbyte?), (SimpleByteWriter writer, object o) => { writer.Write((sbyte?)o); } },
            { typeof(bool?), (SimpleByteWriter writer, object o) => { writer.Write((bool?)o); } },
            { typeof(char?), (SimpleByteWriter writer, object o) => { writer.Write((char?)o); } },
            { typeof(short?), (SimpleByteWriter writer, object o) => { writer.Write((short?)o); } },
            { typeof(ushort?), (SimpleByteWriter writer, object o) => { writer.Write((ushort?)o); } },
            { typeof(int?), (SimpleByteWriter writer, object o) => { writer.Write((int?)o); } },
            { typeof(uint?), (SimpleByteWriter writer, object o) => { writer.Write((uint?)o); } },
            { typeof(long?), (SimpleByteWriter writer, object o) => { writer.Write((long?)o); } },
            { typeof(ulong?), (SimpleByteWriter writer, object o) => { writer.Write((ulong?)o); } },
            { typeof(float?), (SimpleByteWriter writer, object o) => { writer.Write((float?)o); } },
            { typeof(double?), (SimpleByteWriter writer, object o) => { writer.Write((double?)o); } },
            { typeof(decimal?), (SimpleByteWriter writer, object o) => { writer.Write((decimal?)o); } },
            { typeof(DateTime?), (SimpleByteWriter writer, object o) => { writer.Write((DateTime?)o); } },
        };
    protected static Dictionary<Type, Func<SimpleByteWriter, object>> readFunctions = new Dictionary<Type, Func<SimpleByteWriter, object>>() {
            //Primitives
            { typeof(byte), (SimpleByteWriter writer) => { return writer.ReadByte(); } },
            { typeof(sbyte), (SimpleByteWriter writer) => { return writer.ReadSByte(); } },
            { typeof(bool), (SimpleByteWriter writer) => { return writer.ReadBool(); } },
            { typeof(char), (SimpleByteWriter writer) => { return writer.ReadChar(); } },
            { typeof(short), (SimpleByteWriter writer) => { return writer.ReadShort(); } },
            { typeof(ushort), (SimpleByteWriter writer) => { return writer.ReadUShort(); } },
            { typeof(int), (SimpleByteWriter writer) => { return writer.ReadInt(); } },
            { typeof(uint), (SimpleByteWriter writer) => { return writer.ReadUInt(); } },
            { typeof(long), (SimpleByteWriter writer) => { return writer.ReadLong(); } },
            { typeof(ulong), (SimpleByteWriter writer) => { return writer.ReadULong(); } },
            { typeof(float), (SimpleByteWriter writer) => { return writer.ReadFloat(); } },
            { typeof(double), (SimpleByteWriter writer) => { return writer.ReadDouble(); } },
            { typeof(decimal), (SimpleByteWriter writer) => { return writer.ReadDecimal(); } },
            { typeof(string), (SimpleByteWriter writer) => { return writer.ReadString(); } },
            { typeof(Type), (SimpleByteWriter writer) => { return writer.ReadType(); } },
            { typeof(DateTime), (SimpleByteWriter writer) => { return writer.ReadDateTime(); } },
            //Nullables
            { typeof(byte?), (SimpleByteWriter writer) => { return writer.ReadByte_Nullable(); } },
            { typeof(sbyte?), (SimpleByteWriter writer) => { return writer.ReadSByte_Nullable(); } },
            { typeof(bool?), (SimpleByteWriter writer) => { return writer.ReadBool_Nullable(); } },
            { typeof(char?), (SimpleByteWriter writer) => { return writer.ReadChar_Nullable(); } },
            { typeof(short?), (SimpleByteWriter writer) => { return writer.ReadShort_Nullable(); } },
            { typeof(ushort?), (SimpleByteWriter writer) => { return writer.ReadUShort_Nullable(); } },
            { typeof(int?), (SimpleByteWriter writer) => { return writer.ReadInt_Nullable(); } },
            { typeof(uint?), (SimpleByteWriter writer) => { return writer.ReadUInt_Nullable(); } },
            { typeof(long?), (SimpleByteWriter writer) => { return writer.ReadLong_Nullable(); } },
            { typeof(ulong?), (SimpleByteWriter writer) => { return writer.ReadULong_Nullable(); } },
            { typeof(float?), (SimpleByteWriter writer) => { return writer.ReadFloat_Nullable(); } },
            { typeof(double?), (SimpleByteWriter writer) => { return writer.ReadDouble_Nullable(); } },
            { typeof(decimal?), (SimpleByteWriter writer) => { return writer.ReadDecimal_Nullable(); } },
            { typeof(DateTime?), (SimpleByteWriter writer) => { return writer.ReadDateTime_Nullable(); } },
        };

    protected static Action<SimpleByteWriter, object> ArrayWriteFunction = (SimpleByteWriter writer, object o) => { writer.Write((Array)o); };
    protected static Action<SimpleByteWriter, object> IDictionaryWriteFunction = (SimpleByteWriter writer, object o) => { writer.Write((IDictionary)o); };
    protected static Action<SimpleByteWriter, object> IListWriteFunction = (SimpleByteWriter writer, object o) => { writer.Write((IList)o); };

    protected static HashSet<Type> arrayTypes = new HashSet<Type>();
    protected static HashSet<Type> dictionaryTypes = new HashSet<Type>();
    protected static HashSet<Type> IListTypes = new HashSet<Type>();
    protected static HashSet<Type> compoundTypes = new HashSet<Type>();
    protected static HashSet<Type> unsupportedTypes = new HashSet<Type>();

    protected static Dictionary<Type, Type> IListToItemType = new Dictionary<Type, Type>();
    protected static Dictionary<Type, KeyValuePair<Type, Type>> IDictionaryToItemTypes = new Dictionary<Type, KeyValuePair<Type, Type>>();

    protected byte[] storage = new byte[1024];

    public SimpleByteWriter()
    {
        memoryStream = new MemoryStream();
    }

    public SimpleByteWriter(int capacity)
    {
        memoryStream = new MemoryStream(capacity);
    }

    public SimpleByteWriter(byte[] source)
    {
        memoryStream = new MemoryStream(source);
    }

    public void ResetBuffer()
    {
        memoryStream.SetLength(0);
    }

    //Write functions search
    protected static Action<SimpleByteWriter, object> GetWriterFunction(Type type)
    {
        Action<SimpleByteWriter, object> result = null;

        if (writeFunctions.TryGetValue(type, out result))
        {
            //is one of the primitive types
            return result;
        }
        else
        {
            //isn't a primitive type, check if it's a collection or dictionary

            //Check cached lists first to avoid reflection on previously encountered types
            if (arrayTypes.Contains(type))
            {
                return ArrayWriteFunction;
            }
            else if (dictionaryTypes.Contains(type))
            {
                return IDictionaryWriteFunction;
            }
            else if (IListTypes.Contains(type))
            {
                return IListWriteFunction;
            }
            else if (unsupportedTypes.Contains(type))
            {
                return null;
            }

            //not in cach yet, try to determine if we have a valid write function, add to cache if we find a valid function
            if (type.IsArray)
            {
                arrayTypes.Add(type);
                IListToItemType[type] = type.GetElementType();
                return ArrayWriteFunction;
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                dictionaryTypes.Add(type);
                Type[] genericArguments = type.GetGenericArguments();
                IDictionaryToItemTypes[type] = new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);
                return IDictionaryWriteFunction;
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                IListTypes.Add(type);
                IListToItemType[type] = type.GetGenericArguments()[0];
                return IListWriteFunction;
            }
            else
            {
                //Is compound object
                return null;
            }
        }
    }

    //Read functions search
    protected static Func<SimpleByteWriter, object> GetReaderFunction(Type type)
    {
        Func<SimpleByteWriter, object> result = null;

        if (readFunctions.TryGetValue(type, out result))
        {
            //is one of the primitive types
            return result;
        }
        else
        {
            //isn't a primitive type, check if it's a collection or dictionary

            //Check cached lists first to avoid reflection on previously encountered types
            if (arrayTypes.Contains(type))
            {
                Type itemType = IListToItemType[type];
                Func<SimpleByteWriter, object> arrayReadFunction = (SimpleByteWriter writer) => { return writer.ReadArray(itemType); };
                return arrayReadFunction;
            }
            else if (dictionaryTypes.Contains(type))
            {
                Func<SimpleByteWriter, object> dictionaryReadFunction = (SimpleByteWriter writer) => { return writer.ReadDictionary(type); };
                return dictionaryReadFunction;
            }
            else if (IListTypes.Contains(type))
            {
                Func<SimpleByteWriter, object> listReadFunction = (SimpleByteWriter writer) => { return writer.ReadList(type); };
                return listReadFunction;
            }
            else if (unsupportedTypes.Contains(type))
            {
                return null;
            }

            //not in cach yet, try to determine if we have a valid write function, add to cache if we find a valid function
            if (type.IsArray)
            {
                arrayTypes.Add(type);
                Type itemType = type.GetElementType();
                IListToItemType[type] = itemType;
                Func<SimpleByteWriter, object> arrayReadFunction = (SimpleByteWriter writer) => { return writer.ReadArray(itemType); };
                return arrayReadFunction;
            }
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                dictionaryTypes.Add(type);
                Type[] genericArguments = type.GetGenericArguments();
                IDictionaryToItemTypes[type] = new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);

                Func<SimpleByteWriter, object> dictionaryReadFunction = (SimpleByteWriter writer) => { return writer.ReadDictionary(type); };
                return dictionaryReadFunction;
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                IListTypes.Add(type);
                Type itemType = type.GetGenericArguments()[0];
                IListToItemType[type] = itemType;
                Func<SimpleByteWriter, object> listReadFunction = (SimpleByteWriter writer) => { return writer.ReadList(type); };
                return listReadFunction;
            }
            else
            {
                //Is compound object
                return null;
            }
        }
    }

    //Reflection write
    public void Write(params object[] objects)
    {
        foreach (var obj in objects)
        {
            Write(obj);
        }
    }

    public void Write<T>(T source, params Func<T, object>[] getValues)
    {
        foreach (var getValue in getValues)
        {
            Write(getValue(source));
        }
    }

    public void Write(object obj)
    {
        Type objType = obj.GetType();
        var writerFunction = GetWriterFunction(objType);

        writerFunction(this, obj);
    }

    public T Read<T>()
    {
        var readerFunction = GetReaderFunction(typeof(T));

        return (T)readerFunction(this);
    }

    //Basic byte writes/reads
    public void Write(byte b)
    {
        memoryStream.WriteByte(b);
    }

    public void Write(sbyte sb)
    {
        Write((byte)sb);
    }

    public void Write(byte[] bytes)
    {
        memoryStream.Write(bytes, 0, bytes.Length);
    }

    public void Write(IEnumerable<byte> bytes)
    {
        Write(bytes.ToArray());
    }

    public byte ReadByte()
    {
        return Convert.ToByte(memoryStream.ReadByte());
    }

    public sbyte ReadSByte()
    {
        return (sbyte)Convert.ToByte(memoryStream.ReadByte());
    }

    protected void readBytesToStorage(int count)
    {
        if (count > storage.Length)
            throw new IndexOutOfRangeException("Too big for Primitive Storage!");

        memoryStream.Read(storage, 0, count);
    }

    public byte[] ReadBytes(int count)
    {
        if (count <= 0)
            return new byte[0];

        byte[] result = new byte[count];
        memoryStream.Read(result, 0, count);

        return result;
    }

    //Primitive type writes/reads
    public void Write(bool data)
    {
        Write(data ? TRUE_BYTE : FALSE_BYTE);
    }

    public void Write(char data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(short data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(ushort data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(int data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(uint data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(long data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(ulong data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(float data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(double data)
    {
        Write(BitConverter.GetBytes(data));
    }

    public void Write(decimal data)
    {
        int[] bits = decimal.GetBits(data);

        Write(bits[0]);
        Write(bits[1]);
        Write(bits[2]);
        Write(bits[3]);
    }

    public void Write(string data)
    {
        if (data == null)
        {
            Write(FALSE_BYTE);
            return;
        }
        else
        {
            Write(TRUE_BYTE);
            byte[] stringBytes = Encoding.UTF8.GetBytes(data);
            Write(stringBytes.Length);
            Write(stringBytes);
        }
    }

    public void Write(Type data)
    {
        if (data == null)
        {
            Write((byte)0xFF);
            return;
        }

        bool isKnownAssembly = data.Assembly == Assembly.GetExecutingAssembly() || data.Assembly.FullName.StartsWith("mscorlib,");

        if (data.IsGenericType)
        {
            var t = data.GetGenericTypeDefinition();
            var p = data.GetGenericArguments();
            Write((byte)p.Length);

            //Write(t.AssemblyQualifiedName);

            if (isKnownAssembly)
                Write(t.FullName);
            else
                Write(t.AssemblyQualifiedName);

            for (int i = 0; i < p.Length; i++)
            {
                Write(p[i]);
            }
            return;
        }
        Write((byte)0);

        if (isKnownAssembly)
            Write(data.FullName);
        else
            Write(data.AssemblyQualifiedName);
    }

    public void Write(DateTime data)
    {
        Write(data.ToBinary());
    }

    public bool ReadBool()
    {
        int value = memoryStream.ReadByte();

        if (value <= 0)
            return false;
        else
            return true;
    }

    public char ReadChar()
    {
        readBytesToStorage(2);
        return BitConverter.ToChar(storage, 0);
    }

    public short ReadShort()
    {
        readBytesToStorage(2);
        return BitConverter.ToInt16(storage, 0);
    }

    public ushort ReadUShort()
    {
        readBytesToStorage(2);
        return BitConverter.ToUInt16(storage, 0);
    }

    public int ReadInt()
    {
        readBytesToStorage(4);
        return BitConverter.ToInt32(storage, 0);
    }

    public uint ReadUInt()
    {
        readBytesToStorage(4);
        return BitConverter.ToUInt32(storage, 0);
    }

    public long ReadLong()
    {
        readBytesToStorage(8);
        return BitConverter.ToInt64(storage, 0);
    }

    public ulong ReadULong()
    {
        readBytesToStorage(8);
        return BitConverter.ToUInt64(storage, 0);
    }

    public float ReadFloat()
    {
        readBytesToStorage(4);
        return BitConverter.ToSingle(storage, 0);
    }

    public double ReadDouble()
    {
        readBytesToStorage(8);
        return BitConverter.ToDouble(storage, 0);
    }

    public decimal ReadDecimal()
    {
        int[] bits = new int[] {
                ReadInt(),
                ReadInt(),
                ReadInt(),
                ReadInt(),
            };

        return new decimal(bits);
    }

    public string ReadString()
    {
        bool hasValue = ReadBool();

        if (!hasValue)
            return null;

        int sizeInBytes = ReadInt();

        if (sizeInBytes <= storage.Length)
        {
            readBytesToStorage(sizeInBytes);
            return Encoding.UTF8.GetString(storage, 0, sizeInBytes);
        }
        else
        {
            byte[] data = ReadBytes(sizeInBytes);
            return Encoding.UTF8.GetString(data, 0, sizeInBytes);
        }
    }

    public Type ReadType()
    {
        var paramCount = ReadByte();
        if (paramCount == 0xFF)
            return null;
        var typeName = ReadString();
        var type = System.Type.GetType(typeName);
        if (type == null)
            throw new System.Exception("Can't find type; '" + typeName + "'");
        if (type.IsGenericTypeDefinition && paramCount > 0)
        {
            var p = new System.Type[paramCount];
            for (int i = 0; i < paramCount; i++)
            {
                p[i] = ReadType();
            }
            type = type.MakeGenericType(p);
        }
        return type;
    }

    public DateTime ReadDateTime()
    {
        return DateTime.FromBinary(ReadLong());
    }

    //Nullable type writes/reads
    public void Write(byte? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(sbyte? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(bool? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(char? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(short? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(ushort? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(int? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(uint? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(long? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(ulong? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(decimal? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(DateTime? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(float? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public void Write(double? b)
    {
        if (b.HasValue)
        {
            memoryStream.WriteByte(TRUE_BYTE);
            Write(b.Value);
        }
        else
        {
            memoryStream.WriteByte(FALSE_BYTE);
        }
    }

    public byte? ReadByte_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadByte();
        else
            return null;
    }

    public sbyte? ReadSByte_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadSByte();
        else
            return null;
    }

    public bool? ReadBool_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadBool();
        else
            return null;
    }

    public char? ReadChar_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadChar();
        else
            return null;
    }

    public short? ReadShort_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadShort();
        else
            return null;
    }

    public ushort? ReadUShort_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadUShort();
        else
            return null;
    }

    public int? ReadInt_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadInt();
        else
            return null;
    }

    public uint? ReadUInt_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadUInt();
        else
            return null;
    }

    public long? ReadLong_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadLong();
        else
            return null;
    }

    public ulong? ReadULong_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadULong();
        else
            return null;
    }

    public float? ReadFloat_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadFloat();
        else
            return null;
    }

    public double? ReadDouble_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadDouble();
        else
            return null;
    }

    public decimal? ReadDecimal_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadDecimal();
        else
            return null;
    }

    public DateTime? ReadDateTime_Nullable()
    {
        var hasValue = ReadBool();

        if (hasValue)
            return ReadDateTime();
        else
            return null;
    }

    //Simple lists writes

    public void Write(Array data, Func<Type, Action<SimpleByteWriter, object>> getWriterFunction = null)
    {
        if (data == null)
        {
            Write(false);
            return;
        }
        else
        {
            Write(true);
        }

        if (!isSerializeableArray(data)) //checks for rare multi-dimensional arrays with wierd zero-length dimensions.
            return;

        int count = data.Length;

        Type arrayType = data.GetType();
        Type itemType = arrayType.GetElementType();

        Action<SimpleByteWriter, object> writeFunc = getWriterFunction == null ? GetWriterFunction(itemType) : getWriterFunction(itemType);
        if (writeFunc == null)
        {
            return;
        }

        int dimensionsCount = data.Rank;

        //Write dimension count
        Write(dimensionsCount);

        int[] indexes = new int[dimensionsCount];
        int[] maxIndexes = new int[dimensionsCount];
        for (int i = 0; i < dimensionsCount; i++)
        {
            indexes[i] = 0;

            int dimentionLength = data.GetLength(i);
            maxIndexes[i] = dimentionLength - 1;

            //Write dimension size
            Write(dimentionLength);
        }

        int lastIndex = indexes.Length - 1;

        //if it's a zero-length one-dimentional array, don't seialize any items
        if (maxIndexes[0] < 0)
            return;

        //write all items in order, from last dimension to first & from index 0 to (length-1)
        while (true)
        {
            object item = data.GetValue(indexes);
            writeFunc(this, item);

            if (isMaxIndex(indexes, maxIndexes))
                break;

            for (int currIndexToIncrement = lastIndex; currIndexToIncrement >= 0; currIndexToIncrement--)
            {
                if (indexes[currIndexToIncrement] < maxIndexes[currIndexToIncrement])
                {
                    indexes[currIndexToIncrement]++;
                    break;
                }
                else if (currIndexToIncrement != 0) //do not zero-out the first index, we got to the 0 index, and everything is maxed, and 0 index is maxed we are at max index, just exit.
                {
                    indexes[currIndexToIncrement] = 0;
                }
            }
        }
    }

    public Array ReadArray(Type itemType, Func<Type, Func<SimpleByteWriter, object>> getReaderFunction = null)
    {
        bool hasValue = ReadBool();

        if (!hasValue)
            return null;

        //Get Dimensions and lengths
        int dimensionsCount = ReadInt();

        int[] indexes = new int[dimensionsCount];
        int[] maxIndexes = new int[dimensionsCount];
        int[] lengths = new int[dimensionsCount];
        for (int i = 0; i < dimensionsCount; i++)
        {
            indexes[i] = 0;

            int dimensionLength = ReadInt();
            lengths[i] = dimensionLength;
            maxIndexes[i] = dimensionLength - 1;
        }

        int lastIndex = indexes.Length - 1;

        //Instansiate array
        Array data = Array.CreateInstance(itemType, lengths);

        //if it's a zero-length one-dimentional array, don't read any items, return the empty Array.
        if (maxIndexes[0] < 0)
        {
            return data;
        }

        //read all items in order, from last dimension to first & from index 0 to (length-1)
        Func<SimpleByteWriter, object> itemReaderFunction = getReaderFunction == null ? GetReaderFunction(itemType) : getReaderFunction(itemType);
        if(itemReaderFunction == null)
        {
            return null;
        }

        while (true)
        {
            //read item from bytes and insert it to the array
            object item = itemReaderFunction(this);
            data.SetValue(item, indexes);

            if (isMaxIndex(indexes, maxIndexes))
                break;

            for (int currIndexToIncrement = lastIndex; currIndexToIncrement >= 0; currIndexToIncrement--)
            {
                if (indexes[currIndexToIncrement] < maxIndexes[currIndexToIncrement])
                {
                    indexes[currIndexToIncrement]++;
                    break;
                }
                else if (currIndexToIncrement != 0) //do not zero-out the first index, we got to the 0 index, and everything is maxed, and 0 index is maxed we are at max index, just exit.
                {
                    indexes[currIndexToIncrement] = 0;
                }
            }
        }

        return data;
    }

    public void Write(IList data, Func<Type, Action<SimpleByteWriter, object>> getWriterFunction = null)
    {
        if (data == null)
        {
            Write(false);
            return;
        }
        else
        {
            Write(true);
        }

        Type listType = data.GetType();

        int itemCount = data.Count;

        Type itemType;
        if (!IListToItemType.TryGetValue(listType, out itemType))
        {
            Type[] genericArguments = listType.GetGenericArguments();
            itemType = genericArguments[0];
            IListToItemType[listType] = itemType;
        }

        Array items = Array.CreateInstance(itemType, itemCount);
        data.CopyTo(items, 0);

        Write(items, getWriterFunction);
    }

    public IList ReadList(Type listType, Func<Type, Func<SimpleByteWriter, object>> getReaderFunction = null)
    {
        bool hasValue = ReadBool();

        if (!hasValue)
            return null;

        Type itemType;
        if (!IListToItemType.TryGetValue(listType, out itemType))
        {
            Type[] genericArguments = listType.GetGenericArguments();
            itemType = genericArguments[0];
            IListToItemType[listType] = itemType;
        }

        Array items = ReadArray(itemType, getReaderFunction);

        IList instance = (IList)Activator.CreateInstance(listType);
        foreach (var item in items)
        {
            instance.Add(item);
        }

        return instance;
    }

    public void Write(IDictionary data, Func<Type, Action<SimpleByteWriter, object>> getWriterFunction = null)
    {
        if (data == null)
        {
            Write(false);
            return;
        }
        else
        {
            Write(true);
        }

        Type dictionaryType = data.GetType();

        int itemCount = data.Count;
        KeyValuePair<Type, Type> itemType;
        if (!IDictionaryToItemTypes.TryGetValue(dictionaryType, out itemType))
        {
            Type[] genericArguments = dictionaryType.GetGenericArguments();
            itemType = new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);
            IDictionaryToItemTypes[dictionaryType] = itemType;
        }

        Array keys = Array.CreateInstance(itemType.Key, itemCount);
        Array values = Array.CreateInstance(itemType.Value, itemCount);
        data.Keys.CopyTo(keys, 0);
        data.Values.CopyTo(values, 0);

        Write(itemCount);
        Write(keys, getWriterFunction);
        Write(values, getWriterFunction);
    }

    public IDictionary ReadDictionary(Type dictionaryType, Func<Type, Func<SimpleByteWriter, object>> getReaderFunction = null)
    {
        bool hasValue = ReadBool();

        if (!hasValue)
            return null;

        //Get the item type
        KeyValuePair<Type, Type> itemType;
        if (!IDictionaryToItemTypes.TryGetValue(dictionaryType, out itemType))
        {
            Type[] genericArguments = dictionaryType.GetGenericArguments();
            itemType = new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);
            IDictionaryToItemTypes[dictionaryType] = itemType;
        }

        //Instantiate dictionary
        IDictionary instance = (IDictionary)Activator.CreateInstance(dictionaryType);

        //read item count
        int itemCount = ReadInt();

        //read items
        Array keys = ReadArray(itemType.Key, getReaderFunction);
        Array values = ReadArray(itemType.Value, getReaderFunction);

        for (int i = 0; i < keys.Length; i++)
        {
            instance.Add(keys.GetValue(i), values.GetValue(i));
        }

        return instance;
    }

    public byte[] ToArray()
    {
        return memoryStream.ToArray();
    }

    public byte[] ToGZIP()
    {
        var result = new MemoryStream();
        using (GZipStream zipStream = new GZipStream(result, CompressionLevel.Optimal, false))
        {
            var bytes = memoryStream.ToArray();
            zipStream.Write(bytes, 0, bytes.Length);
        }

        return result.ToArray();
    }

    public void WriteToFile(string filePath)
    {
        File.WriteAllBytes(filePath, memoryStream.ToArray());
    }

    public void Dispose()
    {
        memoryStream.Dispose();
    }

    //General Helpers
    protected object GetDefaultValue(Type t)
    {
        if (t.IsValueType)
            return Activator.CreateInstance(t);

        return null;
    }

    protected bool isMaxIndex(int[] indexes, int[] maxIndexes)
    {
        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] < maxIndexes[i])
                return false;
        }

        return true;
    }

    protected bool isSerializeableArray(Array data)
    {
        if (data == null)
            return true;

        int dimensionsCount = data.Rank;

        if (dimensionsCount == 1)
            return true;

        for (int i = 0; i < dimensionsCount; i++)
        {
            //if any dimension length is 0, we can't serialize it.
            if (data.GetLength(i) <= 0)
            {
                //TODO: log error, Debug.Log("Can't serialize array! one of the array dimensions is 0 length, dimension index: " + i);
                return false;
            }
        }

        return true;
    }
}