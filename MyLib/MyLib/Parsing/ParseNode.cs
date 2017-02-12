using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public abstract class ParseNode<Tsource>
    {
        // saving nodes in multithreading usage. you cannot change it
        ArraySafe<Transition> nodes;
        readonly bool ignoreEnd;

        protected abstract void EnterHandle(List<Tsource> values);
        protected abstract void ExitHandle(List<Tsource> values);

        public ParseNode(bool ignoreEnd = false)
        {
            this.ignoreEnd = ignoreEnd;
        }

        public ParseNode(Transition[] transits, bool ignoreEnd = false)
        {
            this.nodes = transits;
            this.ignoreEnd = ignoreEnd;
        }

        public void SetTransitions(Transition[] transitions)
        {
            this.nodes = transitions;
        }

        public void Parse(IEnumerator<Tsource> source)
        {
            Parse(source, new List<Tsource>());
        }

        void Parse(IEnumerator<Tsource> source, List<Tsource> values)
        {
            EnterHandle(values);
            values.Clear();

            Transition[] ts = nodes;

            Tsource temp_s;
            while (source.MoveNext())
            {
                temp_s = source.Current;
                values.Add(temp_s);

                Transition transition;

                if(CheckKeys(ts, temp_s, out transition))
                {
                    values.RemoveRange(values.Count - transition.key.Length, transition.key.Length); // No keys

                    if (transition.handler != null)
                        transition.handler();
                    if (transition.node != null)
                        transition.node.Parse(source, values);
                    if (transition.isExit)
                    {
                        ExitHandle(values);
                        return;
                    }
                }
            }

            if (!ignoreEnd)
                throw new EndOfSequenceParseException();
            ExitHandle(values);
        }

        bool CheckKeys(Transition[] transitions, Tsource value, out Transition result)
        {
            foreach(var t in transitions)
            {
                if(t.Try(value))
                {
                    foreach (var tt in transitions)
                        tt.ZeroHits();
                    result = t;
                    return true;
                }
            }
            result = default(Transition);
            return false;
        }

        public struct Transition
        {
            public Tsource[] key;
            public Action handler;
            public ParseNode<Tsource> node;
            public bool isExit;

            public Transition(Tsource[] key)
            {
                this.key = key;
                handler = null;
                node = null;
                isExit = false;

                hits = 0;
            }

            private int hits;
            public bool Try(Tsource value) { if (key[hits].Equals(value)) return ++hits == key.Length ? true : false; else { hits = 0; return false; } }
            public void ZeroHits() { hits = 0; }

        }
        class ArraySafe<TElement> where TElement : struct // else it has no meaning
        {
            private TElement[] array;
            public ArraySafe(TElement[] array) { this.array = array; }

            public TElement[] Get() { TElement[] t = new TElement[array.Length]; array.CopyTo(t, 0); return t; }

            public static implicit operator ArraySafe<TElement>(TElement[] array) { return new ArraySafe<TElement>(array); }
            public static implicit operator TElement[](ArraySafe<TElement> array) { return array.Get(); }
        }
    }

    public enum ParseNodeFlags
    {
        None = 0,
        OneHit = 1,
        ToEnd = 2,

    }

    public class EndOfSequenceParseException: Exception
    {

    }
}
