using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Structures.Arrays
{
    public static class ProxyArray
    {
        public static ProxyArray<TElement> Create<TElement>(TElement[] array, int start)
        {
            return new ProxyArray<TElement>(array, start);
        }
        public static ProxyArray<TElement> Create<TElement>(TElement[] array, int start, int length)
        {
            return new ProxyArray<TElement>(array, start, length);
        }
    }

    public struct ProxyArray<TElement> : IEnumerable<TElement>
    {
        TElement[] array;
        int start;
        int length;

        public int Length { get { return length; } }

        TElement this [int index]
        {
            get { if(start + index < length) return array[start + index];
                else throw new IndexOutOfRangeException();
            }
            set { if (start + index < length) array[start + index] = value;
                else throw new IndexOutOfRangeException();
            }
        }

        public ProxyArray(TElement[] array, int start)
            :this(array, start, array.Length - start)
        {
        }

        public ProxyArray(TElement[] array, int start, int length)
        {
            this.array = array;
            this.start = start;
            this.length = length;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            for (int i = start; i < length; ++i)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
