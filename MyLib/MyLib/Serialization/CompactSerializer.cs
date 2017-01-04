using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using MyLib.Modern;
using MyLib.Algoriphms;

namespace MyLib.Serialization
{
    public partial class CompactSerializer
    {
        SerializationTypes serializationTypes;
        bool includeTypes;
        IEnumerable<Assembly> assemblies = new Assembly[0];
        CSOptions options;
        char splitter = '&';
        public CompactSerializer(CSOptions options)
        {
            this.options = options;
        }
        public CompactSerializer(SerializeBinders binders)
        {
            this.options = CSOptions.None;
        }
        public CompactSerializer(char splitter = '&', CSOptions options = CSOptions.None, SerializeBinders binders = null)
        {
            this.splitter = splitter;
            this.options = options;
        }
        public string Serialize(object data, Type binder = null)
        {
            serializationTypes = new SerializationTypes();
            if ((options & CSOptions.WithTypes) != 0)
                includeTypes = true;
            else if((options & CSOptions.NoTypes) == 0)
            {
                includeTypes = HasDerivedFields(data, binder);
            }
            var result = Serialize(data, false, binder);
            if (includeTypes)
                return Serialize(serializationTypes, false, null) + result;
            return result;
        }
        public void Serialize(object data, StreamWriter stream, Type binder = null)
        {
            stream.Write(Serialize(data, binder));
        }
        string Serialize(object data, bool includeType, Type binder)
        {

            var pre = data.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<PreSerializeAttribute>() != null);
            foreach (var m in pre)
                m.Invoke(data, new object[] { });

            Type type = Bind(ref data, binder);
            SerializedType stype = new SerializedType(type);
            StringBuilder sb = new StringBuilder();

            if (includeTypes)
            {
                if (includeType)
                    sb.Append(serializationTypes.AddType(type.FullName));
                sb.Append(':');
            }
            if (stype.IsSerializable)
            {
                if (stype.IsIEnumerable || stype.IsArray)
                {
                    var numerable = stype.GetElements(data);
                    bool withType = false;
                    if (includeTypes)
                        if (!HasDerivedElements((IEnumerable)data))
                            withType = true;
                    sb.Append(numerable.Count());
                    Split(sb);
                    if (numerable.Count() > 0)
                        sb.Append(numerable.Select(e => Serialize(e, withType, null)).ToString(""));

                }
                else if (stype.IsString && ((options & CSOptions.SafeString) != 0))
                {
                    sb.Append(((string)data).Length);
                    Split(sb);
                    sb.Append(data.ToString());
                    Split(sb);
                }
                else if (stype.IsComVisible)
                {
                    sb.Append(data.ToString());
                    Split(sb);
                }
                else
                {
                    Write(sb, data, stype.GetTypeFields());
                }
            }
            else {
                Tuple<Type, VersionAttribute> tv;
                if(type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Select(t => new Tuple<Type, VersionAttribute>(t, t.GetCustomAttribute<VersionAttribute>()))
                    .WithMax(t => t.Item2.version, t => t.Item2 != null, out tv))
                {
                    sb.Append(tv.Item2.version.ToString()); Split(sb);

                    sb.Append(Serialize(data, false, tv.Item1));
                }
                else
                    Write(sb, data, stype.GetTypeAll());
            }
            return sb.ToString();
        }
        void Write(StringBuilder sb, object data, IEnumerable<UField> ps)
        {
            foreach (var field in ps)
            {
                object value = field.GetValue(data);
                Type type = Bind(field, ref value);
                sb.Append(Serialize(value, type != value.GetType(), null));
            }
        }
        void Split(StringBuilder sb)
        {
            sb.Append(splitter);
        }

        public T Deserialize<T>(string source, Type binder = null)
        {
            using (MemoryStream ms = new MemoryStream(source.Select(c => (byte)c).ToArray()))
            {
                using (StreamReader sr = new StreamReader(ms))
                {
                    return Deserialize<T>(sr, binder);
                }
            }
        }

        public T Deserialize<T>(StreamReader reader, Type binder = null)
        {
            includeTypes = ((char) reader.Peek()) == ':';
            if (includeTypes)
            {
                assemblies = GetAssemblies(typeof(T));
                serializationTypes = (SerializationTypes) Deserialize(typeof(SerializationTypes), null, reader);
            }

            T result = (T)Deserialize(typeof(T), binder, reader);
            return result;
        }
        object Deserialize(Type ftype, Type binder, StreamReader reader)
        {
            if (includeTypes)
            {
                string sub;
                Extract(reader, out sub);
                if (sub != "")
                    ftype = GetType(serializationTypes.GetType(int.Parse(sub)));
            }
            if(binder == null)
                binder = Bind(ftype);

            var stype = new SerializedType(binder);
            SerializeTemp result = new SerializeTemp(stype);

            if (stype.IsSerializable)
            {
                if (stype.IsIEnumerable || stype.IsArray)
                {
                    Type elementType = null;
                    if (binder.IsArray)
                        elementType = binder.GetElementType();
                    else
                        elementType = binder.GetGenericArguments().First();

                    string line = "";
                    Extract(reader, out line);
                    int length = int.Parse(line);
                    for (int i = 0; i < length; i++)
                        result.AddElement(Deserialize(elementType, null, reader));
                }
                else if(stype.IsString && ((options & CSOptions.SafeString) != 0))
                {
                    string line;
                    Extract(reader, out line);
                    int length = int.Parse(line);
                    Extract(reader, out line, length);
                    result.SetValue(line);
                }
                else if (stype.IsComVisible)
                {
                    string line;
                    Extract(reader, out line);
                    result.SetValue(GetValue(binder, line));
                }
                else
                {
                    foreach (var sa in stype.GetTypeFields())
                        result.AddElement(Deserialize(sa.fieldType, Bind(sa), reader));
                }

                return result.GetResult();
            }
            else
            {
                var vs = binder.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Select(t => new Tuple<Type, VersionAttribute>(t, t.GetCustomAttribute<VersionAttribute>()))
                    .Where(t => t.Item2 != null).ToArray();
                if (vs.Length == 0)
                {
                    foreach (var ca in stype.GetTypeConstructorArgs())
                        result.AddArg(Deserialize(Bind(ca), null, reader));
                    foreach (var ad in stype.GetTypeFields())
                        result.AddElement(Deserialize(Bind(ad), null, reader));

                    var value = result.GetResult();
                    var postSerialize = binder.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(m => m.GetCustomAttribute<PostDeserializeAttribute>() != null);
                    foreach (var m in postSerialize)
                        m.Invoke(value, null);

                    return value is IBinder ? ((IBinder)value).GetResult() : value;
                }
                else
                {
                    string line;
                    Extract(reader, out line);
                    int v = int.Parse(line);
                    var value = Deserialize(binder, vs.First(t => t.Item2.version == v).Item1, reader);
                    return value is IBinder ? ((IBinder)value).GetResult() : value;
                }
            }

        }
        Type GetType(string name)
        {
            Type type = null;
            foreach(var a in assemblies)
            {
                type = Type.GetType(name + ", " + a.FullName);// a.GetType(name);
                if (type != null)
                    return type;
            }
            return type;
        }

        IEnumerable<Assembly> GetAssemblies(Type type)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach(var t in new CustomTree<Type, Type>(type, t => Bind(t), t => GetFields(t).Where(f => f.fieldType != t && f.GetCustomAttribute<NoSerializeBinder>() == null) .Select(f => Other.NonNull(Bind(f), f.fieldType))).ByElements().Select(n => n))
            {
                assemblies.Add(t.source.Assembly);
                assemblies.Add(t.node.Assembly);
                if(IsIEnumerable(t.node))
                    assemblies.AddRange(GetAssemblies(GetEnumerableElementType(t.node)));
            }

            return assemblies.Distinct();
        }


        static Type GetEnumerableElementType(Type enumerable)
        {
            if (enumerable.IsArray)
               return enumerable.GetElementType();
            else
                return enumerable.GetGenericArguments()[0];
        }
        static Type Bind(UField field, ref object obj)
        {
            Type binder = Bind(field);
            if (binder != field.fieldType)
            {
                return Bind(ref obj, binder);
            }
            else
                return binder;
        }
        static Type Bind(UField field)
        {
            var attr = field.GetCustomAttribute<SerializeBinderAttribute>();
            return attr != null ? attr.binder : Bind(field.fieldType);
        }
        static Type Bind(Type type)
        {
            var attr = type.GetCustomAttribute<SerializeBinderAttribute>();
            return attr != null ? attr.binder : type;
        }
        static Type Bind(ref object obj)
        {
            Type type = obj.GetType();
            Type binder = Bind(type);
            if (type != binder)
                obj = binder.GetConstructor(new Type[] { type }).Invoke(new object[] { obj });
            return binder;
        }
        static Type Bind(ref object obj, Type binder)
        {
            if (binder != null)
            {
                obj = binder.GetConstructors().First(c => c.GetParameters().Length == 1).Invoke(new object[] { obj });
                return binder;
            }
            else
            {
                return Bind(ref obj);
            }
        }

        static object GetValue(Type type, string value)
        {
            if (type.Equals(typeof(string)))
                return value;
            else if (value != null)
            {
                var parse = type.GetMethod("Parse", new Type[] { typeof(string) });
                if (parse != null)
                    return parse.Invoke(null, new object[] { value });
            }
            throw new Exception(type.ToString() + " has no Parse method");
        }
        static Type GetVersionSerialize(Type type, int version)
        {
            return type.GetNestedTypes().First(t => t.GetCustomAttribute<VersionAttribute>().version == version);
        }
        static bool IsIEnumerable(Type type)
        {
            return type.GetInterfaces().Has(typeof(IEnumerable)) && type != typeof(string);
        }
        static bool IsComVisible(Type type)
        {
            foreach (var a in type.GetCustomAttributes())
                if (a.GetType() == typeof(System.Runtime.InteropServices.ComVisibleAttribute))
                    return true;
            return false;
        }
        static IEnumerable<UField> GetConstructorArgs(Type type)
        {
            return UField.GetUFields(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<ConstructorArgAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<ConstructorArgAttribute>().position);
        }
        static IEnumerable<UField> GetAddons(Type type)
        {
            return UField.GetUFields(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<AddonAttribute>() != null);
        }
        static IEnumerable<UField> GetSerializedFields(Type type)
        {
            return UField.GetUFields(type, BindingFlags.Instance | BindingFlags.Public, true)
                .Where(f => f.GetCustomAttribute<NonSerializedAttribute>() == null);
        }
        bool HasDerivedElements(IEnumerable numerable, bool deep = false)
        {
            Type t = numerable.GetType();
            Type elementType = null;
            if (t.IsArray)
                elementType = t.GetElementType();
            else
                elementType = t.GetGenericArguments().First();
            foreach (var e in numerable)
                if (e.GetType() != elementType || (deep && HasDerivedFields(e, null)))
                    return false;
            return true;
        }
        bool HasDerivedFields(object obj, Type binder)
        {
            Bind(ref obj, binder);

            if (IsIEnumerable(obj.GetType()))
                if (!HasDerivedElements((IEnumerable)obj, true))
                    return true;
            Forest<FieldObjectTree> forest = new Forest<FieldObjectTree>(FieldObjectTree.CreateTrees(obj));

            foreach(var f in forest.ByElements())
            {
                if (!f.fieldObject.IsSameType())
                    return true;
                if (IsIEnumerable(f.fieldObject.btype))
                    if (!HasDerivedElements((IEnumerable)f.fieldObject.bvalue, true))
                        return true;
            }
            return false;
        }
        IEnumerable<object> GetTreeObjects(object obj)
        {
            Bind(ref obj);
            return TreeNode<object>.BuildCustomTree(obj, t => t, t =>
            {
                if (IsComVisible(t.GetType()))
                    return new object[0];
                Type binder = Bind(ref t);
                return GetFields(binder).Select(f => f.GetValue(t));
            })
            .ByElements().Select(n => n.item);
        }
        static IEnumerable<UField> GetFields(Type type)
        {
            if (type.IsSerializable)
            {
                if (IsIEnumerable(type) || IsComVisible(type))
                    return new UField[0];
                else
                    return GetSerializedFields(type);
            }
            else
                return GetConstructorArgs(type).Concat(GetAddons(type));
        }

        public static List<Type> CollectDynamicTypes(object obj)
        {
            List<Type> types = new List<Type>();

            Forest<CustomTree<FieldObject, FieldObject>> forest = new Forest<CustomTree<FieldObject, FieldObject>>(
                GetFields(obj.GetType()).Select(field => new CustomTree<FieldObject, FieldObject>(new FieldObject(field, field.GetValue(obj)), f => f,
                    f => GetFields(f.btype)
                    .Where(c => c.fieldType != f.btype)
                    .Select(t => new FieldObject(t, t.GetValue(f.bvalue))))
                )
                );

            foreach (var f in forest.ByElements())
            {
                Type type = f.source.bvalue.GetType();
                if (f.source.btype != type)
                {
                    if (!types.Contains(type))
                        types.Add(type);
                }
                if(IsIEnumerable(type))
                {
                    var en = (IEnumerable)f.source.bvalue;
                    var enType = GetEnumerableElementType(type);
                    foreach(var e in en)
                    {
                        Type eType = e.GetType();
                        if (eType != enType)
                            if (!types.Contains(eType))
                                types.Add(eType);
                        types.AddRange(CollectDynamicTypes(e));
                    }
                }
            }

            return types;
        }

        ExtractResult Extract(StreamReader sr, out string result)
        {
            StringBuilder r = new StringBuilder();
            while(!sr.EndOfStream)
            {
                char c = (char) sr.Read();
                if(c == splitter)
                {
                    result = r.ToString();
                    return ExtractResult.Value;
                }
                else if(c == ':')
                {
                    result = r.ToString();
                    return ExtractResult.Type;
                }
                r.Append(c);
            }

            //End of stream
            result = r.ToString();
            return ExtractResult.Value;
        }
        ExtractResult Extract(StreamReader sr, out string result, int length)
        {
            StringBuilder r = new StringBuilder();
            for(int i = 0; i < length && !sr.EndOfStream; i++)
                r.Append((char)sr.Read());
            if(!sr.EndOfStream)
                sr.Read();
            result = r.ToString();
            return ExtractResult.Value;
        }
        enum ExtractResult
        {
            Type,
            Value
        }

        struct FieldObject
        {
            public readonly Type btype;
            public readonly object bvalue;

            public FieldObject(UField field, object value)
            {
                bvalue = value;
                btype = Bind(field, ref bvalue);
            }

            public bool IsSameType()
            {
                return btype == bvalue.GetType();
            }
        }
    }
    public enum CSOptions
    {
        None = 0,
        WithTypes = 1,
        NoTypes = 2,
        SafeString = 4,
    }

}
