using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRLib.Algoriphms;

namespace DRLib.Parsing
{
    public abstract class ParseNode<Tsource, Tvalue, Tcontroller> where Tcontroller : IParseController
    {
        IParseValueHandler<Tsource, Tvalue> valueHandler;
        Transition[] nodes;
        Tsource[][] keys;

        readonly ParseNodeFlags flags;

        bool ignoreEnd { get { return (flags & ParseNodeFlags.IgnoreEnd) != 0; } }

        protected abstract void EnterHandle(Tvalue value, Tcontroller controller);
        protected abstract void ExitHandle(Tvalue value, Tcontroller controller);

        public ParseNode(IParseValueHandler<Tsource, Tvalue> valueHandler, ParseNodeFlags flags = ParseNodeFlags.None, Transition[] transitions = null)
        {
            this.valueHandler = valueHandler;
            this.flags = flags;
            SetTransitions(transitions);
        }

        public void SetTransitions(Transition[] transitions)
        {
            if (transitions == null) return;
            if (!ignoreEnd)
                if (transitions.All(t => t.isExit == false))
                    throw new Exception("Not igoring end ParseNode has to contain exit transition");
            this.nodes = transitions;
            keys = this.nodes.Select(n => n.key).ToArray();
        }

        public object Parse(IEnumerable<Tsource> source, Tcontroller controller)
        {
            var pc = new ParseController(source, controller);
            pc.MoveTo(this);
            return pc.controller.GetResult();
        }

        void Parse(ParseController pc)
        {
            EnterHandle(pc.value, pc.controller);
            pc.ClearValues();

            while(pc.Next() != -1)
            {
                Transition transition = pc.transition;

                //removing key from value sequence
                if (transition.clearKeyBuffer)
                    pc.ClearKey();

                //if the value is meaningful
                if (transition.noZero && !pc.hasValue)
                    continue;

                if (transition.handler != null)
                {
                    var node = transition.handler(pc.value, pc.controller);
                    if (node != null) pc.MoveTo(node);
                }
                if (transition.node != null)
                    pc.MoveTo(transition.node);
                if (transition.isExit)
                {
                    ExitHandle(pc.value, pc.controller);
                    return;
                }

                if (transition.clearOnBack)
                    pc.ClearValues();
            }

            if (!ignoreEnd)
                throw new EndOfSequenceParseException();
            ExitHandle(pc.value, pc.controller);
        }

        public Transition newTransition(Tsource[] key, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, flags = flags }; }
        public Transition newTransition(Tsource[] key, Func<Tvalue, Tcontroller, ParseNode<Tsource, Tvalue, Tcontroller>> handler, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, handler = handler, flags = flags }; }
        public Transition newTransition(Tsource[] key, ParseNode<Tsource, Tvalue, Tcontroller> node, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, node = node, flags = flags }; }
        public Transition newTransition(Tsource[] key, ParseNode<Tsource, Tvalue, Tcontroller> node, Func<Tvalue, Tcontroller, ParseNode<Tsource, Tvalue, Tcontroller>> handler, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, node = node, handler = handler, flags = flags }; }
        public Transition[] newTransitions(params Transition[] transitions) { return transitions; }

        public class Transition
        {
            public Tsource[] key;
            public Func<Tvalue, Tcontroller, ParseNode<Tsource, Tvalue, Tcontroller>> handler;
            public ParseNode<Tsource, Tvalue, Tcontroller> node;
            public ParseTransitionFlags flags;

            public bool isExit { get { return (flags & ParseTransitionFlags.Exit) != 0; } }
            public bool noZero { get { return (flags & ParseTransitionFlags.NoZero) != 0; } }
            public bool clearOnBack { get { return (flags & ParseTransitionFlags.DontClearOnBack) != ParseTransitionFlags.DontClearOnBack; } }
            public bool clearKeyBuffer { get { return (flags & ParseTransitionFlags.DontClearKeyBuffer) != ParseTransitionFlags.DontClearKeyBuffer; } }
        }

        class ParseController
        {
            public readonly Tcontroller controller;
            SequenceReader<Tsource> sr;

            ParseNode<Tsource, Tvalue, Tcontroller> node;

            Value<Tvalue> valueResult;
            public Tvalue value { get { return valueResult.value; } }
            public bool hasValue { get { return valueResult.hasValue; } }

            public Transition transition { get { return node.nodes[sr.Key]; } }

            public void ClearValues()
            {
                sr.ClearValues();
            }

            public ParseController(IEnumerable<Tsource> source, Tcontroller controller)
            {
                sr = new SequenceReader<Tsource>(source);
                this.controller = controller;
            }

            public void SetNode(ParseNode<Tsource, Tvalue, Tcontroller> node)
            {
                this.node = node;
                if (node != null)
                    sr.SetKeys(node.keys);
            }

            public void ClearKey()
            {
                sr.ClearKey();
            }

            public void MoveTo(ParseNode<Tsource, Tvalue, Tcontroller> node)
            {
                var t = this.node;
                SetNode(node);
                node.Parse(this);
                SetNode(t);
            }

            public int Next()
            {
                sr.Next();
                valueResult = node.valueHandler.GetValue(sr.Result);
                return sr.Key;
            }

        }
    }

    public enum ParseNodeFlags
    {
        None = 0,
        IgnoreEnd = 1,

    }

    public enum ParseTransitionFlags
    {
        None = 0,
        Exit = 1,
        NoZero = 2,
        DontClearOnBack = 4,
        DontClearKeyBuffer = 8,

    }

    public class EndOfSequenceParseException: Exception
    {

    }

    public class ParseData<TSource>
    {
        public IEnumerable<TSource> values;
        public TSource[] key;
    }
}
