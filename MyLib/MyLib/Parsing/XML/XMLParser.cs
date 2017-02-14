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
        readonly char[] elementExitKey = new char[] { '/', '>' };
        readonly char[] elementNameKey = new char[] { ' ' };

        readonly char[] attributeWaitingKey = new char[] { '=' };
        readonly char[] attributeEndKey = new char[] { ' ' };

        readonly char[] elementOpenKey = new char[] { '>' };
        readonly char[] elementCloseBeginKey = new char[] { '<', '/' };
        readonly char[] elementCloseEndKey = new char[] { '>' };

        StringParseNode root;

        public IXMLElement xmlResult { get; private set; }

        public XMLParser()
        {
            root = new StringParseNode(null, null, trimChars, ParseNodeFlags.IgnoreEnd);
            StringParseNode elementName = new StringParseNode(null, null, trimChars);
            StringParseNode elementNode = new StringParseNode(AddElement, null, trimChars);
            StringParseNode nestedElementsNode = new StringParseNode(null, null, new char[0]);
            StringParseNode closeElementNode = new StringParseNode(SetValue, CloseElement, trimChars);

            StringParseNode attributeNode = new StringParseNode(AddAttributeName, AddAttributeValue, trimChars);
            StringParseNode stringValue = new StringParseNode(null, null, new char[0]);


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

        public XMLRoot Parse(IEnumerable<char> source)
        {
            xmlResult = new XMLRoot();
            root.Parse(source);
            return xmlResult as XMLRoot;
        }

        public TRoot Parse<TRoot>(IEnumerable<char> source, TRoot xmlRoot) where TRoot : class, IXMLElement
        {
            xmlResult = xmlRoot;
            root.Parse(source);
            return xmlResult as TRoot;
        }

        IXMLElement currElement { get { return element.Any() ? element.Peek() : null; } }
        Stack<IXMLElement> element = new Stack<IXMLElement>();
        void AddElement(string value)
        {
            var curr = currElement;
            if (curr != null)
                element.Push(curr.AddElement(value));
            else
                element.Push(xmlResult.AddElement(value));
        }
        void CloseElement()
        {
            element.Pop();
        }
        void CloseElement(string value)
        {
            if (currElement.name == value)
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
            currElement.AddAttribute(attributeName, value);
        }

        void SetValue(string value)
        {
            currElement.SetValue(value);
        }
    }

    public interface IXMLElement
    {
        string name { get; }

        IXMLElement AddElement(string name);
        void AddAttribute(string name, string value);
        void SetValue(string value);
    }

    public class XMLRoot : IXMLElement
    {
        public List<XMLElement> childs { get; private set; }

        string IXMLElement.name { get{return "XMLRoot"; } }

        public XMLRoot()
        {
            childs = new List<XMLElement>();
        }

        void IXMLElement.AddAttribute(string name, string value)
        {
        }

        public IXMLElement AddElement(string name)
        {
            XMLElement e = new XMLElement(name);
            childs.Add(e);
            return e;
        }

        void IXMLElement.SetValue(string value)
        {
        }
    }

    public class XMLElement : IXMLElement
    {
        public string name { get; private set; }
        public string value { get; private set; }
        public Dictionary<string, string> attributes { get; private set; }
        public List<XMLElement> childs { get; private set; }

        public XMLElement(string name)
        {
            this.name = name;
            attributes = new Dictionary<string, string>();
            childs = new List<XMLElement>();
        }

        public IXMLElement AddElement(string name)
        {
            XMLElement e = new XMLElement(name);
            childs.Add(e);
            return e;
        }

        public void AddAttribute(string name, string value)
        {
            attributes.Add(name, value);
        }

        public void SetValue(string value)
        {
            this.value = value;
        }
    }
}
