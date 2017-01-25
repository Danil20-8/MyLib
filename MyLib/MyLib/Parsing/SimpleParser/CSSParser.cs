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
        readonly char[] endValueKey = new char[] { ';' };
        readonly char[] beginStringKey = new char[] { '"' };
        readonly char[] endStringKey = new char[] { '"' };

        readonly char[][] rootKeys = new char[][] { new char[] { '.' }, new char[] { '{' } };
        readonly char[] valueKey = new char[] { ';' };
        readonly char[][] optionKeys = new char[][] { new char[] { '=' } };
        readonly char[] optionExitKey = new char[] { '}' };
        readonly char[][] classKeys = new char[][] { new char[] { '{' } };
        public CSSParser()
        {
            var stringValueNode = new StringParseNode(AddOptionValue, trimChars,
                endStringKey);
            var valueNode = new StringParseNode(AddOptionValue, trimChars,
                valueKey,
                new char[][] { beginStringKey },
                new ParseNode<char>[] { stringValueNode });
            var optionNode = new StringParseNode(AddOption, trimChars, optionExitKey, optionKeys, new ParseNode<char>[] { valueNode });
            var classNode = new StringParseNode(AddClass, trimChars, null, classKeys, new ParseNode<char>[] { optionNode }, ParseNodeFlags.OneHit);

            root = new StringParseNode(AddClass, trimChars,
                null,
                new char[][] { preNameKey, optionsEnterKey },
                new ParseNode<char>[] { classNode, optionNode },
                ParseNodeFlags.ToEnd);
        }

        public void Parse(IEnumerable<char> source)
        {
            cssResult = new Dictionary<string, Dictionary<string, string>>();
            root.Parse(source.GetEnumerator());
        }

        void Nope(string value, char[] key) { }
        void AddClass(string value, char[] key)
        {
            if(key == optionsEnterKey)
                cssResult.Add(value, new Dictionary<string, string>());
        }
        void AddOption(string value, char[] key)
        {
            AddOption(value);
        }
        bool skipOne = false;
        void AddOptionValue(string value, char[] key)
        {
            if (key == beginStringKey)
                return;
            if (skipOne)
            {
                skipOne = false;
                return;
            }
            if (key == endStringKey)
                skipOne = true;
            AddValue(value);
        }
        string optionName;
        void AddOption(string name)
        {
            optionName = name;
        }
        void AddValue(string value)
        {
            cssResult.Last().Value.Add(optionName, value);
        }
    }
}
