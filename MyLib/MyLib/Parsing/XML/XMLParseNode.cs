using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing.XML
{
    public class XMLParseNode : StringParseNode<XMLParseController>
    {
        public XMLParseNode(Action<string, XMLParseController> enterHandler, Action<string, XMLParseController> exitHandler, char[] trimChars, ParseNodeFlags flags = ParseNodeFlags.None, Transition[] transitions = null)
            :base(enterHandler, exitHandler, trimChars, flags, transitions)
        {
        }
        }
}
