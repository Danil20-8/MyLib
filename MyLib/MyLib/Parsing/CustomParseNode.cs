using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public class CustomParseNode<Tsource, Tvalue, Tcontroller> : ParseNode<Tsource, Tvalue, Tcontroller> where Tcontroller : IParseController
    {
        Action<Tvalue, Tcontroller> enterHandle;
        Action<Tvalue, Tcontroller> exitHandle;

        public CustomParseNode(Action<Tvalue, Tcontroller> enterHandle, Action<Tvalue, Tcontroller> exitHandle, IParseValueHandler<Tsource, Tvalue> valueHandler, ParseNodeFlags flags = ParseNodeFlags.None, Transition[] transits = null)
            : base(valueHandler, flags, transits)
        {
            this.enterHandle = enterHandle;
            this.exitHandle = exitHandle;
        }

        protected override void EnterHandle(Tvalue value, Tcontroller controller)
        {
            if(enterHandle != null)
                enterHandle(value, controller);
        }
        protected override void ExitHandle(Tvalue value, Tcontroller controller)
        {
            if(exitHandle != null)
                exitHandle(value, controller);
        }
    }
}
