using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing.SimpleParser
{
    public class CSSParseNode : StringParseNode<CSSParser>
    {
        public CSSParseNode(Action<string, CSSParser> enterHandler, Action<String, CSSParser> exitHandler, char[] trimChars, ParseNodeFlags flags = ParseNodeFlags.None, Transition[] transitions = null)
            :base(enterHandler, exitHandler, trimChars, flags, transitions)
        {
        }
    }
}
