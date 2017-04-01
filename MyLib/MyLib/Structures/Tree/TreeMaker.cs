using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRLib.Modern;

namespace DRLib.Structures.Tree
{
    public static class TreeMaker
    {
        public static void AddChild<TNode>(this TNode self, TNode node, params int[] path)
            where TNode : IHaveParent<TNode>, IHaveChilds<TNode>, ITreemaker<TNode>
        {
            TNode r = path.Length > 0 ? self.GetNode(path) : self;
            node.SetParent(r);
        }
        public static void RemoveChild<TNode>(this TNode self, TNode node, params int[] path)
            where TNode : IHaveChilds<TNode>, ITreemaker<TNode>
        {
            TNode r = path.Length > 0 ? self.GetNode(path) : self;
            r.RemoveChild(node);
        }
        public static TResult BuildTree<TSource, TResult>(this TSource self, Func<TSource, TResult> selector)
            where TSource : IHaveChilds<TSource>
            where TResult : IHaveParent<TResult>, IHaveChilds<TResult>, ITreemaker<TResult>
        {
            TResult root = selector(self);
            IEnumerable<Tuple<TSource, TResult>> pre = new Tuple<TSource, TResult>[] { new Tuple<TSource, TResult>(self, root) };
            List<Tuple<TSource, TResult>> curr;
            foreach (var n in self.ByLevels().Skip(1))
            {
                curr = new List<Tuple<TSource, TResult>>();
                foreach (var p in pre)
                    foreach (var c in p.Item1.childs)
                    {
                        TResult t = selector(c);
                        AddChild(p.Item2, t);
                        curr.Add(new Tuple<TSource, TResult>(c, t));
                    }
                pre = curr;
            }
            return root;
        }

        public static void Break<TNode>(this TNode self)
            where TNode : IHaveParent<TNode>, IHaveChilds<TNode>, ITreemaker<TNode>
        {
            foreach (var n in self.ByElements().ToList())
                n.Release();
        }

        public static void Clear<TNode>(this TNode node)
            where TNode : IHaveChilds<TNode>, ITreemaker<TNode>
        {
            var childs = node.childs;
            while(childs.Any())
            {
                node.RemoveChild(childs.First());
                childs = node.childs;
            }
        }

        public static void Release<TNode>(this TNode self)
            where TNode : IHaveParent<TNode>, ITreemaker<TNode>
        {
            if (self.parent != null)
                self.parent.RemoveChild(self);
        }

        public static void SetParent<TNode>(this TNode self, TNode node)
            where TNode : IHaveParent<TNode>, ITreemaker<TNode>
        {
            self.Release();
            node.AddChild(self);
        }
    }
}
