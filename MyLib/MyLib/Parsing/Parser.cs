using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public class Parser<Tsource>
    {
        ParseNode<Tsource> rootNode;

        public Parser(ParseNode<Tsource> node)
        {
            this.rootNode = node;
        }
        public void Parse(IEnumerable<Tsource> source)
        {
            rootNode.Parse(source.GetEnumerator());
        }
    }
}
