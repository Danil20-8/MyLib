using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Structures
{
    public struct CubeBounds<T> where T : struct, IComparable
    {
        public LineBounds<T> xBounds;
        public LineBounds<T> yBoudns;
        public LineBounds<T> zBounds;

        public CubeBounds(T leftX, T rightX, T leftY, T rightY, T leftZ, T rightZ)
        {
            xBounds = new LineBounds<T>(leftX, rightX);
            yBoudns = new LineBounds<T>(leftY, rightY);
            zBounds = new LineBounds<T>(leftZ, rightZ);
        }

        public bool InBoubds(T x, T y, T z)
        {
            return xBounds.InBounds(x) && yBoudns.InBounds(y) && zBounds.InBounds(z);
        }

        public bool ToBounds(ref T x, ref T y, ref T z)
        {
            return xBounds.ToBounds(ref x) || yBoudns.ToBounds(ref y) || zBounds.ToBounds(ref z);
        }
    }
}
