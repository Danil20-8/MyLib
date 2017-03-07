using System.Collections.Generic;
using System.Linq;
using System;

namespace DRLib.Parsing.XML
{
    public class XMLParseController : IParseController
    {
        IXMLModule xmlResult;

        public object GetResult()
        {
            return xmlResult;
        }

        public XMLParseController(IXMLModule targetXMLModule) { this.xmlResult = targetXMLModule; }


        ElementData currElement { get { return element.Any() ? element.Peek() : null; } }
        Stack<ElementData> element = new Stack<ElementData>();

        public void AddElement(string value)
        {
            var curr = currElement;
            if (curr != null)
            {
                element.Push(curr.AddElement(value));
            }
            else
                element.Push(new ElementData(xmlResult.AddElement(value)));
        }
        public void CloseElement()
        {
            element.Pop();
        }
        public void CloseElement(string value)
        {
            if (currElement.name == value)
                element.Pop();
            else
                throw new Exception("Syntax error: expecting " + currElement.name + ", got: " + value);
        }

        string attributeName;
        public void AddAttributeName(string name)
        {
            attributeName = name;
        }

        public void AddAttributeValue(string value)
        {
            currElement.AddAttribute(attributeName, value);
        }

        public void SetValue(string value)
        {
            currElement.SetValue(value);
        }

        class ElementData : IXMLElement
        {
            public IXMLElement element;
            public bool hasValue;

            public ElementData(IXMLElement element) { this.element = element; this.hasValue = true; }

            public string name
            {
                get
                {
                    return element.name;
                }
            }

            public void AddAttribute(string name, string value)
            {
                element.AddAttribute(name, value);
            }

            public ElementData AddElement(string name)
            {
                hasValue = false;
                return new ElementData(element.AddElement(name));
            }

            IXMLElement IXMLElement.AddElement(string name)
            {
                return AddElement(name);
            }

            public void SetValue(string value)
            {
                if (hasValue)
                    element.SetValue(value);
            }
        }
    }
}