using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Algoriphms
{
    public static class Other
    {
        public static T NotNull<T>(T a1, T a2, T bothNull)
        {
            if (a1 != null)
                return a1;
            else if (a2 != null)
                return a2;
            else
                return bothNull;
        }
        public static T NonNull<T>(T a1, T a2)
        {
            if (a1 != null)
                return a1;
            else if (a2 != null)
                return a2;
            else
                throw new Exception("both values are null");
        }
        public static void Swap<T>(ref T v1, ref T v2)
        {
            T temp = v1;
            v1 = v2;
            v2 = temp;
        }
    }
}
