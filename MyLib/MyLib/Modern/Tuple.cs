﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Modern
{
    public struct Tuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public Tuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Tuple<T1, T2>))
                return false;

            var rt = (Tuple<T1, T2>)obj;

            return Item1.Equals(rt.Item1) && Item2.Equals(Item2);
        }
    }
}