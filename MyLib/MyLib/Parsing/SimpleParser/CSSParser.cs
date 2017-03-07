using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Parsing.SimpleParser
{
    /// <summary>
    /// Thread unsafe
    /// </summary>
    public class CSSParser : IParseController
    {
        public Dictionary<string, Dictionary<string, string>> cssResult = new Dictionary<string, Dictionary<string, string>>();

        CSSParseNode root;

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
            CSSParseNode sample = new CSSParseNode(null, null, null);

            var stringValue = new CSSParseNode(null, null, trimChars,
                transitions: sample.newTransitions(
                    sample.newTransition(endStringKey, ParseTransitionFlags.Exit )
                ));
            var valueNode = new CSSParseNode(AddOption, AddValue, trimChars,
                transitions : sample.newTransitions(
                    sample.newTransition(valueKey, ParseTransitionFlags.Exit),
                    sample.newTransition(beginStringKey, stringValue, ParseTransitionFlags.DontClearOnBack )
                ));

            var optionNode = new CSSParseNode(AddClass, null, trimChars,
                transitions: sample.newTransitions(
                    sample.newTransition(waitingValueKey, valueNode ),
                    sample.newTransition(optionsExitKey, ParseTransitionFlags.Exit )
                ));

            root = new CSSParseNode(null, null, trimChars, ParseNodeFlags.IgnoreEnd,
                sample.newTransitions(
                    sample.newTransition(preNameKey, MarkAsExist ),
                    sample.newTransition(optionsEnterKey, optionNode )
                ));
        }

        public void Parse(IEnumerable<char> source)
        {
            cssResult = new Dictionary<string, Dictionary<string, string>>();
            root.Parse(source, this);
        }
        bool exist = false;
        void MarkAsExist(CSSParser parser)
        {
            exist = true;
        }

        Dictionary<string, string> currClass;
        void AddClass(string value, CSSParser parser)
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
        void AddOption(string name, CSSParser parse)
        {
            optionName = name;
        }

        void AddValue(string value, CSSParser parser)
        {
            currClass.Add(optionName, value);
        }

        object IParseController.GetResult()
        {
            return cssResult;
        }
    }
}
