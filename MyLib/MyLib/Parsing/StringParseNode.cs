using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Parsing
{
    public class StringParseNode<Tcontroller> : ParseNode<char, string, Tcontroller> where Tcontroller : IParseController
    {
        Action<string, Tcontroller> enterHandle;
        Action<string, Tcontroller> exitHandle;

        public StringParseNode(Action<string, Tcontroller> enterHandle, Action<string, Tcontroller> exitHandle, char[] trimChars, ParseNodeFlags flags = ParseNodeFlags.None, Transition[] transitions = null)
            : base(new StringValueHandler(trimChars), flags, transitions)
        {
            this.enterHandle = enterHandle;
            this.exitHandle = exitHandle;
        }

        protected override void EnterHandle(string value, Tcontroller controller)
        {
            if(enterHandle != null)
                enterHandle(value, controller);
        }
        protected override void ExitHandle(string value, Tcontroller controller)
        {
            if(exitHandle != null)
                exitHandle(value, controller);
        }
    }

    public class StringValueHandler : IParseValueHandler<char, string>
    {
        DefaultValueHandler<char> dvh;

        public StringValueHandler(char[] trimChars)
        {
            dvh = new DefaultValueHandler<char>(trimChars);
        }

        public Value<string> GetValue(List<char> source)
        {
            var result = dvh.GetValue(source);
            return new Value<string> { hasValue = result.hasValue, value = new string(result.value.ToArray()) };
        }
    }
}
