using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing.XML
{
    public class XMLParser
    {
        readonly char[] trimChars = new char[] { ' ' };

        readonly char[] elementEnter = new char[] { '<' };
        readonly char[] elementExit = new char[] { '\\', '>' };

        readonly char[] attributeWaiting = new char[] { '=' };
        readonly char[] attributeEnd = new char[] { ' ' };

        readonly char[] elementOpen = new char[] { '>' };
        readonly char[] elementCloseBegin = new char[] { '<', '\\' };
        readonly char[] elementCloseEnd = new char[] { '>' };

        StringParseNode root;

        Forest<TreeNode<Element>> xmlResult;

        public XMLParser()
        {
            StringParseNode attributeNode = new StringParseNode(null, null, trimChars,
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = attributeEnd, isExit = true }
                });

            StringParseNode elementNode = new StringParseNode(null, null, trimChars,
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = elementExit, isExit = true },
                    new ParseNode<char>.Transition() { key = elementOpen },
                    new ParseNode<char>.Transition() { key = attributeWaiting }
                });

            root = new StringParseNode(null, null, trimChars,
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = elementEnter, node = elementNode }
                }, ParseNodeFlags.IgnoreEnd);
        }

        public void Parse(IEnumerable<char> source)
        {
            xmlResult = new Forest<TreeNode<Element>>();
            root.Parse(source);
        }

        TreeNode<Element> currElement { get { return element.Any() ? element.Peek() : null; }
            set {
                var t = currElement;
                if (t != null)
                    t.AddNode(value);
                else
                    xmlResult.AddTree(value);
                element.Push(value);
            }
        }
        Stack<TreeNode<Element>> element = new Stack<TreeNode<Element>>();
        public void AddElement(string value)
        {
            currElement = new TreeNode<Element>(new Element { name = value, attributes = new Dictionary<string, string>() });
        }
        public void CloseElement()
        {
            element.Pop();
        }
        public void CloseElement(string value)
        {
            if (currElement.item.name == value)
                element.Pop();
            else
                throw new Exception("Syntax error");
        }
        public struct Element
        {
            public string name;
            public Dictionary<string, string> attributes;
        }
    }
}
