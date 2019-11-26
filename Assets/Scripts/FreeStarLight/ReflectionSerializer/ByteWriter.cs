using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ReflectionSerializer
{
    public class ByteWriter : IDisposable
    {
        public const byte FALSE_BYTE = 0x00;
        public const byte TRUE_BYTE = 0x01;

        private MemoryStream memoryStream;
        
        private static Dictionary<Type, Action<ByteWriter, object>> writeFunctions = new Dictionary<Type, Action<ByteWriter, object>>() {
            //Primitives
            { typeof(byte), (ByteWriter writer, object o) => { writer.Write((byte)o); } },
            { typeof(sbyte), (ByteWriter writer, object o) => { writer.Write((sbyte)o); } },
            { typeof(bool), (ByteWriter writer, object o) => { writer.Write((bool)o); } },
            { typeof(char), (ByteWriter writer, object o) => { writer.Write((char)o); } },
            { typeof(short), (ByteWriter writer, object o) => { writer.Write((short)o); } },
            { typeof(ushort), (ByteWriter writer, object o) => { writer.Write((ushort)o); } },
            { typeof(int), (ByteWriter writer, object o) => { writer.Write((int)o); } },
            { typeof(uint), (ByteWriter writer, object o) => { writer.Write((uint)o); } },
            { typeof(long), (ByteWriter writer, object o) => { writer.Write((long)o); } },
            { typeof(ulong), (ByteWriter writer, object o) => { writer.Write((ulong)o); } },
            { typeof(float), (ByteWriter writer, object o) => { writer.Write((float)o); } },
            { typeof(double), (ByteWriter writer, object o) => { writer.Write((double)o); } },
            { typeof(decimal), (ByteWriter writer, object o) => { writer.Write((decimal)o); } },
            { typeof(string), (ByteWriter writer, object o) => { writer.Write((string)o); } },
            //Nullables
            { typeof(byte?), (ByteWriter writer, object o) => { writer.Write((byte?)o); } },
            { typeof(sbyte?), (ByteWriter writer, object o) => { writer.Write((sbyte?)o); } },
            { typeof(bool?), (ByteWriter writer, object o) => { writer.Write((bool?)o); } },
            { typeof(char?), (ByteWriter writer, object o) => { writer.Write((char?)o); } },
            { typeof(short?), (ByteWriter writer, object o) => { writer.Write((short?)o); } },
            { typeof(ushort?), (ByteWriter writer, object o) => { writer.Write((ushort?)o); } },
            { typeof(int?), (ByteWriter writer, object o) => { writer.Write((int?)o); } },
            { typeof(uint?), (ByteWriter writer, object o) => { writer.Write((uint?)o); } },
            { typeof(long?), (ByteWriter writer, object o) => { writer.Write((long?)o); } },
            { typeof(ulong?), (ByteWriter writer, object o) => { writer.Write((ulong?)o); } },
            { typeof(float?), (ByteWriter writer, object o) => { writer.Write((float?)o); } },
            { typeof(double?), (ByteWriter writer, object o) => { writer.Write((double?)o); } },
            { typeof(decimal?), (ByteWriter writer, object o) => { writer.Write((decimal?)o); } },
        };
        private static Dictionary<Type, Func<ByteWriter, object>> readFunctions = new Dictionary<Type, Func<ByteWriter, object>>() {
            //Primitives
            { typeof(byte), (ByteWriter writer) => { return writer.ReadByte(); } },
            { typeof(sbyte), (ByteWriter writer) => { return writer.ReadSByte(); } },
            { typeof(bool), (ByteWriter writer) => { return writer.ReadBool(); } },
            { typeof(char), (ByteWriter writer) => { return writer.ReadChar(); } },
            { typeof(short), (ByteWriter writer) => { return writer.ReadShort(); } },
            { typeof(ushort), (ByteWriter writer) => { return writer.ReadUShort(); } },
            { typeof(int), (ByteWriter writer) => { return writer.ReadInt(); } },
            { typeof(uint), (ByteWriter writer) => { return writer.ReadUInt(); } },
            { typeof(long), (ByteWriter writer) => { return writer.ReadLong(); } },
            { typeof(ulong), (ByteWriter writer) => { return writer.ReadULong(); } },
            { typeof(float), (ByteWriter writer) => { return writer.ReadFloat(); } },
            { typeof(double), (ByteWriter writer) => { return writer.ReadDouble(); } },
            { typeof(decimal), (ByteWriter writer) => { return writer.ReadDecimal(); } },
            { typeof(string), (ByteWriter writer) => { return writer.ReadString(); } },
            //Nullables
            { typeof(byte?), (ByteWriter writer) => { return writer.ReadByte_Nullable(); } },
            { typeof(sbyte?), (ByteWriter writer) => { return writer.ReadSByte_Nullable(); } },
            { typeof(bool?), (ByteWriter writer) => { return writer.ReadBool_Nullable(); } },
            { typeof(char?), (ByteWriter writer) => { return writer.ReadChar_Nullable(); } },
            { typeof(short?), (ByteWriter writer) => { return writer.ReadShort_Nullable(); } },
            { typeof(ushort?), (ByteWriter writer) => { return writer.ReadUShort_Nullable(); } },
            { typeof(int?), (ByteWriter writer) => { return writer.ReadInt_Nullable(); } },
            { typeof(uint?), (ByteWriter writer) => { return writer.ReadUInt_Nullable(); } },
            { typeof(long?), (ByteWriter writer) => { return writer.ReadLong_Nullable(); } },
            { typeof(ulong?), (ByteWriter writer) => { return writer.ReadULong_Nullable(); } },
            { typeof(float?), (ByteWriter writer) => { return writer.ReadFloat_Nullable(); } },
            { typeof(double?), (ByteWriter writer) => { return writer.ReadDouble_Nullable(); } },
            { typeof(decimal?), (ByteWriter writer) => { return writer.ReadDecimal_Nullable(); } },
        };

        private static Action<ByteWriter, object> ArrayWriteFunction = (ByteWriter writer, object o) => { writer.Write((Array)o); };
        private static Action<ByteWriter, object> IDictionaryWriteFunction = (ByteWriter writer, object o) => { writer.Write((IDictionary)o); };
        private static Action<ByteWriter, object> IListWriteFunction = (ByteWriter writer, object o) => { writer.Write((IList)o); };
        private static Action<ByteWriter, object> compundWriteFunction = (ByteWriter writer, object o) => { writer.writeCompoundObject(o); };

        private static HashSet<Type> arrayTypes = new HashSet<Type>();
        private static HashSet<Type> dictionaryTypes = new HashSet<Type>();
        private static HashSet<Type> IListTypes = new HashSet<Type>();
        private static HashSet<Type> compoundTypes = new HashSet<Type>();
        private static HashSet<Type> unsupportedTypes = new HashSet<Type>();

        private static Dictionary<Type, Type> IListToItemType = new Dictionary<Type, Type>();
        private static Dictionary<Type, KeyValuePair<Type, Type>> IDictionaryToItemTypes = new Dictionary<Type, KeyValuePair<Type, Type>>();

        private byte[] storage = new byte[10240];

        public ByteWriter()
        {
            memoryStream = new MemoryStream();
        }

        public ByteWriter(byte[] source)
        {
            memoryStream = new MemoryStream(source);
        }

        //Write functions search
        private static Action<ByteWriter, object> GetWriterFunction(Type type)
        {
            Debug.Log("ByteWriter GetWriterFunction() type:" + type.ToString());

            Action<ByteWriter, object> result = null;

            if (writeFunctions.TryGetValue(type, out result))
            {
                //is one of the primitive types
                return result;
            }
            else
            {
                //isn't a primitive type, check if it's a collection or dictionary

                //Check cached lists first to avoid reflection on previously encountered types
                if(arrayTypes.Contains(type))
                {
                    return ArrayWriteFunction;
                }
                else if(dictionaryTypes.Contains(type))
                {
                    return IDictionaryWriteFunction;
                }
                else if(IListTypes.Contains(type))
                {
                    return IListWriteFunction;
                }
                else if (unsupportedTypes.Contains(type))
                {
                    return null;
                }
                
                //not in cach yet, try to determine if we have a valid write function, add to cache if we find a valid function
                if(type.IsArray)
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
                    compoundTypes.Add(type);
                    return compundWriteFunction;
                }
            }
        }

        //Read functions search
        private static Func<ByteWriter, object> GetReaderFunction(Type type)
        {
            Debug.Log("ByteWriter GetReaderFunction() type:" + type.ToString());

            Func<ByteWriter, object> result = null;

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
                    Func<ByteWriter, object> arrayReadFunction = (ByteWriter writer) => { return writer.ReadArray(itemType); };
                    return arrayReadFunction;
                }
                else if (dictionaryTypes.Contains(type))
                {
                    Func<ByteWriter, object> dictionaryReadFunction = (ByteWriter writer) => { return writer.ReadDictionary(type); };
                    return dictionaryReadFunction;
                }
                else if (IListTypes.Contains(type))
                {
                    Func<ByteWriter, object> listReadFunction = (ByteWriter writer) => { return writer.ReadList(type); };
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
                    Func<ByteWriter, object> arrayReadFunction = (ByteWriter writer) => { return writer.ReadArray(itemType); };
                    return arrayReadFunction;
                }
                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    dictionaryTypes.Add(type);
                    Type[] genericArguments = type.GetGenericArguments();
                    IDictionaryToItemTypes[type] = new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);

                    Func<ByteWriter, object> dictionaryReadFunction = (ByteWriter writer) => { return writer.ReadDictionary(type); };
                    return dictionaryReadFunction;
                }
                else if (typeof(IList).IsAssignableFrom(type))
                {
                    IListTypes.Add(type);
                    Type itemType = type.GetGenericArguments()[0];
                    IListToItemType[type] = itemType;
                    Func<ByteWriter, object> listReadFunction = (ByteWriter writer) => { return writer.ReadList(type); };
                    return listReadFunction;
                }
                else
                {
                    //Is compound object
                    compoundTypes.Add(type);
                    Func<ByteWriter, object> compundReadFunction = (ByteWriter writer) => { return writer.readCompoundObject(type); };
                    return compundReadFunction;
                }
            }
        }

        //Reflection write
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

        private void readBytesToStorage(int count)
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
            if(data == null)
            {
                Write(FALSE_BYTE);
                return;
            }
            else
            {
                Write(TRUE_BYTE);
                Write(data.Length);
                Write(Encoding.UTF8.GetBytes(data));
            }
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

            if(sizeInBytes <= storage.Length)
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

        //Simple lists writes

        public void Write(Array data)
        {
            if (!isSerializeableArray(data)) //checks for rare multi-dimensional arrays with wierd zero-length dimensions.
                return;

            int count = data.Length;

            Type arrayType = data.GetType();
            Type itemType = arrayType.GetElementType();

            Action<ByteWriter, object> writeFunc = GetWriterFunction(itemType);
            if (writeFunc == null)
            {
                Debug.LogError("Can't serialize array! unsupported item type used: " + itemType);
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

        public Array ReadArray(Type itemType)
        {
            ////Get the item type
            //Type itemType;
            //if (!IListToItemType.TryGetValue(arrayType, out itemType))
            //{
            //    itemType = arrayType.GetElementType();
            //    IListToItemType[arrayType] = itemType;
            //}

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
            Func<ByteWriter, object> itemReaderFunction = GetReaderFunction(itemType);
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
        
        public void Write(IList data)
        {
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
            
            Write(items);
        }
        
        public IList ReadList(Type listType)
        {
            Type itemType;
            if (!IListToItemType.TryGetValue(listType, out itemType))
            {
                Type[] genericArguments = listType.GetGenericArguments();
                itemType = genericArguments[0];
                IListToItemType[listType] = itemType;
            }

            Array items = ReadArray(itemType);

            IList instance = (IList)Instantiate(listType);
            foreach (var item in items)
            {
                instance.Add(item);
            }

            return instance;
        }

        public void Write(IDictionary data)
        {
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
            Write(keys);
            Write(values);
        }
        
        public IDictionary ReadDictionary(Type dictionaryType)
        {
            //Get the item type
            KeyValuePair<Type, Type> itemType;
            if (!IDictionaryToItemTypes.TryGetValue(dictionaryType, out itemType))
            {
                Type[] genericArguments = dictionaryType.GetGenericArguments();
                itemType = new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);
                IDictionaryToItemTypes[dictionaryType] = itemType;
            }
            
            //Instantiate dictionary
            IDictionary instance = (IDictionary)Instantiate(dictionaryType);

            //read item count
            int itemCount = ReadInt();

            //read items
            Array keys = ReadArray(itemType.Key);
            Array values = ReadArray(itemType.Value);

            for (int i = 0; i < keys.Length; i++)
            {
                instance.Add(keys.GetValue(i), values.GetValue(i));
            }

            return instance;
        }

        private void writeCompoundObject(object parent)
        {
            if(parent == null)
            {
                Write(FALSE_BYTE);
                return;
            }

            //write hasValue flag
            Write(TRUE_BYTE);

            Type parentType = parent.GetType();
            Debug.Log("writeCompoundObject() type: " + parentType);

            var members = GetCachedSerializableMembers(parentType).OrderBy(x => x.MetadataToken);

            //Write unique id of this compound class/struct
            Write(parentType.FullName);

            //Write hash of all membersinfos to detect trying to deserialize 
            //two different versions of the same compund class/struct
            
            foreach (var memberInfo in members)
            {
                Type memberType = memberInfo.GetMemberType();
                var writerFunction = GetWriterFunction(memberType);

                object memberValue = GetValue(memberInfo, parent);

                Debug.Log("writeCompoundObject() member: " + memberInfo.Name);
                Debug.Log("writeCompoundObject() member value: " + (memberValue == null ? "null" : memberValue.ToString()));
                
                writerFunction(this, memberValue);
            }

        }
        
        private object readCompoundObject(Type type)
        {
            bool hasValue = ReadBool();
            if(!hasValue)
                return null;

            Debug.Log("readCompoundObject() type: " + type);

            var members = GetCachedSerializableMembers(type).OrderBy(x => x.MetadataToken);

            //Write unique id of this compound class/struct
            string typeFullName = ReadString();
            if(typeFullName != type.FullName)
            {
                Debug.LogFormat("Cannot read object, save object type ({0}) does not match requested type ({1})!", typeFullName, type.FullName);
            }

            //Read hash of all membersinfos to detect trying to deserialize 
            //two different versions of the same compund class/struct
            object instance = Instantiate(type);

            foreach (var memberInfo in members)
            {
                Type memberType = memberInfo.GetMemberType();
                Func<ByteWriter, object> readerunction = GetReaderFunction(memberType);

                object memberValue = readerunction(this);

                Debug.Log("writeCompoundObject() member: " + memberInfo.Name);
                Debug.Log("writeCompoundObject() member value: " + (memberValue == null ? "null" : memberValue.ToString()));

                SetValue(memberInfo, instance, memberValue);
            }

            return instance;
        }

        //public T Deserialize<T>(byte[] bytes)
        //{
        //    Type targetType = typeof(T);

        //    Func<ByteWriter, object> readFunction;
        //    if(readFunctions.TryGetValue(targetType, out readFunction))
        //    {
        //        //is primitive
        //        return (T)readFunction(this);
        //    }


        //    var members = GetSerializableMembers(targetType);

        //    foreach (var memberInfo in members)
        //    {

        //    }
        //}

        public byte[] ToArray()
        {
            return memoryStream.ToArray();
        }

        public void Dispose()
        {
            memoryStream.Dispose();
        }

        //General Helpers
        private bool isMaxIndex(int[] indexes, int[] maxIndexes)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] < maxIndexes[i])
                    return false;
            }

            return true;
        }

        private bool isSerializeableArray(Array data)
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
                    Debug.Log("Can't serialize array! one of the array dimensions is 0 length, dimension index: " + i);
                    return false;
                }
            }

            return true;
        }

        //Reflection Helpers
        static Dictionary<IntPtr, MemberInfo[]> memberCache = new Dictionary<IntPtr, MemberInfo[]>();
        static Dictionary<IntPtr, Func<object>> constructorCache = new Dictionary<IntPtr, Func<object>>();
        static Dictionary<IntPtr, Func<object, object>> getterCache = new Dictionary<IntPtr, Func<object, object>>();
        static Dictionary<IntPtr, Action<object, object>> setterCache = new Dictionary<IntPtr, Action<object, object>>();
        static Dictionary<IntPtr, MethodHandler> methodCache = new Dictionary<IntPtr, MethodHandler>();

        public static IEnumerable<MemberInfo> GetSerializableMembers(Type type)
        {
            return type.GetFields(ReflectionHelper.AllFieldsAndProperties).Cast<MemberInfo>();

            //return type.GetProperties(ReflectionHelper.AllFieldsAndProperties)
            //    .Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null && p.GetGetMethod().GetParameters().Length == 0)
            //    .Cast<MemberInfo>()
            //    .Union(type.GetFields(ReflectionHelper.AllFieldsAndProperties).Cast<MemberInfo>());
        }

        public static IEnumerable<MemberInfo> GetCachedSerializableMembers(Type type)
        {
            MemberInfo[] properties;
            if (!memberCache.TryGetValue(type.TypeHandle.Value, out properties))
            {
                properties = GetSerializableMembers(type).ToArray();
                memberCache.Add(type.TypeHandle.Value, properties);
            }
            
            return properties;
        }

        public static object GetValue(MemberInfo memberInfo, object instance)
        {
            Func<object, object> getter;
            if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                var key = propertyInfo.GetGetMethod().MethodHandle.Value;
                if (!getterCache.TryGetValue(key, out getter))
                    getterCache.Add(key, getter = EmitHelper.CreatePropertyGetterHandler(propertyInfo));
            }
            else if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;
                if (!getterCache.TryGetValue(fieldInfo.FieldHandle.Value, out getter))
                    getterCache.Add(fieldInfo.FieldHandle.Value, getter = EmitHelper.CreateFieldGetterHandler(fieldInfo));
            }
            else
                throw new NotImplementedException();

            return getter(instance);
        }

        public static void SetValue(MemberInfo memberInfo, object instance, object value)
        {
            Action<object, object> setter;
            if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                var key = propertyInfo.GetSetMethod().MethodHandle.Value;
                if (!setterCache.TryGetValue(key, out setter))
                    setterCache.Add(key, setter = EmitHelper.CreatePropertySetterHandler(propertyInfo));
            }
            else if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;
                if (!setterCache.TryGetValue(fieldInfo.FieldHandle.Value, out setter))
                    setterCache.Add(fieldInfo.FieldHandle.Value, setter = EmitHelper.CreateFieldSetterHandler(fieldInfo));
            }
            else
                throw new NotImplementedException();

            setter(instance, value);
        }

        public static object Instantiate(Type type)
        {
            Func<object> constructor;
            if (!constructorCache.TryGetValue(type.TypeHandle.Value, out constructor))
            {
                constructor = EmitHelper.CreateParameterlessConstructorHandler(type);
                constructorCache.Add(type.TypeHandle.Value, constructor);
            }
            
            return constructor();
        }


        //public static bool IsICollection(Type type)
        //{
        //    return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        //}
        
    }
}

