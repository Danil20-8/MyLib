using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyLib.Algoriphms;

namespace MyLib.Parsing
{
    public abstract class ParseNode<Tsource>
    {
        Transition[] nodes;
        readonly ParseNodeFlags flags;
        Tsource[] trimKeys;

        bool ignoreEnd { get { return (flags & ParseNodeFlags.IgnoreEnd) != 0; } }

        protected abstract void EnterHandle(List<Tsource> values);
        protected abstract void ExitHandle(List<Tsource> values);

        public ParseNode(ParseNodeFlags flags = ParseNodeFlags.None)
            :this(new Tsource[0], flags)
        {
        }

        public ParseNode(Tsource[] trimKeys, ParseNodeFlags flags = ParseNodeFlags.None)
        {
            this.trimKeys = trimKeys;
            this.flags = flags;
        }

        public ParseNode(Transition[] transits, Tsource[] trimKeys, ParseNodeFlags flags = ParseNodeFlags.None)
        {
            this.trimKeys = trimKeys;
            this.flags = flags;
            SetTransitions(transits);
        }

        public void SetTransitions(Transition[] transitions)
        {
            if (!ignoreEnd)
                if (transitions.All(t => t.isExit == false))
                    throw new Exception("Not igoring end ParseNode has to contain exit transition");
            this.nodes = transitions;
        }

        public void Parse(IEnumerable<Tsource> source)
        {
            new SequenceReader(source).MoveTo(this);
        }

        void Parse(SequenceReader sr)
        {
            EnterHandle(sr.values);
            sr.values.Clear();

            while(sr.Next() != -1)
            {
                Transition transition = sr.transition;

                if (transition.clearKeyBuffer)
                    sr.ClearKey();

                if (transition.noZero && sr.values.Count == 0)
                    continue;

                if (transition.handler != null)
                    transition.handler();
                if (transition.node != null)
                    sr.MoveTo(transition.node);
                if (transition.isExit)
                {
                    ExitHandle(sr.values);
                    return;
                }

                if (transition.clearOnBack)
                    sr.values.Clear();
            }

            if (!ignoreEnd)
                throw new EndOfSequenceParseException();
            ExitHandle(sr.values);
        }

        public struct Transition
        {
            public Tsource[] key;
            public Action handler;
            public ParseNode<Tsource> node;
            public bool isExit { get { return (flags & ParseTransitionFlags.Exit) != 0; } }
            public bool noZero { get { return (flags & ParseTransitionFlags.NoZero) != 0; } }
            public bool clearOnBack { get { return (flags & ParseTransitionFlags.DontClearOnBack) != ParseTransitionFlags.DontClearOnBack; } }
            public bool clearKeyBuffer { get { return (flags & ParseTransitionFlags.DontClearKeyBuffer) != ParseTransitionFlags.DontClearKeyBuffer; } }
            public ParseTransitionFlags flags;
        }

        class SequenceReader
        {
            List<Tsource> _values = new List<Tsource>();
            Queue<Tsource> keyBuffer = new Queue<Tsource>();

            Transition[] transitions { get { return node.nodes; } }
            ParseNode<Tsource> node;
            int[] hits;
            int maxKeyLength;
            int result;
            IEnumerator<Tsource> source;

            public List<Tsource> values { get { return _values; } }
            public Transition transition { get { return transitions[result]; } }
            public bool isEnd { get; private set; }

            public SequenceReader(IEnumerable<Tsource> source)
            {
                this.source = source.GetEnumerator();
            }

            public void SetNode(ParseNode<Tsource> node)
            {
                this.node = node;
                if (node != null)
                {
                    hits = new int[node.nodes.Length];
                    maxKeyLength = transitions.Max(t => t.key.Length);
                }
            }

            public void ClearKey()
            {
                for (int i = 0; i < transition.key.Length; ++i)
                    keyBuffer.Dequeue();
            }

            public void MoveTo(ParseNode<Tsource> node)
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
                    if (result != -1)
                        return result;

                    if (keyBuffer.Count == 0)
                        break;

                    AddValue(keyBuffer.Dequeue());
                }
                isEnd = true;
                SetResult(-1);
                return result;
            }

            void SetResult(int value)
            {
                result = value;
                if(result != -1)
                {
                    for (int i = values.Count - 1; i >= 0; --i)
                        if (!HasToBeTrim(values[i]))
                            break;
                        else
                            values.RemoveAt(i);
                    for(int i = 0; i < values.Count; ++i)
                        if (!HasToBeTrim(values[i]))
                            break;
                        else
                            values.RemoveAt(i);
                }
            }

            bool HasToBeTrim(Tsource value)
            {
                foreach (var tk in node.trimKeys)
                    if (value.Equals(tk))
                        return true;
                return false;
            }

            void AddValue(Tsource value)
            {
                foreach (var tk in node.trimKeys)
                    if (value.Equals(tk))
                        return;
                values.Add(value);
            }

            int CheckKeys()
            {
                int maxHits = 0;
                int maxIndex = 0;
                int miss = 0;
                foreach(var c in keyBuffer)
                {
                    for(int i = 0; i < hits.Length; ++i)
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
                                if (++miss == hits.Length - 1 && maxHits > 0)
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
                for (int i = 0; i < hits.Length; ++i)
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
}
