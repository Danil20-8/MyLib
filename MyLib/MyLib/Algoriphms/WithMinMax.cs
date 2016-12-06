using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Algoriphms
{
    public static class WithMinMax
    {
        public static T WithMin<T, Tcmp>(this IEnumerable<T> numerable, Func<T, Tcmp> selector) where Tcmp : IComparable
        {
            var i = numerable.GetEnumerator();

            i.MoveNext();
            T min = i.Current;
            Tcmp value = selector(min);
            Tcmp curr;

            T current;

            while (i.MoveNext())
            {
                current = i.Current;
                curr = selector(current);
                if (curr.CompareTo(value) < 0)
                {
                    min = current;
                    value = curr;
                }
            }
            i.Dispose();
            return min;

        }
        public static bool WithMin<T, Tcmp>(this IEnumerable<T> numerable, Func<T, Tcmp> selector, Predicate<T> predicate, out T result) where Tcmp : IComparable
        {
            var i = numerable.GetEnumerator();
            Tcmp curr;
            while (i.MoveNext())
            {
                if (predicate(i.Current))
                    goto begin;
            }
            result = default(T);
            return false;
            begin:

            T min = i.Current;
            Tcmp value = selector(min);
            T current;
            while (i.MoveNext())
            {
                current = i.Current;
                if (predicate(current))
                {
                    curr = selector(current);
                    if (curr.CompareTo(value) < 0)
                    {
                        min = current;
                        value = curr;
                    }
                }
            }
            i.Dispose();
            result = min;
            return true;
        }
        public static T WithMax<T, Tcmp>(this IEnumerable<T> numerable, Func<T, Tcmp> selector) where Tcmp : IComparable
        {
            var i = numerable.GetEnumerator();

            i.MoveNext();
            T max = i.Current;
            Tcmp value = selector(max);
            Tcmp curr;

            T current;

            while (i.MoveNext())
            {
                current = i.Current;
                curr = selector(current);
                if (curr.CompareTo(value) > 0)
                {
                    max = current;
                    value = curr;
                }
            }
            i.Dispose();
            return max;
        }
        public static bool WithMax<T, Tcmp>(this IEnumerable<T> numerable, Func<T, Tcmp> selector, Predicate<T> predicate, out T result) where Tcmp : IComparable
        {
            var i = numerable.GetEnumerator();
            Tcmp curr;
            while (i.MoveNext())
            {
                if (predicate(i.Current))
                    goto begin;
            }
            result = default(T);
            return false;
            begin:

            T max = i.Current;
            Tcmp value = selector(max);
            T current;
            while (i.MoveNext())
            {
                current = i.Current;
                if (predicate(current))
                {
                    curr = selector(current);
                    if (curr.CompareTo(value) > 0)
                    {
                        max = current;
                        value = curr;
                    }
                }
            }
            i.Dispose();
            result = max;
            return true;
        }
    }
}
