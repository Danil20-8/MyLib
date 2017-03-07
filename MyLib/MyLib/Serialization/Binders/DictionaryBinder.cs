using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Serialization.Binders
{
    public struct DictionaryBinder<Tkey, Tvalue> : IBinder
    {
        Dictionary<Tkey, Tvalue> __dict;
        [Addon]
        public Pair<Tkey, Tvalue>[] dict
        {
            get { return __dict.Select(p => new Pair<Tkey, Tvalue>() { key = p.Key, value = p.Value }).ToArray(); }
            set { __dict = value.ToDictionary(p => p.key, p => p.value); }
        }

        public DictionaryBinder(Dictionary<Tkey, Tvalue> dict)
        {
            __dict = dict;
        }

        public object GetResult()
        {
            return __dict;
        }
    }
    public struct DictionaryArrayBinder<Tkey, Tvalue> : IBinder
    {
        Dictionary<Tkey, Tvalue>[] __dict;
        [Addon]
        public Pair<Tkey, Tvalue>[][] dict
        {
            get { return __dict.Select(d => d.Select(p => new Pair<Tkey, Tvalue>() { key = p.Key, value = p.Value }).ToArray()).ToArray(); }
            set { __dict = value.Select(d => d.ToDictionary(p => p.key, p => p.value)).ToArray(); }
        }

        public DictionaryArrayBinder(Dictionary<Tkey, Tvalue>[] dict)
        {
            __dict = dict;
        }

        public object GetResult()
        {
            return __dict;
        }
    }
    [Serializable]
    public struct Pair<T1, T2>
    {
        public T1 key;
        public T2 value;
    }
}
