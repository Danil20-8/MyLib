using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public class StringParseNode : ParseNode<char>
    {
        Action<string> enterHandle;
        Action<string> exitHandle;


        public StringParseNode(Action<string> enterHandle, Action<string> exitHandle, char[] trimChars, Transition[] transits, ParseNodeFlags flags = ParseNodeFlags.None)
            : base(transits, trimChars, flags)
        {
            this.enterHandle = enterHandle;
            this.exitHandle = exitHandle;
        }

        protected override void EnterHandle(List<char> values)
        {
            if(enterHandle != null)
                enterHandle(new string(values.ToArray()));
        }
        protected override void ExitHandle(List<char> values)
        {
            if(exitHandle != null)
                exitHandle(new string(values.ToArray()));
        }
    }
}
