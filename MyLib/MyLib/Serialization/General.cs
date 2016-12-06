using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;

using MyLib.Algoriphms;
using MyLib.Modern;

namespace MyLib.Serialization
{
    class SerializeTemp
    {
        List<object> dfields = new List<object>();
        Dictionary<string, object> addons = new Dictionary<string, object>();
        List<object> elements = new List<object>();
        Type type;
        public SerializeTemp(Type type)
        {
            this.type = type;
        }
        public void AddField(string name, object field, FieldFlags flags)
        {
            switch (flags)
            {
                case FieldFlags.Element:
                    elements.Add(field);
                    break;
                case FieldFlags.Arg:
                    dfields.Add(field);
                    break;
                case FieldFlags.Addon:
                    addons.Add(name, field);
                    break;
            }
        }
        public object GetValue()
        {
            object result = null;
            var ctor = type.GetConstructor(dfields.Select(f => f.GetType()).ToArray());
            if (ctor != null)
                result = ctor.Invoke(dfields.ToArray());
            else if (type != typeof(string) && !type.IsArray)
                result = Activator.CreateInstance(type);
            foreach (var a in addons)
            {
                var f = UField.GetField(type, a.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                f.SetValue(result, a.Value);
            }

            if (type.IsArray)
            {
                Array arr = Array.CreateInstance(type.GetElementType(), elements.Count);
                for (int i = 0; i < elements.Count; i++)
                    arr.SetValue(elements[i], i);
                result = arr;
            }
            else if (type.GetInterfaces().Has(typeof(IEnumerable)) && type.GetInterfaces().Has(typeof(ICollection)))
            {
                MethodInfo add = null;
                if (type.IsGenericType)
                    add = type.GetMethod("Add", type.GetGenericArguments());
                else
                    add = type.GetMethod("Add", new Type[] { typeof(object) });
                if (add != null)
                    foreach (var e in elements)
                        add.Invoke(result, new object[] { e });
            }
            else if (elements.Count == 1)
            {
                result = elements[0];
            }
            return result;
        }

        public enum FieldFlags
        {
            Element,
            Arg,
            Addon
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
        public static UField[] GetUFields(Type self, BindingFlags flags)
        {
            var ps = self.GetProperties(flags).Select(p => new UField(p));
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
