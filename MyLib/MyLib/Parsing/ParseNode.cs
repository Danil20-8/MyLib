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
        readonly ParseNodeFlags flags;
        Tsource[] trimKeys;

        bool ignoreEnd { get { return (flags & ParseNodeFlags.IgnoreEnd) != 0; } }
        bool clearOnBack { get { return (flags & ParseNodeFlags.DontClearOnBack) != ParseNodeFlags.DontClearOnBack; } }

        protected abstract void EnterHandle(List<Tsource> values);
        protected abstract void ExitHandle(List<Tsource> values);

        public ParseNode(Transition[] transits, ParseNodeFlags flags)
        {
            this.nodes = transits;
            this.trimKeys = new Tsource[0];
            this.flags = flags;
        }

        public ParseNode(Transition[] transits, Tsource[] trimKeys, ParseNodeFlags flags = ParseNodeFlags.None)
        {
            this.nodes = transits;
            this.trimKeys = trimKeys;
            this.flags = flags;
        }

        public void SetTransitions(Transition[] transitions)
        {
            this.nodes = transitions;
        }

        public void Parse(IEnumerable<Tsource> source)
        {
            var e = source.GetEnumerator();  // if enumerator is struct?
            Parse(ref e, new List<Tsource>());
        }

        void Parse(ref IEnumerator<Tsource> source, List<Tsource> values)
        {
            EnterHandle(values);
            values.Clear();

            Transition[] ts = nodes;

            Tsource temp_s;
            while (source.MoveNext())
            {
                temp_s = source.Current;

                foreach (var key in trimKeys)
                    if (key.Equals(temp_s))
                        goto SKIP_ELEMENT;
                values.Add(temp_s);
                SKIP_ELEMENT:
                Transition transition;

                if (CheckKeys(ts, temp_s, out transition))
                {
                    int keyLength = transition.GetKeyLength(trimKeys);
                    values.RemoveRange(values.Count - keyLength, keyLength); // No keys

                    if (transition.handler != null)
                        transition.handler();
                    if (transition.node != null)
                        transition.node.Parse(ref source, values);
                    if (transition.isExit)
                    {
                        ExitHandle(values);
                        return;
                    }

                    if(clearOnBack)
                        values.Clear();
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

            public int GetKeyLength(Tsource[] trimKeys)
            {
                if (trimKeys.Length == 0) return key.Length;
                int length = 0;
                foreach (var e in key) {
                    foreach (var tk in trimKeys)
                        if (e.Equals(tk)) goto TRIM;
                    ++length;
                    TRIM:
                    continue;
                }
                return length;
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
        IgnoreEnd = 1,
        DontClearOnBack = 2,

    }

    public class EndOfSequenceParseException: Exception
    {

    }
}
