using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public class StringParseNode : ParseNode<char>
    {
        Action<string, char[]> handle;
        char[] trimChars;

        public StringParseNode(Action<string, char[]> handle, char[] trimChars, char[] exitKey)
            : this(handle, trimChars, exitKey, new char[0][], new ParseNode<char>[0], ParseNodeFlags.None)
        {
        }
        public StringParseNode(Action<string, char[]> handle, char[] trimChars, char[] exitKey, char[][] keys, ParseNode<char>[] nodes)
            : this(handle, trimChars, exitKey, keys, nodes, ParseNodeFlags.None)
        {
        }
        public StringParseNode(Action<string, char[]> handle, char[] trimChars, char[] exitKey, char[][] keys, ParseNode<char>[] nodes, ParseNodeFlags flags)
            : base(exitKey, keys, nodes, flags)
        {
            this.handle = handle;
            this.trimChars = trimChars;
        }

        protected override void Handle(List<char> values, char[] key)
        {
            handle(new string(values.ToArray()).Trim(trimChars), key);
        }
    }
}
