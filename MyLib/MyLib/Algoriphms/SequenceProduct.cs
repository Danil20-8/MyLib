using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Algoriphms
{
    public static class SequenceProduct
    {
        public static TProduct Product<TSource, TProduct>(this IEnumerable<TSource> source, Func<TSource, TProduct, TProduct> productFunc)
            where TProduct : new()
        {
            return source.Product(new TProduct(), productFunc);
        }
        public static TProduct Product<TSource, TProduct>(this IEnumerable<TSource> source, TProduct originProduct, Func<TSource, TProduct, TProduct> productFunc)
        {
            foreach (var s in source)
                originProduct = productFunc(s, originProduct);
            return originProduct;
        }
        public static TProduct Product<TSource, TProduct>(this IEnumerable<TSource> source, Func<TSource, TProduct> productSelector, Func<TSource, TProduct, TProduct> productFunc)
        {
            using (var i = source.GetEnumerator())
            {
                if (!i.MoveNext())
                    return default(TProduct);

                TProduct result = productSelector(i.Current);

                while (i.MoveNext())
                    result = productFunc(i.Current, result);

                return result;
            }
        }
    }
}
