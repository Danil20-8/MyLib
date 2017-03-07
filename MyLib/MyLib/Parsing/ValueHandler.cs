using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Parsing
{
    public interface IParseValueHandler<TSource, TValue>
    {
        Value<TValue> GetValue(List<TSource> source);
    }

    public struct Value<TValue>
    {
        public bool hasValue;
        public TValue value;
    }

    public class DefaultValueHandler<TSource> : IParseValueHandler<TSource, List<TSource>>
    {

        TSource[] trimKeys;

        public DefaultValueHandler(TSource[] trimKeys)
        {
            this.trimKeys = trimKeys;
        }

        public Value<List<TSource>> GetValue(List<TSource> source)
        {
            for (int i = source.Count - 1; i >= 0; --i)
                if (!HasToBeTrim(source[i]))
                    break;
                else
                    source.RemoveAt(i);

            while (source.Count > 0 && HasToBeTrim(source[0]))
                source.RemoveAt(0);

            return new Value<List<TSource>> { hasValue = source.Count > 0, value = source };
        }

        bool HasToBeTrim(TSource value)
        {
            foreach (var tk in trimKeys)
                if (value.Equals(tk))
                    return true;
            return false;
        }
    }
}
