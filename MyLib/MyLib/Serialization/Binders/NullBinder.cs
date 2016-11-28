using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Serialization.Binders
{
    public struct NullBinder<T> : IBinder where T: class
    {
        [Addon]
        int hasValue { get { return rvalue != null ? 1 : 0; } set { _hasValue = value == 1 ? true : false; } }
        bool _hasValue;

        [Addon]
        string svalue { get { if (rvalue != null) return new CompactSerializer('~').Serialize(rvalue); else return ""; } set { if (_hasValue) rvalue = new CompactSerializer('~').Deserialize<T>(value); else rvalue = null; } }

        T rvalue;

        public NullBinder(T value)
        {
            rvalue = value;
            _hasValue = false;
        }

        public object GetResult()
        {
            return rvalue;
        }
    }
}
