using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing.XML
{

    /// <summary>
    /// Thread safe implementation
    /// </summary>
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

        XMLParseNode root;

        public XMLParser()
        {
            root = new XMLParseNode(null, null, trimChars, ParseNodeFlags.IgnoreEnd);
            XMLParseNode elementName = new XMLParseNode(null, null, trimChars);
            XMLParseNode elementNode = new XMLParseNode(AddElement, null, trimChars);
            XMLParseNode nestedElementsNode = new XMLParseNode(null, null, new char[0]);
            XMLParseNode closeElementNode = new XMLParseNode(SetValue, CloseElement, trimChars);

            XMLParseNode attributeNode = new XMLParseNode(AddAttributeName, AddAttributeValue, trimChars);
            XMLParseNode stringValue = new XMLParseNode(null, null, new char[0]);


            root.SetTransitions(
                root.newTransitions(
                    root.newTransition(elementEnterKey, elementName)
                ));
            // Elements
            elementName.SetTransitions(
                elementName.newTransitions(
                    elementName.newTransition(elementNameKey, elementNode, ParseTransitionFlags.NoZero | ParseTransitionFlags.Exit ),
                    elementName.newTransition(elementOpenKey, elementNode, ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer ),
                    elementName.newTransition(elementExitKey, elementNode, ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer )
                ));

            elementNode.SetTransitions(
                elementNode.newTransitions(
                    elementNode.newTransition(elementExitKey, CloseElement, ParseTransitionFlags.Exit ),
                    elementNode.newTransition(elementOpenKey, nestedElementsNode, ParseTransitionFlags.Exit ),
                    elementNode.newTransition(attributeWaitingKey, attributeNode )
                ));

            nestedElementsNode.SetTransitions(
                nestedElementsNode.newTransitions(
                    nestedElementsNode.newTransition(elementEnterKey, elementName ),
                    nestedElementsNode.newTransition(elementCloseBeginKey, closeElementNode, ParseTransitionFlags.Exit )
                ));

            closeElementNode.SetTransitions(
                closeElementNode.newTransitions(
                    closeElementNode.newTransition(elementCloseEndKey, ParseTransitionFlags.Exit )
                ));

            // Attributes
            attributeNode.SetTransitions(
                attributeNode.newTransitions(
                    attributeNode.newTransition(attributeEndKey, ParseTransitionFlags.Exit | ParseTransitionFlags.NoZero ),
                    attributeNode.newTransition(elementOpenKey, ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer),
                    attributeNode.newTransition(elementExitKey, ParseTransitionFlags.Exit | ParseTransitionFlags.DontClearKeyBuffer),
                    attributeNode.newTransition(stringValueKey, stringValue, ParseTransitionFlags.DontClearOnBack | ParseTransitionFlags.Exit )
                ));

            stringValue.SetTransitions(
                stringValue.newTransitions(
                    stringValue.newTransition(stringValueKey, ParseTransitionFlags.Exit )
                ));

        }

        public XMLModule Parse(IEnumerable<char> source)
        {
            return (XMLModule) root.Parse(source, new XMLParseController(new XMLModule()));
        }

        public TXMLModule Parse<TXMLModule>(IEnumerable<char> source, TXMLModule xmlRoot) where TXMLModule : class, IXMLModule
        {
            return (TXMLModule) root.Parse(source, new XMLParseController(xmlRoot));
        }

        void AddElement(string value, XMLParseController controller)
        {
            controller.AddElement(value);
        }
        void CloseElement(XMLParseController controller)
        {
            controller.CloseElement();
        }
        void CloseElement(string value, XMLParseController controller)
        {
            controller.CloseElement(value);
        }

        void AddAttributeName(string name, XMLParseController controller)
        {
            controller.AddAttributeName(name);
        }

        void AddAttributeValue(string value, XMLParseController controller)
        {
            controller.AddAttributeValue(value);
        }

        void SetValue(string value, XMLParseController controller)
        {
            controller.SetValue(value);
        }
    }

    public interface IXMLModule
    {
        IXMLElement AddElement(string name);
    }

    public interface IXMLElement
    {
        string name { get; }

        IXMLElement AddElement(string name);
        void AddAttribute(string name, string value);
        void SetValue(string value);
    }

    public class XMLModule : IXMLModule
    {
        public List<XMLElement> childs { get; private set; }

        public XMLModule()
        {
            childs = new List<XMLElement>();
        }

        public IXMLElement AddElement(string name)
        {
            XMLElement e = new XMLElement(name);
            childs.Add(e);
            return e;
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
