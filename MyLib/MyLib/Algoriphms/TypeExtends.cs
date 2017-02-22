using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Algoriphms
{
    public static class TypeExtends
    {
        static readonly Type objectType = typeof(object);

        public static int GetGeneration(this Type type, Type parentType)
        {
            int i = 0;
            while(type != objectType)
            {
                if(type == parentType)
                    return i;

                ++i;
                type = type.BaseType;
            }
            if (parentType == objectType)
                return i;
            return -1;
        }
    }
}
