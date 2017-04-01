using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Structures
{
    [Serializable]
    public struct LineBounds<T> where T : struct, IComparable
    {
        public T left;
        public T right;

        public LineBounds(T left, T right)
        {
            this.left = left;
            this.right = right;
        }

        public bool InBounds(T value)
        {
            return InBounds(value, left, right);
        }
        public bool ToBounds(ref T value)
        {
            return ToBounds(ref value, left, right);
        }
        public T ToBounds(T value)
        {
            return ToBounds(value, left, right);
        }

        public static bool InBounds(T value, T left, T right)
        {
            return value.CompareTo(left) >= 0 && value.CompareTo(right) <= 0;
        }
        public static bool ToBounds(ref T value, T left, T right)
        {
            if (value.CompareTo(left) < 0) { value = left; return true; }
            else if (value.CompareTo(right) > 0) { value = right; return true; }

            return false;
        }
        public static T ToBounds(T value, T left, T right)
        {
            if (value.CompareTo(left) < 0) return left;
            else if (value.CompareTo(right) > 0) return right;
            else return value;
        }

    }
}
