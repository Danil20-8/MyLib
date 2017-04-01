using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Structures
{
    public struct SquareBounds<T> where T : struct, IComparable
    {
        public LineBounds<T> xBounds;
        public LineBounds<T> yBoudns;

        public SquareBounds(T leftX, T rightX, T leftY, T rightY)
        {
            xBounds = new LineBounds<T>(leftX, rightX);
            yBoudns = new LineBounds<T>(leftY, rightY);
        }

        public bool InBoubds(T x, T y)
        {
            return xBounds.InBounds(x) && yBoudns.InBounds(y);
        }

        public bool ToBounds(ref T x, ref T y)
        {
            return xBounds.ToBounds(ref x) || yBoudns.ToBounds(ref y);
        }
    }
}
