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
    public class CompactSerializer
    {
        SerializeBinders binders;
        SerializationTypes serializationTypes;
        bool includeTypes;
        IEnumerable<Assembly> assemblies = new Assembly[0];
        CSOptions options;
        char splitter = '&';
        public CompactSerializer(CSOptions options, SerializeBinders binders)
        {
            this.options = options;
            this.binders = binders;
        }

        public CompactSerializer(CSOptions options)
        {
            this.options = options;
            this.binders = new SerializeBinders();
        }
        public CompactSerializer(SerializeBinders binders)
        {
            this.options = CSOptions.None;
            this.binders = binders;
        }
        public CompactSerializer(char splitter = '&', CSOptions options = CSOptions.None, SerializeBinders binders = null)
        {
            this.splitter = splitter;
            this.options = options;
            this.binders = binders == null ? new SerializeBinders() : binders;
        }
        public string Serialize(object data, Type binder = null)
        {
            serializationTypes = new SerializationTypes();
            if ((options & CSOptions.WithTypes) != 0)
                includeTypes = true;
            else {
                includeTypes = !IsSameTypes(data, binder);
                if (includeTypes && (options & CSOptions.NoTypes) != 0)
                    throw new Exception(string.Format("{0} contains elements by them base", data));
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
                .Where(m => m.GetCustomAttribute<PostDeserializeAttribute>() != null);
            foreach (var m in pre)
                m.Invoke(data, new object[] { });

            Type type = data.GetType();
            if (binder == null)
            {
                binder = type;
                type = Bind(ref data);
            }
            else {
                Bind(ref data, binder);
                Other.Swap(ref type, ref binder);
            }

            StringBuilder sb = new StringBuilder();

            if (includeTypes)
            {
                if (includeType)
                    sb.Append(serializationTypes.AddType(binder != null ? binder.FullName : type.FullName));
                sb.Append(':');
            }
            if (type.IsSerializable)
            {
                if (IsIEnumerable(type))
                {
                    var numerable = ((IEnumerable)data).Cast<object>();
                    bool withType = false;
                    if (includeTypes)
                        if(!IsSameElements((IEnumerable)data))
                            withType = true;
                    sb.Append(numerable.Count());
                                        Split(sb);
                    if (numerable.Count() > 0)
                        sb.Append(numerable.Select(e => Serialize(e, withType, null)).ToString(""));

                }
                else if(type == typeof(string) && ((options & CSOptions.SafeString) != 0))
                {
                    sb.Append(((string)data).Length);
                    Split(sb);
                    sb.Append(data.ToString());
                    Split(sb);
                }
                else if (IsComVisible(type))
                {
                    sb.Append(data.ToString());
                    Split(sb);
                }
                else
                {
                    if (!typeof(ValueType).IsInstanceOfType(data))
                        if (type.GetConstructor(new Type[] { }) == null)
                            throw new Exception(string.Format("{0} has no default constructor", type));
                    Write(sb, data, GetSerializedFields(type));
                }
            }
            else
                Write(sb, data, GetConstructorArgs(type).Concat(GetAddons(type)));
            return sb.ToString();
        }
        void Write(StringBuilder sb, object data, IEnumerable<UField> ps)
        {
            foreach (var field in ps)
            {
                object value = field.GetValue(data);
                sb.Append(Serialize(value, field.fieldType != value.GetType(), Bind(field)));
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
            int l = 0;
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
            SerializeTemp result = null;

            if (includeTypes)
            {
                string sub;
                Extract(reader, out sub);
                if (sub != "")
                    ftype = GetType(serializationTypes.GetType(int.Parse(sub)));
            }
            if(binder == null)
                binder = Bind(ftype);

            result = new SerializeTemp(binder);

            if (binder.IsSerializable)
            {
                if (IsIEnumerable(binder) || binder.IsArray)
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
                        result.AddField("", Deserialize(elementType, null, reader), SerializeTemp.FieldFlags.Element);
                }
                else if(binder == typeof(string) && ((options & CSOptions.SafeString) != 0))
                {
                    string line;
                    Extract(reader, out line);
                    int length = int.Parse(line);
                    Extract(reader, out line, length);
                    result.AddField("", line, SerializeTemp.FieldFlags.Element);
                }
                else if (IsComVisible(binder))
                {
                    string line;
                    Extract(reader, out line);
                    result.AddField("", GetValue(binder, line), SerializeTemp.FieldFlags.Element);
                }
                else
                {
                    foreach (var sa in GetSerializedFields(binder))
                        result.AddField(sa.Name, Deserialize(sa.fieldType, Bind(sa), reader), SerializeTemp.FieldFlags.Addon);
                }
            }
            else
            {
                foreach (var ca in GetConstructorArgs(binder))
                    result.AddField(ca.Name, Deserialize(ca.fieldType, Bind(ca), reader), SerializeTemp.FieldFlags.Arg);
                foreach (var ad in GetAddons(binder))
                    result.AddField(ad.Name, Deserialize(ad.fieldType, Bind(ad), reader), SerializeTemp.FieldFlags.Addon);
            }

            return binder == ftype ? result.GetValue() : ((IBinder)result.GetValue()).GetResult();
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
            if (IsIEnumerable(type))
            {
                if (type.IsArray)
                    UpdateBinders(type.GetElementType());
                else
                    UpdateBinders(type.GetGenericArguments().First());
            }
            return assemblies.Distinct();
        }
        Type GetEnumerableElementType(Type enumerable)
        {
            if (enumerable.IsArray)
               return enumerable.GetElementType();
            else
                return enumerable.GetGenericArguments().First();
        }
        Type Bind(Type type)
        {
            Type binder = BindWithAttribute(type);
            if (binder == null)
            {
                binder = binders.FindBySource(type);
                if (binder == null)
                    binder = type;
            }
            return binder;
        }
        Type BindWithAttribute(Type type)
        {
            var attr = type.GetCustomAttribute<SerializeBinderAttribute>();
            return attr != null ? attr.binder : null;
        }
        Type Bind(ref object obj)
        {
            Type type = obj.GetType();
            Type binder = Bind(type);
            if (type != binder)
                obj = binder.GetConstructor(new Type[] { type }).Invoke(new object[] { obj });
            return binder;
        }
        void Bind(ref object obj, Type binder)
        {
            obj = binder.GetConstructor(new Type[] { obj.GetType() }).Invoke(new object[] { obj });
        }
        Type Bind(UField field)
        {
            var attr = field.GetCustomAttribute<SerializeBinderAttribute>();
            return attr != null ? attr.binder : null;
        }
        void UpdateBinders(Type type) // Remove this method later
        {
            // So much pain and abjection! For what?!
            AddBinderFromSource(type);
            type = Bind(type);

            if (IsIEnumerable(type))
            {
                if (type.IsArray)
                    UpdateBinders(type.GetElementType());
                else
                    UpdateBinders(type.GetGenericArguments().First());
            }

            foreach (var tf in GetFields(type).Select(f => new CustomTree<UField, Type>(f, s => Other.NonNull(Bind(s), Bind(s.fieldType)), t => GetFields(t).Where(a => a.GetCustomAttribute<NoSerializeBinder>() == null))).SelectMany(n => n.ByElements()).Select(n => n.node))
            {
                AddBinderFromSource(tf);
                if(IsIEnumerable(tf))
                {
                    if (tf.IsArray)
                        UpdateBinders(tf.GetElementType());
                    else
                        UpdateBinders(tf.GetGenericArguments().First());
                }
            }    
        }
        void AddBinderFromSource(Type source)
        {
            var attr = source.GetCustomAttribute<SerializeBinderAttribute>();
            if (attr != null)
                binders.Add(new TypePair(source, attr.binder));
        }
        static string SubString(string source, ref int left, int right)
        {
            int l = left;
            left = right + 1;
            return source.Substring(l, right - l);
        }

        static object GetValue(Type type, string value)
        {
            object result = null;
            if (type.Equals(typeof(string)))
                result = value;
            else if (value != null)
            {
                var parse = type.GetMethod("Parse", new Type[] { typeof(string) });
                if (parse != null)
                    result = parse.Invoke(null, new object[] { value });
            }
            return result;
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
            return UField.GetUFields(type, BindingFlags.Instance | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<NonSerializedAttribute>() == null);
        }
        bool IsSameElements(IEnumerable numerable)
        {
            Type t = numerable.GetType();
            Type elementType = null;
            if (t.IsArray)
                elementType = t.GetElementType();
            else
                elementType = t.GetGenericArguments().First();
            foreach (var e in numerable)
                if (e.GetType() != elementType || !IsSameTypes(e, null))
                    return false;
            return true;
        }
        bool IsSameTypes(object obj, Type binder)
        {
            if (binder != null)
                Bind(ref obj, binder);
            else
                Bind(ref obj);
            var objs = TreeNode<object>.BuildCustomTree(obj, t => t, t =>
            {
                if (IsComVisible(t.GetType()))
                    return new object[0];
                binder = Bind(ref t);
                return GetFields(binder).Select(f => f.GetValue(t));
            })
            .ByElements().Select(n => n.item);

            foreach (var o in objs)
            {
                object tempo = o;
                Type t = Bind(ref tempo);
                if (IsIEnumerable(t))
                {
                    if (!IsSameElements((IEnumerable)tempo))
                        return false;
                }
                else
                foreach (var f in GetFields(t))
                    if (f.fieldType != f.GetValue(tempo).GetType())
                        return false;
            }
            return true;
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
        bool TryExtract(ExtractResult expected, string source, int left)
        {
            string sub;
            return Extract(source, ref left, out sub) == expected;
        }

        ExtractResult Extract(string source, ref int left, out string result)
        {
            for (int i = left; i < source.Length; i++)
            {
                if (source[i] == splitter)
                {
                    result = SubString(source, ref left, i);
                    return ExtractResult.Value;
                }
                else if (source[i] == ':')
                {
                    result = SubString(source, ref left, i);
                    return ExtractResult.Type;
                }
            }

            result = SubString(source, ref left, source.Length);
            return ExtractResult.Value;
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
    }
    public enum CSOptions
    {
        None = 0,
        WithTypes = 1,
        NoTypes = 2,
        SafeString = 4,
    }
}
