using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing.XML
{
    public class XMLParser
    {
        readonly char[] trimChars = new char[] { ' ' };

        readonly char[] stringValueKey = new char[] { '\"' };

        readonly char[] elementEnterKey = new char[] { '<' };
        readonly char[] elementExitKey = new char[] { '\\', '>' };
        readonly char[] elementNameKey = new char[] { ' ' };

        readonly char[] attributeWaitingKey = new char[] { '=' };
        readonly char[] attributeEndKey = new char[] { ' ' };

        readonly char[] elementOpenKey = new char[] { '>' };
        readonly char[] elementCloseBeginKey = new char[] { '<', '\\' };
        readonly char[] elementCloseEndKey = new char[] { '>' };

        StringParseNode root;

        public Forest<TreeNode<Element>> xmlResult { get; private set; }

        public XMLParser()
        {
            root = new StringParseNode(null, null, trimChars, ParseNodeFlags.IgnoreEnd);
            StringParseNode elementName = new StringParseNode(null, null, trimChars);
            StringParseNode elementNode = new StringParseNode(AddElement, null, trimChars);
            StringParseNode nestedElementsNode = new StringParseNode(null, null, trimChars);
            StringParseNode closeElementNode = new StringParseNode(null, CloseElement, trimChars);

            StringParseNode attributeNode = new StringParseNode(AddAttributeName, AddAttributeValue, trimChars);
            StringParseNode stringValue = new StringParseNode(null, null, trimChars);


            root.SetTransitions(
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = elementEnterKey, node = elementName }
                });
            // Elements
            elementName.SetTransitions(
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition { key = elementNameKey, node = elementNode, flags = ParseTransitionFlags.NoZero | ParseTransitionFlags.Exit },
                    new ParseNode<char>.Transition { key = elementOpenKey, node = elementNode, flags = ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer },
                    new ParseNode<char>.Transition { key = elementExitKey, node = elementNode, flags = ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer }
                });

            elementNode.SetTransitions(
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = elementExitKey, handler = CloseElement, flags = ParseTransitionFlags.Exit },
                    new ParseNode<char>.Transition() { key = elementOpenKey, node = nestedElementsNode, flags = ParseTransitionFlags.Exit },
                    new ParseNode<char>.Transition() { key = attributeWaitingKey, node = attributeNode }
                });

            nestedElementsNode.SetTransitions(
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition { key = elementEnterKey, node = elementName },
                    new ParseNode<char>.Transition { key = elementCloseBeginKey, node = closeElementNode, flags = ParseTransitionFlags.Exit }
                });

            closeElementNode.SetTransitions(
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition { key = elementCloseEndKey, flags = ParseTransitionFlags.Exit }
                });

            // Attributes
            attributeNode.SetTransitions(
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition { key = attributeEndKey, flags = ParseTransitionFlags.Exit | ParseTransitionFlags.NoZero },
                    new ParseNode<char>.Transition { key = elementOpenKey, flags = ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer },
                    new ParseNode<char>.Transition { key = elementExitKey, flags = ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer },
                    new ParseNode<char>.Transition { key = stringValueKey, node = stringValue, flags = ParseTransitionFlags.DontClearOnBack | ParseTransitionFlags.Exit }
                });

            stringValue.SetTransitions(
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition { key = stringValueKey, flags = ParseTransitionFlags.Exit }
                });

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
        void AddElement(string value)
        {
            currElement = new TreeNode<Element>(new Element { name = value, attributes = new Dictionary<string, string>() });
        }
        void CloseElement()
        {
            element.Pop();
        }
        void CloseElement(string value)
        {
            if (currElement.item.name == value)
                element.Pop();
            else
                throw new Exception("Syntax error");
        }

        string attributeName;
        void AddAttributeName(string name)
        {
            attributeName = name;
        }

        void AddAttributeValue(string value)
        {
            currElement.item.attributes.Add(attributeName, value);
        }

        public struct Element
        {
            public string name;
            public Dictionary<string, string> attributes;
        }
    }
}
