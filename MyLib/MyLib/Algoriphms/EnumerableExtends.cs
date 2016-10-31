using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;


namespace MyLib.Algoriphms
{
    public static class EnumerableExtends
    {
        public static bool Has<T>(this IEnumerable<T> self, T item)
        {
            foreach (var i in self)
                if (i.Equals(item)) return true;
            return false;
        }
        public static string ToString<T>(this IEnumerable<T> enumerable, string splitter = " ")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in enumerable)
                sb.Append(splitter + e.ToString());
            if (sb.Length > 0)
                sb.Remove(0, splitter.Length);
            return sb.ToString();
        }
        public static string ToString(this IEnumerable self, string splitter = " ")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in self)
                sb.Append(splitter + e.ToString());
            if (sb.Length > 0)
                sb.Remove(0, splitter.Length);
            return sb.ToString();
        }
        public static T[] Parse<T>(this string source, char splitter = ' ')
        {
            var parseMethod = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });
            return source.Split(splitter).Select(s => (T)parseMethod.Invoke(null, new object[] { s })).ToArray();
        }
        public static IEnumerable<T> Insert<T>(this IEnumerable<T> self, T value, int pos = 0)
        {
            int i = 0;
            List<T> l = new List<T>();
            foreach (var e in self)
            {
                if (i == pos)
                    l.Add(value);
                l.Add(e);
                i++;
            }
            return l;
        }
        public static int IndexOf<T>(this IEnumerable<T> self, T item)
        {
            int i = 0;
            using (var e = self.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (e.Current.Equals(item))
                        return i;
                    i++;
                }
            }
            throw new Exception("Item's not found");
        }
    }
}
