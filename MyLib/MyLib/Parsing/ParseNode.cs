using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLib.Parsing
{
    public abstract class ParseNode<Tsource>
    {
        Tsource[] exitKey;
        int exitHits;
        Tsource[][] keys;
        ParseNode<Tsource>[] nodes;
        int[] hits;

        ParseNodeFlags flags;
        bool oneHit { get { return (flags & ParseNodeFlags.OneHit) != 0; } }
        bool toEnd { get { return (flags & ParseNodeFlags.ToEnd) != 0; } }

        List<Tsource> values = new List<Tsource>();

        protected bool IsExitKey(Tsource[] key)
        {
            return key.Equals(exitKey);
        }

        protected abstract void Handle(List<Tsource> values, Tsource[] key);

        public ParseNode(Tsource[] exitKey) : this(exitKey, new Tsource[0][], new ParseNode<Tsource>[0])
        {
        }
        public ParseNode(Tsource[] exitKey, Tsource[][] keys, ParseNode<Tsource>[] nodes) : this(exitKey, keys, nodes, ParseNodeFlags.None)
        {
        }
        public ParseNode(Tsource[] exitKey, Tsource[][] keys, ParseNode<Tsource>[] nodes, ParseNodeFlags flags)
        {
            this.exitKey = exitKey;

            this.keys = keys;
            this.hits = new int[keys.Length];
            this.nodes = nodes;

            this.flags = flags;
        }

        public void Parse(IEnumerator<Tsource> source)
        {
            values.Clear();
            Tsource temp_s;
            while (source.MoveNext())
            {
                temp_s = source.Current;
                values.Add(temp_s);
                for (int i = 0; i < keys.Length; ++i)
                {
                    if (keys[i][hits[i]].Equals(temp_s))
                    {
                        if (++hits[i] == keys[i].Length)
                        {
                            Handle(keys[i]);
                            nodes[i].Parse(source);
                            if (oneHit)
                                return;
                            break;
                        }
                    }
                    else
                        hits[i] = 0;
                }
                if (exitKey != null)
                    if (temp_s.Equals(exitKey[exitHits]))
                    {
                        if (++exitHits == exitKey.Length)
                        {
                            Handle(exitKey);
                            return;
                        }
                    }
                    else
                        exitHits = 0;
            }

            if (!toEnd)
                throw new EndOfSequenceParseException();

        }
        void Handle(Tsource[] key)
        {
            values.RemoveRange(values.Count - key.Length, key.Length);
            Handle(values, key);
            values.Clear();
            ZeroHits();
        }
        void ZeroHits()
        {
            for (int i = 0; i < hits.Length; i++)
                hits[i] = 0;

            exitHits = 0;
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
