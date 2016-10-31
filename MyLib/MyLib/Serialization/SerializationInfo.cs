using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyLib.Serialization.Binders;

namespace MyLib.Serialization
{
    internal class SerializationTypes
    {
        [Addon]
        List<string> types = new List<string>();

        public int AddType(string type)
        {
            for (int i = 0; i < types.Count; i++)
                if (type == types[i])
                    return i;
            types.Add(type);
            return types.Count - 1;
        }
        public string GetType(int index)
        {
            return types[index];
        }
    }
}
