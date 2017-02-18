using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyLib.Algoriphms;

namespace MyLib.Parsing
{
    public abstract class ParseNode<Tsource, Tvalue, Tcontroller> where Tcontroller : IParseController
    {
        IParseValueHandler<Tsource, Tvalue> valueHandler;
        Transition[] nodes;
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
        }

        public object Parse(IEnumerable<Tsource> source, Tcontroller controller)
        {
            var sr = new SequenceReader(source, controller);
            sr.MoveTo(this);
            return sr.controller.GetResult();
        }

        void Parse(SequenceReader sr)
        {
            EnterHandle(sr.value, sr.controller);
            sr.ClearValues();

            while(sr.Next() != -1)
            {
                Transition transition = sr.transition;

                //removing key from value sequence
                if (transition.clearKeyBuffer)
                    sr.ClearKey();

                //if the value is meaningful
                if (transition.noZero && !sr.hasValue)
                    continue;

                if (transition.handler != null)
                    transition.handler(sr.controller);
                if (transition.node != null)
                    sr.MoveTo(transition.node);
                if (transition.isExit)
                {
                    ExitHandle(sr.value, sr.controller);
                    return;
                }

                if (transition.clearOnBack)
                    sr.ClearValues();
            }

            if (!ignoreEnd)
                throw new EndOfSequenceParseException();
            ExitHandle(sr.value, sr.controller);
        }

        public Transition newTransition(Tsource[] key, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, flags = flags }; }
        public Transition newTransition(Tsource[] key, Action<Tcontroller> handler, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, handler = handler, flags = flags }; }
        public Transition newTransition(Tsource[] key, ParseNode<Tsource, Tvalue, Tcontroller> node, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, node = node, flags = flags }; }
        public Transition newTransition(Tsource[] key, ParseNode<Tsource, Tvalue, Tcontroller> node, Action<Tcontroller> handler, ParseTransitionFlags flags = ParseTransitionFlags.None) { return new Transition { key = key, node = node, handler = handler, flags = flags }; }
        public Transition[] newTransitions(params Transition[] transitions) { return transitions; }

        public struct Transition
        {
            public Tsource[] key;
            public Action<Tcontroller> handler;
            public ParseNode<Tsource, Tvalue, Tcontroller> node;
            public ParseTransitionFlags flags;

            public bool isExit { get { return (flags & ParseTransitionFlags.Exit) != 0; } }
            public bool noZero { get { return (flags & ParseTransitionFlags.NoZero) != 0; } }
            public bool clearOnBack { get { return (flags & ParseTransitionFlags.DontClearOnBack) != ParseTransitionFlags.DontClearOnBack; } }
            public bool clearKeyBuffer { get { return (flags & ParseTransitionFlags.DontClearKeyBuffer) != ParseTransitionFlags.DontClearKeyBuffer; } }
        }

        class SequenceReader
        {
            public readonly Tcontroller controller;

            List<Tsource> values = new List<Tsource>();
            Queue<Tsource> keyBuffer = new Queue<Tsource>();

            Transition[] transitions { get { return node.nodes; } }
            ParseNode<Tsource, Tvalue, Tcontroller> node;
            List<int> hits = new List<int>();
            int hitsLength { get { return hits.Count; } }

            int maxKeyLength;
            int transitionIndex;
            IEnumerator<Tsource> source;

            Value<Tvalue> valueResult;
            public Tvalue value { get { return valueResult.value; } }
            public bool hasValue { get { return valueResult.hasValue; } }

            public Transition transition { get { return transitions[transitionIndex]; } }
            public bool isEnd { get; private set; }

            public void ClearValues()
            {
                values.Clear();
            }

            public SequenceReader(IEnumerable<Tsource> source, Tcontroller controller)
            {
                this.source = source.GetEnumerator();
                this.controller = controller;
            }

            public void SetNode(ParseNode<Tsource, Tvalue, Tcontroller> node)
            {
                this.node = node;
                if (node != null)
                {
                    hits.SetSize(node.nodes.Length);
                    maxKeyLength = transitions.Max(t => t.key.Length);
                }
            }

            public void ClearKey()
            {
                for (int i = 0; i < transition.key.Length; ++i)
                    keyBuffer.Dequeue();
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
                for (;;)
                {
                    while (keyBuffer.Count != maxKeyLength && source.MoveNext())
                        keyBuffer.Enqueue(source.Current);

                    SetResult(CheckKeys());
                    if (transitionIndex != -1)
                        return transitionIndex;

                    if (keyBuffer.Count == 0)
                        break;

                    AddValue(keyBuffer.Dequeue());
                }
                isEnd = true;
                SetResult(-1);
                return transitionIndex;
            }

            void SetResult(int value)
            {
                transitionIndex = value;
                if(value != -1)
                    valueResult = node.valueHandler.GetValue(values);
            }

            void AddValue(Tsource value)
            {
                /*foreach (var tk in node.trimKeys)
                    if (value.Equals(tk))
                        return;*/
                values.Add(value);
            }

            int CheckKeys()
            {
                int maxHits = 0;
                int maxIndex = 0;
                int miss = 0;
                foreach(var c in keyBuffer)
                {
                    for(int i = 0; i < hitsLength; ++i)
                    {
                        if(transitions[i].key.Length != hits[i] && hits[i] != -1)
                        {
                            if (transitions[i].key[hits[i]].Equals(c))
                            {
                                if (++hits[i] == transitions[i].key.Length)
                                    if (maxHits < hits[i])
                                    {
                                        maxHits = hits[i];
                                        maxIndex = i;
                                    }
                            }
                            else {
                                if (++miss == hitsLength - 1 && maxHits > 0)
                                    goto ONE_OF_ALL_FOUNDED;

                                hits[i] = -1;
                            }
                        }
                    }
                }
                ONE_OF_ALL_FOUNDED:
                ZeroHits();
                return maxHits > 0 ? maxIndex : -1;
            }

            void ZeroHits()
            {
                for (int i = 0; i < hitsLength; ++i)
                    hits[i] = 0;
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
