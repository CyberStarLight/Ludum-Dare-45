using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

public class RobustSerializer : SimpleByteWriter
{
    public const string TYPE_VERSIONS_FOLDER = "TypeVersions";
    public const string TYPE_VERSIONS_EXTENSION = "def";

    private static string PathToDataFolder;
    private static Dictionary<Type, TypeDefinition> CurrentDefinitions = new Dictionary<Type, TypeDefinition>(); //contains the currently compiled definitions of compound objects
    private static Dictionary<string, HashSet<TypeDefinition>> OldDefinitions = new Dictionary<string, HashSet<TypeDefinition>>(); // Key = [TypeFullName - VersionNumber], contains old and current versions deserialized from files on disk

    private static Dictionary<Type, PrimitiveType> PrimitiveTypes = new Dictionary<Type, PrimitiveType>() {
        { typeof(byte), PrimitiveType.Byte },
        { typeof(sbyte), PrimitiveType.Sbyte },
        { typeof(bool), PrimitiveType.Bool },
        { typeof(char), PrimitiveType.Char },
        { typeof(short), PrimitiveType.Short },
        { typeof(ushort), PrimitiveType.UShort },
        { typeof(int), PrimitiveType.Int },
        { typeof(uint), PrimitiveType.UInt },
        { typeof(long), PrimitiveType.Long },
        { typeof(ulong), PrimitiveType.ULong },
        { typeof(float), PrimitiveType.Float },
        { typeof(double), PrimitiveType.Double },
        { typeof(decimal), PrimitiveType.Decimal },
        { typeof(string), PrimitiveType.String },
        //Nullables
        { typeof(byte?), PrimitiveType.NullableByte },
        { typeof(sbyte?), PrimitiveType.NullableSbyte },
        { typeof(bool?), PrimitiveType.NullableBool },
        { typeof(char?), PrimitiveType.NullableChar },
        { typeof(short?), PrimitiveType.NullableShort },
        { typeof(ushort?), PrimitiveType.NullableUshort },
        { typeof(int?), PrimitiveType.NullableInt },
        { typeof(uint?), PrimitiveType.NullableUint },
        { typeof(long?), PrimitiveType.NullableLong },
        { typeof(ulong?), PrimitiveType.NullableUlong },
        { typeof(float?), PrimitiveType.NullableFloat },
        { typeof(double?), PrimitiveType.NullableDouble },
        { typeof(decimal?), PrimitiveType.NullableDecimal },
    };

    //private static Action<ByteWriter, object> compundWriteFunction = (ByteWriter writer, object o) => { ((RobustSerializer)writer).writeCompoundObject(o); };
    protected static new Action<SimpleByteWriter, object> ArrayWriteFunction = (SimpleByteWriter writer, object o) => { writer.Write((Array)o, GetWriterFunction); };
    protected static new Action<SimpleByteWriter, object> IDictionaryWriteFunction = (SimpleByteWriter writer, object o) => { writer.Write((IDictionary)o, GetWriterFunction); };
    protected static new Action<SimpleByteWriter, object> IListWriteFunction = (SimpleByteWriter writer, object o) => { writer.Write((IList)o, GetWriterFunction); };

    public RobustSerializer() { }
    public RobustSerializer(int capacity) : base(capacity) { }
    public RobustSerializer(byte[] source) : base(source) { }

    public static void Initialize(string pathToDataFolder)
    {
        PathToDataFolder = pathToDataFolder;
        string pathToTypeVersions = Files.BuildPath(PathToDataFolder, TYPE_VERSIONS_FOLDER);

        if (!Directory.Exists(pathToTypeVersions))
        {
            Directory.CreateDirectory(PathToDataFolder);
            return;
        }

        string[] definitionFilesPaths = Directory.GetFiles(pathToTypeVersions);
        foreach (var filePath in definitionFilesPaths)
        {
            TypeDefinition definition = TypeDefinition.FromBytes(File.ReadAllBytes(filePath));
            HashSet<TypeDefinition> set;
            if (!OldDefinitions.TryGetValue(definition.ParentType, out set))
            {
                set = new HashSet<TypeDefinition>();
                OldDefinitions.Add(definition.ParentType, set);
            }

            set.Add(definition);
        }
    }

    public void writeCompoundObject<T>(T parent, TextWriter log = null)
    {
        writeCompoundObject(parent, typeof(T), log);
    }

    public void writeCompoundObject(object parent, Type objType, TextWriter log = null)
    {
        Type parentType = parent == null ? objType : parent.GetType();
        TypeDefinition typeDefinition = GetCurrentDefinition(parentType);

        //Write unique id of this compound class/struct
        Write(typeDefinition.ParentType);
        Write(typeDefinition.Version);

        log?.WriteLine("typeDefinition.ParentType: " + typeDefinition.ParentType);
        log?.WriteLine("typeDefinition.Version: " + typeDefinition.Version);

        //Check if value needs serializing, if it's deafault(T) no need to serialize any bytes.
        if (parent == GetDefaultValue(parentType))
        {
            //write noValue flag
            Write(false);
            log?.WriteLine("Value == DefaultValue");
            return;
        }
        else
        {
            //write hasValue flag
            Write(true);
            log?.WriteLine("Value != DefaultValue");
        }

        //Get serializeable fields/properties
        var members = GetCachedSerializableMembers(parentType).OrderBy(x => x.Name).ToArray();

        //Write(members.Length);
        //log?.WriteLine("members.Length: " + members.Length);

        //Write hash of all membersinfos to detect trying to deserialize 
        //two different versions of the same compund class/struct

        foreach (var memberInfo in members)
        {
            Type memberType = memberInfo.GetMemberType();
            object memberValue = GetValue(memberInfo, parent);

            var writerFunction = GetWriterFunction(memberType);
            writerFunction(this, memberValue);
        }
    }

    public T readCompoundObject<T>(TextWriter log = null)
    {
        return (T)readCompoundObject(typeof(T));
    }

    public object readCompoundObject(Type type, TextWriter log = null)
    {
        Type requestedType = type;
        string storedTypeName = ReadString();
        string requestedTypeName = type.ToString();

        if (storedTypeName != requestedTypeName)
            throw new InvalidOperationException(string.Format("Stored type ({0}) does not match the type requested ({1})!", storedTypeName, requestedTypeName));

        //Get definition versions for the type
        int versionNumber = ReadInt();

        //avoid deserialization if serialized as default(T)
        bool HasNonDefaultValue = ReadBool();
        if (HasNonDefaultValue)
        {
            log?.WriteLine("Value != DefaultValue");
        }
        else
        {
            log?.WriteLine("Value == DefaultValue");
            return GetDefaultValue(requestedType);
        }

        TypeDefinition currentTypeDefinition = GetCurrentDefinition(requestedType);
        TypeDefinition? oldDefinition = GetOldDefinition(storedTypeName, versionNumber);

        if (!oldDefinition.HasValue)
            throw new InvalidOperationException(string.Format("Stored type version ({0}) cannot be found.", versionNumber));

        var storedTypeDefinition = oldDefinition.Value;

        var requestedTypeMembers = GetCachedSerializableMembers(requestedType).OrderBy(x => x.Name).ToList();
        object instance = Instantiate(requestedType);
        if (storedTypeDefinition == currentTypeDefinition)
        {
            //Deserialize as usual
            foreach (var memberInfo in requestedTypeMembers)
            {
                Type memberType = memberInfo.GetMemberType();
                Func<SimpleByteWriter, object> readerunction = GetReaderFunction(memberType);

                object memberValue = readerunction(this);

                SetValue(memberInfo, instance, memberValue);
            }

            return instance;
        }
        else
        {
            //Serialized with an old definition, cannot deserialize as usual
            var storedMembers = storedTypeDefinition.Members.OrderBy(x => x.FieldName).ToArray();
            foreach (var storedMember in storedMembers)
            {
                //Make sure we always read all the stored values, in the right order, even when not used! we must do that so the byte position of the next fields will be correct
                Type storedMemberType = storedMember.OriginalType;
                Func<SimpleByteWriter, object> readerunction = GetReaderFunction(storedMemberType);
                object storedValue = readerunction(this);

                var target = requestedTypeMembers.FirstOrDefault(x => x.Name == storedMember.FieldName);
                if (target == null)
                {
                    //member was deleted or renamed.search through field attributes that mark renamed fields, and redirect to the new field if rename detected
                    foreach (var requestedMember in requestedTypeMembers)
                    {
                        if (Attribute.IsDefined(requestedMember, typeof(RenamedAttribute)))
                        {
                            RenamedAttribute attribute = requestedMember.GetCustomAttribute<RenamedAttribute>();
                            if (attribute != null && attribute.OldNames != null)
                            {
                                string newName = attribute.OldNames.FirstOrDefault(x => x == storedMember.FieldName);
                                if (newName != null)
                                {
                                    //found the renamed field on the target type, redirect to it
                                    target = requestedMember;
                                    break;
                                }
                            }
                        }
                    }

                    if (target == null)
                    {
                        //Target was deleted or user forgot to set a renamed attribute, skip field
                        continue;
                    }
                }

                Type targetMemberType = target.GetMemberType();
                string targetTypeName = targetMemberType.ToString();


                if (targetTypeName == storedMember.MemberType)
                {
                    //Types match, deserialize as usual
                    SetValue(target, instance, storedValue);
                }
                else
                {
                    //member type changed, check if a casting is possible
                    if (storedValue != null) //if stored val is null, we can leave target as default
                    {
                        if (
                            !Conversions.reflectionConversionDic.TryGetValue(storedMemberType, out Dictionary<Type, Func<object, object>> convertDic) ||
                            !convertDic.TryGetValue(targetMemberType, out Func<object, object> convertToTarget)
                            )
                        {
                            //No valid predefined convertion found
                            if (storedValue.GetType() == targetMemberType)
                            {
                                //Probably a conversion between a Nullable<T> and T, because when Nullable<T> that is not null is cast to object it turns into T. anyway, if both are same type for any reason, just set as usual.
                                SetValue(target, instance, storedValue);
                            }
                            else if (targetMemberType.TryCast(storedValue, out object storedValueCasted))
                            {
                                //Cast was succesful
                                SetValue(target, instance, storedValueCasted);
                            }
                            else
                            {
                                //cannot implicitly cast stored value to target value, check if it implements IConvertible
                                TypeConverter typeConverter = TypeDescriptor.GetConverter(storedMemberType);
                                if (typeConverter != null && typeConverter.CanConvertTo(targetMemberType))
                                {
                                    //Conversion was succesful
                                    SetValue(target, instance, typeConverter.ConvertTo(storedValue, targetMemberType));
                                }
                            }
                        }
                        else
                        {
                            //Found a predefined conversion function, use it
                            SetValue(target, instance, convertToTarget(storedValue));
                        }
                    }
                }
            }

            return instance;
        }

    }

    private TypeDefinition? GetOldDefinition(string parentType, int version)
    {
        HashSet<TypeDefinition> versionsList;
        if (!OldDefinitions.TryGetValue(parentType, out versionsList))
        {
            return null;
        }

        foreach (var definition in versionsList)
        {
            if (definition.Version == version)
                return definition;
        }

        return null;
    }

    private TypeDefinition GetCurrentDefinition(Type parentType)
    {
        TypeDefinition typeDefinition;
        if (!CurrentDefinitions.TryGetValue(parentType, out typeDefinition))
        {
            TypeDefinition membersOnly = new TypeDefinition(parentType);
            string parentTypeName = parentType.ToString();
            bool isNewVersion = false;

            HashSet<TypeDefinition> versionsList;
            if (!OldDefinitions.TryGetValue(parentTypeName, out versionsList))
            {
                versionsList = new HashSet<TypeDefinition>();
                OldDefinitions.Add(parentTypeName, versionsList);
                isNewVersion = true;
            }
            else if (!versionsList.Contains(membersOnly))
            {
                isNewVersion = true;
            }
            else
            {
                typeDefinition = versionsList.First(x => x == membersOnly);
            }

            if (isNewVersion)
            {
                //Serialize version to file
                string fileName = parentTypeName.Length <= 32 ? parentTypeName : Hash.MD5_Hex(parentTypeName);
                string pathToFolder = Files.BuildPath(PathToDataFolder, TYPE_VERSIONS_FOLDER);

                string[] existingVersionFiles = Files.GetAutoIncrementedFiles(
                    pathToFolder,
                    fileName,
                    TYPE_VERSIONS_EXTENSION
                    );

                int version = existingVersionFiles.Length + 1;

                string filePath = Files.AutoIncrementFileName(
                    pathToFolder,
                    fileName,
                    version,
                    TYPE_VERSIONS_EXTENSION
                    );

                Directory.CreateDirectory(pathToFolder);

                typeDefinition = new TypeDefinition(parentTypeName, membersOnly.Members, version);
                File.WriteAllBytes(filePath, typeDefinition.ToBytes());

                //Add to OldDefinitions
                versionsList.Add(typeDefinition);
            }

            //Add to CurrentDefinitions
            CurrentDefinitions.Add(parentType, typeDefinition);
        }

        return typeDefinition;
    }

    //Write functions search
    private new static Action<SimpleByteWriter, object> GetWriterFunction(Type type)
    {
        //Debug.Log("ByteWriter GetWriterFunction() type:" + type.ToString());

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
                compoundTypes.Add(type);
                Action<SimpleByteWriter, object> compundWriteFunction = (SimpleByteWriter writer, object o) => { ((RobustSerializer)writer).writeCompoundObject(o, type); };
                return compundWriteFunction;
            }
        }
    }

    //Read functions search
    private new static Func<SimpleByteWriter, object> GetReaderFunction(Type type)
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
                Func<SimpleByteWriter, object> arrayReadFunction = (SimpleByteWriter writer) => { return writer.ReadArray(itemType, GetReaderFunction); };
                return arrayReadFunction;
            }
            else if (dictionaryTypes.Contains(type))
            {
                Func<SimpleByteWriter, object> dictionaryReadFunction = (SimpleByteWriter writer) => { return writer.ReadDictionary(type, GetReaderFunction); };
                return dictionaryReadFunction;
            }
            else if (IListTypes.Contains(type))
            {
                Func<SimpleByteWriter, object> listReadFunction = (SimpleByteWriter writer) => { return writer.ReadList(type, GetReaderFunction); };
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
                Func<SimpleByteWriter, object> arrayReadFunction = (SimpleByteWriter writer) => { return writer.ReadArray(itemType, GetReaderFunction); };
                return arrayReadFunction;
            }
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                dictionaryTypes.Add(type);
                Type[] genericArguments = type.GetGenericArguments();
                IDictionaryToItemTypes[type] = new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);

                Func<SimpleByteWriter, object> dictionaryReadFunction = (SimpleByteWriter writer) => { return writer.ReadDictionary(type, GetReaderFunction); };
                return dictionaryReadFunction;
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                IListTypes.Add(type);
                Type itemType = type.GetGenericArguments()[0];
                IListToItemType[type] = itemType;
                Func<SimpleByteWriter, object> listReadFunction = (SimpleByteWriter writer) => { return writer.ReadList(type, GetReaderFunction); };
                return listReadFunction;
            }
            else
            {
                //Is compound object
                compoundTypes.Add(type);
                Func<SimpleByteWriter, object> compundReadFunction = (SimpleByteWriter writer) => { return ((RobustSerializer)writer).readCompoundObject(type); };
                return compundReadFunction;
            }
        }
    }

    public new byte[] ToArray()
    {
        return memoryStream.ToArray();
    }

    public new void Dispose()
    {
        memoryStream.Dispose();
    }


    private enum PrimitiveType
    {
        Byte,
        Sbyte,
        Bool,
        Char,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float,
        Double,
        Decimal,
        String,
        //Nullable
        NullableByte,
        NullableSbyte,
        NullableBool,
        NullableChar,
        NullableShort,
        NullableUshort,
        NullableInt,
        NullableUint,
        NullableLong,
        NullableUlong,
        NullableFloat,
        NullableDouble,
        NullableDecimal,
    }

    public struct TypeDefinition
    {
        public string ParentType;
        public MemberDefinition[] Members;
        public int Version;

        public TypeDefinition(Type type)
        {
            ParentType = type.ToString();

            Members =
                GetCachedSerializableMembers(type)
                .Select(x => new MemberDefinition(x.GetMemberType(), x.Name))
                .ToArray();

            Version = -1;
        }

        public TypeDefinition(string typeName, MemberDefinition[] members, int version)
        {
            ParentType = typeName;
            Members = members;
            Version = version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            else if (!(obj is TypeDefinition))
            {
                return false;
            }

            TypeDefinition other = (TypeDefinition)obj;

            if (ParentType != other.ParentType || Members.Length != other.Members.Length)
                return false;

            for (int i = 0; i < Members.Length; i++)
            {
                if (Members[i] != other.Members[i])
                    return false;
            }

            return true;
        }

        public static bool operator ==(TypeDefinition a, TypeDefinition b)
        {
            if (a.ParentType != b.ParentType || a.Members.Length != b.Members.Length)
                return false;

            for (int i = 0; i < a.Members.Length; i++)
            {
                if (a.Members[i] != b.Members[i])
                    return false;
            }

            return true;
        }

        public static bool operator !=(TypeDefinition a, TypeDefinition b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ParentType.GetHashCode();

                foreach (var member in Members)
                {
                    hash = hash * 23 + member.GetHashCode();
                }

                return hash;
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(ParentType);
            result.AppendLine(Version);
            result.AppendLine(Members.Length);
            result.AppendLine();

            foreach (var member in Members)
            {
                result.AppendLine(member.MemberType);
                result.AppendLine(member.FieldName);
            }

            return result.ToString();
        }

        public byte[] ToBytes()
        {
            using (var writer = new SimpleByteWriter())
            {
                writer.Write(ParentType);
                writer.Write(Version);

                writer.Write(Members.Length);

                foreach (var member in Members)
                {
                    member.WriteToByteWriter(writer);
                }

                return writer.ToArray();
            }
        }

        public static TypeDefinition FromBytes(byte[] bytes)
        {
            string parentType;
            int version;
            MemberDefinition[] members;

            using (var writer = new SimpleByteWriter(bytes))
            {
                parentType = writer.ReadString();
                version = writer.ReadInt();
                members = new MemberDefinition[writer.ReadInt()];

                for (int i = 0; i < members.Length; i++)
                {
                    members[i] = MemberDefinition.ReadFromByteWriter(writer);
                }
            }

            return new TypeDefinition()
            {
                ParentType = parentType,
                Version = version,
                Members = members,
            };
        }
    }

    public struct MemberDefinition
    {
        public string MemberType;
        public string FieldName;
        public Type OriginalType;

        public MemberDefinition(Type memberType, string fieldName)
        {
            MemberType = memberType.ToString();
            FieldName = fieldName;
            OriginalType = memberType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            else if (!(obj is MemberDefinition))
            {
                return false;
            }

            MemberDefinition otherMember = (MemberDefinition)obj;

            return MemberType == otherMember.MemberType && FieldName == otherMember.FieldName;
        }

        public static bool operator ==(MemberDefinition a, MemberDefinition b)
        {
            return a.MemberType == b.MemberType && a.FieldName == b.FieldName;
        }

        public static bool operator !=(MemberDefinition a, MemberDefinition b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + MemberType.GetHashCode();
                hash = hash * 23 + FieldName.GetHashCode();
                hash = hash * 23 + OriginalType.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(MemberType);
            result.AppendLine(FieldName);

            return result.ToString();
        }

        public void WriteToByteWriter(SimpleByteWriter writer)
        {
            writer.Write(MemberType);
            writer.Write(FieldName);
            writer.Write(OriginalType);
        }

        public static MemberDefinition ReadFromByteWriter(SimpleByteWriter writer)
        {
            return new MemberDefinition()
            {
                MemberType = writer.ReadString(),
                FieldName = writer.ReadString(),
                OriginalType = writer.ReadType()
            };
        }

        public byte[] ToBytes()
        {
            using (var writer = new SimpleByteWriter())
            {
                WriteToByteWriter(writer);

                return writer.ToArray();
            }
        }

        public static MemberDefinition FromBytes(byte[] bytes)
        {
            using (var writer = new SimpleByteWriter(bytes))
            {
                return ReadFromByteWriter(writer);
            }
        }
    }


    //Reflection Helpers
    static Dictionary<IntPtr, MemberInfo[]> memberCache = new Dictionary<IntPtr, MemberInfo[]>();
    static Dictionary<IntPtr, Func<object>> constructorCache = new Dictionary<IntPtr, Func<object>>();
    static Dictionary<IntPtr, Func<object, object>> getterCache = new Dictionary<IntPtr, Func<object, object>>();
    static Dictionary<IntPtr, Action<object, object>> setterCache = new Dictionary<IntPtr, Action<object, object>>();

    public static IEnumerable<MemberInfo> GetSerializableMembers(Type type)
    {
        var skipProperties =
                   type.GetProperties()
                   .Where(x => Attribute.IsDefined(x, typeof(DoNotSerializeAttribute)))
                   .Select(x => string.Format("<{0}>k__BackingField", x.Name))
                   .ToHashSet();

        return type.GetFields(ReflectionHelper.AllFieldsAndProperties).Cast<MemberInfo>().Where(x => !Attribute.IsDefined(x, typeof(DoNotSerializeAttribute)) && !skipProperties.Contains(x.Name));
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
}
