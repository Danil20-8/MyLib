using MyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Serialization
{
    public partial class CompactSerializer
    {
        struct FieldObjectTree : ITreeable<FieldObjectTree>
        {
            public FieldObjectTree root
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerable<FieldObjectTree> childs
            {
                get
                {
                    object value = fieldObject.bvalue;
                    return GetFields(fieldObject.btype).Select(f => new FieldObjectTree(new FieldObject(f, f.GetValue(value))));
                }
            }

            public void AddChild(FieldObjectTree child)
            {
                throw new NotImplementedException();
            }

            public void RemoveChild(FieldObjectTree child)
            {
                throw new NotImplementedException();
            }

            public FieldObjectTree(FieldObject fieldObject)
            {
                this.fieldObject = fieldObject;
            }

            public readonly FieldObject fieldObject;

            public static IEnumerable<FieldObjectTree> CreateTrees(object obj)
            {
                return GetFields(obj.GetType()).Select(f => new FieldObjectTree(new FieldObject(f, f.GetValue(obj))));
            }
        }
    }
}
