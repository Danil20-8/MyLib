using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Algoriphms
{
    public static class ListExtends
    {
        public static void SetSize<TValue>(this List<TValue> list, int size)
        {
            int count = list.Count;
            if(count < size)
            {
                int dif = size - count;
                for (int i = 0; i < dif; ++i)
                    list.Add(default(TValue));
            }
            else if(count > size)
            {
                int dif = count - size;
                list.RemoveRange(count - dif, dif);
            }
        }
    }
}
