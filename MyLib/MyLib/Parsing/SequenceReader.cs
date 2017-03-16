using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRLib.Algoriphms;

namespace DRLib.Parsing
{
    public class SequenceReader<TSource>
    {
        List<TSource> values = new List<TSource>();
        Queue<TSource> keyBuffer = new Queue<TSource>();

        TSource[][] keys;
        List<int> hits = new List<int>();
        int hitsLength { get { return hits.Count; } }

        int maxKeyLength;
        int keyIndex;
        IEnumerator<TSource> source;

        public List<TSource> Result { get { return values; } }
        public bool End { get; private set; }
        public int Key { get { return keyIndex; } }

        public void ClearValues()
        {
            values.Clear();
        }

        public SequenceReader(IEnumerable<TSource> source)
        {
            this.source = source.GetEnumerator();
        }

        public void SetKeys(params TSource[][] keys)
        {
            this.keys = keys;
            hits.SetSize(keys.Length);
            maxKeyLength = keys.Max(k => k.Length);
        }

        public void ClearKey()
        {
            for (int i = 0; i < keys[keyIndex].Length; ++i)
                keyBuffer.Dequeue();
        }

        public int Next()
        {
            for (;;)
            {
                while (keyBuffer.Count < maxKeyLength && source.MoveNext())
                    keyBuffer.Enqueue(source.Current);

                keyIndex = CheckKeys();
                if (keyIndex != -1)
                    return keyIndex;

                if (keyBuffer.Count == 0)
                    break;

                values.Add(keyBuffer.Dequeue());
            }
            End = true;
            keyIndex = -1;
            return keyIndex;
        }

        int CheckKeys()
        {
            int maxHits = 0;
            int maxIndex = 0;
            int miss = 0;

            int count = 0;

            foreach (var c in keyBuffer)
            {
                if (count == maxKeyLength) // preservation of keyBuffer count greater then max key length
                    break;
                for (int i = 0; i < hitsLength; ++i)
                {
                    if (keys[i].Length != hits[i] && hits[i] != -1)
                    {
                        if (keys[i][hits[i]].Equals(c))
                        {
                            if (++hits[i] == keys[i].Length)
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
                ++count;
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
