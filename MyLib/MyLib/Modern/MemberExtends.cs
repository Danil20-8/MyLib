using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DRLib.Modern
{
    public static class MemberExtends
    {
        public static T GetCustomAttribute<T>(this MemberInfo member) where T : Attribute
        {
            Type type = typeof(T);
            foreach (var a in member.GetCustomAttributes(true))
                if (a.GetType() == type)
                    return (T)a;
            return null;
        }
        public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo member)
        {
            return member.GetCustomAttributes(true).Select(a => (Attribute)a);
        }
    }
    /*public static class PropertyExtends
    {
        /*public static object GetValue(this PropertyInfo self, object obj)
        {
            return self.GetValue(obj, null);
        }
        public static void SetValue(this PropertyInfo self, object obj, object value)
        {
            self.SetValue(obj, value, null);
        }
    }*/
}
