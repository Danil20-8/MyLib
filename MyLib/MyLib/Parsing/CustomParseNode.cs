using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public class CustomParseNode<Tsource> : ParseNode<Tsource>
    {
        Action<List<Tsource>> enterHandle;
        Action<List<Tsource>> exitHandle;

        public CustomParseNode(Action<List<Tsource>> enterHandle, Action<List<Tsource>> exitHandle, Transition[] transits, ParseNodeFlags flags = ParseNodeFlags.None)
            : base(transits, flags)
        {
            this.enterHandle = enterHandle;
            this.exitHandle = exitHandle;
        }

        protected override void EnterHandle(List<Tsource> values)
        {
            if(enterHandle != null)
                enterHandle(values);
        }
        protected override void ExitHandle(List<Tsource> values)
        {
            if(exitHandle != null)
                exitHandle(values);
        }
    }
}
