using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using MyLib.Algoriphms;
using MyLib.Modern;

namespace MyLib.Serialization
{
    struct SerializedType
    {
        public bool IsSerializable { get { return kind != Kind.MySerializable; } }
        public bool IsMySerializable { get { return kind == Kind.MySerializable; } }
        public bool IsString { get { return kind == Kind.String; } }
        public bool IsComVisible { get { return kind == Kind.ComVisible || kind == Kind.String; } }
        public bool IsArray { get { return kind == Kind.Array; } }
        public bool IsIEnumerable { get { return kind == Kind.IEnumerable; } }


        public readonly Type type;
        Kind kind;

        UField[] addons;
        UField[] args;
        public SerializedType(Type type)
        {
            this.type = type;

            addons = null;
            args = null;

            if (type.IsSerializable)
            {
                if(type == typeof(string))
                {
                    kind = Kind.String;
                }
                else if (type.IsArray)
                {
                    kind = Kind.Array;
                }
                else if(type.GetInterface("IEnumerable") != null && type.GetInterface("ICollection") != null && type.IsGenericType)
                {
                    kind = Kind.IEnumerable;
                }
                else if (type.GetCustomAttribute<ComVisibleAttribute>() != null)
                {
                    kind = Kind.ComVisible;
                }
                else
                {
                    kind = Kind.Serializable;
                    addons = UField.GetUFields(type, BindingFlags.Instance | BindingFlags.Public, true).Where(p => p.GetCustomAttribute<NonSerializedAttribute>() == null).ToArray();
                }
            }
            else
            {
                kind = Kind.MySerializable;

                var fields = UField.GetUFields(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                addons = fields.Where(p => p.GetCustomAttribute<AddonAttribute>() != null).ToArray();
                args = fields.Select(p => new Tuple<UField, ConstructorArgAttribute>(p, p.GetCustomAttribute<ConstructorArgAttribute>())).Where(t => t.Item2 != null).OrderBy(t => t.Item2.position).Select(t => t.Item1).ToArray();
            }
        }

        public object GetValue(object obj)
        {
            return obj;
        }
        public IEnumerable<object> GetElements(object obj)
        {
            return ((IEnumerable)obj).Cast<object>();
        }
        public IEnumerable<object> GetFields(object obj)
        {
            if (IsSerializable)
                return addons.Select(p => p.GetValue(obj));
            else
                return args.Select(p => p.GetValue(obj)).Concat(addons.Select(p => p.GetValue(obj)));
        }
        public IEnumerable<UField> GetTypeFields()
        {
            return addons;
        }
        public IEnumerable<UField> GetTypeConstructorArgs()
        {
            return args;
        }
        public IEnumerable<UField> GetTypeAll()
        {
            return args.Concat(addons);
        }

        public object CreateInstance(object[] args, object[] fields)
        {
            object obj = CreateInstance(args);
            SetFields(obj, fields);
            return obj;
        }

        public object CreateInstance(object[] args)
        {
            if (this.args == null)
                return Activator.CreateInstance(type);
            else
            {
                if (this.args.Length == 0)
                    return Activator.CreateInstance(type);
                else 
                    return type.GetConstructor(this.args.Select(a => a.fieldType).ToArray()).Invoke(args);
                
            }
        }

        public void SetFields(object obj, object[] fields)
        {
            for(int i = 0; i < addons.Length; i++)
                addons[i].SetValue(obj, fields[i]);
        }

        enum Kind
        {
            ComVisible,
            IEnumerable,
            Array,
            String,
            Serializable,
            MySerializable
        }
    }

    struct SerializeTemp
    {
        SerializedType type;

        object value;
        List<object> elements;
        List<object> args;
        public SerializeTemp(SerializedType type)
        {
            this.type = type;

            value = null;
            elements = null;
            args = null;

            if(!type.IsComVisible)
            {
                elements = new List<object>();
                if (type.IsMySerializable)
                    args = new List<object>();
            }
        }

        public void SetValue(object value)
        {
            this.value = value;
        }

        public void AddElement(object element)
        {
            elements.Add(element);
        }
        public void AddArg(object arg)
        {
            args.Add(arg);
        }

        public object GetResult()
        {
            if(type.IsSerializable)
            {
                if (type.IsString)
                {
                    return value;
                }
                else if (type.IsArray)
                {
                    var arr = Array.CreateInstance(type.type.GetElementType(), elements.Count);
                    for (int i = 0; i < arr.Length; i++)
                        arr.SetValue(elements[i], i);
                    return arr;
                }
                else if (type.IsIEnumerable)
                {
                    var result = type.CreateInstance(null);
                    MethodInfo add = type.type.GetMethod("Add", type.type.GetGenericArguments());

                    object[] arg = new object[1];
                    foreach (var e in elements)
                    {
                        arg[0] = e;
                        add.Invoke(result, arg);
                    }
                    return result;
                }
                else if(type.IsComVisible)
                {
                    return value;
                }
                else
                {
                    return type.CreateInstance(null, elements.ToArray());
                }
            }
            else
            {
                return type.CreateInstance(args.ToArray(), elements.ToArray());
            }
        }
    }

    public class UField
    {
        MemberInfo field;
        public Type fieldType
        {
            get
            {
                if (field is PropertyInfo)
                    return ((PropertyInfo)field).PropertyType;
                else
                    return ((FieldInfo)field).FieldType;
            }
        }
        public string Name { get { return field.Name; } }
        public UField(PropertyInfo info)
        {
            field = info;
        }
        public UField(FieldInfo info)
        {
            field = info;
        }

        public object GetValue(object master)
        {
            if (field is PropertyInfo)
                return ((PropertyInfo)field).GetValue(master, null);
            else
                return ((FieldInfo)field).GetValue(master);
        }
        public void SetValue(object master, object value)
        {
            if (field is PropertyInfo)
                ((PropertyInfo)field).SetValue(master, value, null);
            else
                ((FieldInfo)field).SetValue(master, value);
        }
        public static UField[] GetUFields(Type self, BindingFlags flags, bool readWriteOnly = false)
        {
            IEnumerable<UField> ps;
            if (readWriteOnly)
                ps = self.GetProperties(flags).Where(p => p.CanRead && p.CanWrite).Select(p => new UField(p));
            else
                ps = self.GetProperties(flags).Select(p => new UField(p));
            var fs = self.GetFields(flags).Select(f => new UField(f));
            return ps.Concat(fs).ToArray();
        }
        public static UField GetField(Type self, string name, BindingFlags flags)
        {
            var p = self.GetProperty(name, flags);
            if (p != null)
                return new UField(p);
            var f = self.GetField(name, flags);
            if (f != null)
                return new UField(f);
            return null;
        }
        public static UField GetField(Type self, string name)
        {
            var p = self.GetProperty(name);
            if (p != null)
                return new UField(p);
            var f = self.GetField(name);
            if (f != null)
                return new UField(f);
            return null;
        }
        public IEnumerable<Attribute> GetCustomAttributes()
        {
            return field.GetCustomAttributes();
        }
        public T GetCustomAttribute<T>() where T : Attribute
        {
            return field.GetCustomAttribute<T>();
        }
    }

    public class SerializeBinders : ICollection<TypePair>
    {
        List<TypePair> binders = new List<TypePair>();

        public Type FindBySource(Type source)
        {
            foreach (var b in binders)
                if (b.source == source)
                    return b.binder;
            return null;
        }
        public Type FindByBinder(Type binder)
        {
            foreach (var b in binders)
                if (b.binder == binder)
                    return b.source;
            return null;
        }
        public void AddRange(IEnumerable<TypePair> binders)
        {
            this.binders = this.binders.Concat(binders).Distinct().ToList();
        }
        public int Count
        {
            get
            {
                return ((ICollection<TypePair>)binders).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((ICollection<TypePair>)binders).IsReadOnly;
            }
        }

        public void Add(TypePair item)
        {
            foreach (var b in binders)
                if (b == item)
                    return;

            ((ICollection<TypePair>)binders).Add(item);
        }

        public void Clear()
        {
            ((ICollection<TypePair>)binders).Clear();
        }

        public bool Contains(TypePair item)
        {
            return ((ICollection<TypePair>)binders).Contains(item);
        }

        public void CopyTo(TypePair[] array, int arrayIndex)
        {
            ((ICollection<TypePair>)binders).CopyTo(array, arrayIndex);
        }

        public IEnumerator<TypePair> GetEnumerator()
        {
            return ((ICollection<TypePair>)binders).GetEnumerator();
        }

        public bool Remove(TypePair item)
        {
            return ((ICollection<TypePair>)binders).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<TypePair>)binders).GetEnumerator();
        }
    }
    public class TypePair
    {
        public Type source;
        public Type binder;
        public TypePair(Type source, Type binder)
        {
            this.source = source;
            this.binder = binder;
        }
    }
    public interface IBinder
    {
        object GetResult();
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ConstructorArgAttribute : Attribute
    {
        public readonly int position;
        public ConstructorArgAttribute(int position)
        {
            this.position = position;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AddonAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method)]
    public class PostDeserializeAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method)]
    public class PreSerializeAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializeBinderAttribute : Attribute
    {
        public readonly Type binder;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder">Binder type extends IBinder</param>
        public SerializeBinderAttribute(Type binder)
        {
            this.binder = binder;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NoSerializeBinder : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class VersionAttribute : Attribute
    {
        public readonly int version;
        public VersionAttribute(int version)
        {
            this.version = version;
        }
    }
}
