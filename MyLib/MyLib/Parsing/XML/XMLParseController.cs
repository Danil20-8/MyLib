using System.Collections.Generic;
using System.Linq;
using System;

namespace MyLib.Parsing.XML
{
    public class XMLParseController : IParseController
    {
        IXMLModule xmlResult;

        public object GetResult()
        {
            return xmlResult;
        }

        public XMLParseController(IXMLModule targetXMLModule) { this.xmlResult = targetXMLModule; }


        IXMLElement currElement { get { return element.Any() ? element.Peek() : null; } }
        Stack<IXMLElement> element = new Stack<IXMLElement>();

        public void AddElement(string value)
        {
            var curr = currElement;
            if (curr != null)
                element.Push(curr.AddElement(value));
            else
                element.Push(xmlResult.AddElement(value));
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
    }
}