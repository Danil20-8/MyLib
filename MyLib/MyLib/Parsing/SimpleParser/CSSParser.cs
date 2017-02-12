using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing.SimpleParser
{
    public class CSSParser
    {
        public Dictionary<string, Dictionary<string, string>> cssResult = new Dictionary<string, Dictionary<string, string>>();

        StringParseNode root;

        readonly char[] trimChars = new char[] { ' ' };

        readonly char[] preNameKey = new char[] { '.' };
        readonly char[] optionsEnterKey = new char[] { '{' };
        readonly char[] optionsExitKey = new char[] { '}' };
        readonly char[] waitingValueKey = new char[] { '=' };
        readonly char[] beginStringKey = new char[] { '"' };
        readonly char[] endStringKey = new char[] { '"' };
        readonly char[] valueKey = new char[] { ';' };

        public CSSParser()
        {
            var stringValue = new StringParseNode(null, null, trimChars,
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = endStringKey, isExit = true }
                });
            var valueNode = new StringParseNode(AddOption, AddValue, trimChars,
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = valueKey, isExit = true },
                    new ParseNode<char>.Transition() { key = beginStringKey, node = stringValue }
                });

            var optionNode = new StringParseNode(AddClass, null, trimChars,
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = waitingValueKey, node = valueNode },
                    new ParseNode<char>.Transition() { key = optionsExitKey, isExit = true }
                });

            root = new StringParseNode(null, null, trimChars,
                new ParseNode<char>.Transition[]
                {
                    new ParseNode<char>.Transition() { key = preNameKey, handler = MarkAsExist },
                    new ParseNode<char>.Transition() { key = optionsEnterKey, node = optionNode }
                }, true);
        }

        public void Parse(IEnumerable<char> source)
        {
            cssResult = new Dictionary<string, Dictionary<string, string>>();
            root.Parse(source.GetEnumerator());
        }
        bool exist = false;
        void MarkAsExist()
        {
            exist = true;
        }

        Dictionary<string, string> currClass;
        void AddClass(string value)
        {
            if (exist)
            {
                currClass = cssResult[value];
                exist = false;
            }
            else
            {
                currClass = new Dictionary<string, string>();
                cssResult.Add(value, currClass);
            }
        }

        string optionName;
        void AddOption(string name)
        {
            optionName = name;
        }

        void AddValue(string value)
        {
            currClass.Add(optionName, value);
        }
    }
}
