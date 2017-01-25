using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public class CustomParseNode<Tsource> : ParseNode<Tsource>
    {
        Action<List<Tsource>, Tsource[]> handle;


        public CustomParseNode(Action<List<Tsource>, Tsource[]> handle, Tsource[] exitKey) : this(handle, exitKey, new Tsource[0][], new ParseNode<Tsource>[0])
        {
        }
        public CustomParseNode(Action<List<Tsource>, Tsource[]> handle, Tsource[] exitKey, ParseNodeFlags flags) : this(handle, exitKey, new Tsource[0][], new ParseNode<Tsource>[0], flags)
        {
        }
        public CustomParseNode(Action<List<Tsource>, Tsource[]> handle, Tsource[] exitKey, Tsource[][] keys, ParseNode<Tsource>[] nodes) : this(handle, exitKey, keys, nodes, ParseNodeFlags.None)
        {
        }
        public CustomParseNode(Action<List<Tsource>, Tsource[]> handle, Tsource[] exitKey, Tsource[][] keys, ParseNode<Tsource>[] nodes, ParseNodeFlags flags)
            : base(exitKey, keys, nodes, flags)
        {
            this.handle = handle;
        }

        protected override void Handle(List<Tsource> values, Tsource[] key)
        {
            handle(values, key);
        }
    }
}
