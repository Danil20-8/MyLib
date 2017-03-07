using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRLib.Algoriphms;
using DRLib.Modern;

namespace DRLib
{
    public interface ITreeable<Tnode> where Tnode : ITreeable<Tnode>
    {
        Tnode root { get; set; }
        IEnumerable<Tnode> childs { get; }
        void AddChild(Tnode child);
        void RemoveChild(Tnode child);
    }

    public interface IForest<Tnode> where Tnode : ITreeable<Tnode>
    {
        IEnumerable<Tnode> trees { get; } 
    }

    public static class Treeable
    {

        public static void AddNode<Tnode>(this Tnode self, Tnode node, params int[] path) where Tnode : ITreeable<Tnode>
        {
            Tnode r = path.Length > 0 ? self.GetNode(path) : self;
            r.AddChild(node);
            node.SetRoot(r);
        }
        public static void RemoveNode<Tnode>(this Tnode self, Tnode node, params int[] path) where Tnode : ITreeable<Tnode>
        {
            Tnode r = path.Length > 0 ? self.GetNode(path) : self;
            r.RemoveChild(node);
            node.SetRoot(default(Tnode));
        }
        public static Tresult BuildTree<Tnode, Tresult>(this Tnode self, Func<Tnode, Tresult> selector) where Tnode : ITreeable<Tnode> where Tresult : ITreeable<Tresult>
        {
            Tresult root = selector(self);
            IEnumerable<Tuple<Tnode, Tresult>> pre = new Tuple<Tnode, Tresult>[] { new Tuple<Tnode, Tresult>(self, root) };
            List<Tuple<Tnode, Tresult>> curr;
            foreach (var n in self.ByLevels().Skip(1))
            {
                curr = new List<Tuple<Tnode, Tresult>>();
                foreach (var p in pre)
                    foreach (var c in p.Item1.childs)
                    {
                        Tresult t = selector(c);
                        p.Item2.AddNode(t);
                        curr.Add(new Tuple<Tnode, Tresult>(c, t));
                    }
                pre = curr;
            }
            return root;
        }

        public static void Clear<Tnode>(this Tnode self) where Tnode : ITreeable<Tnode>
        {
            foreach (var n in self.childs)
                n.Clear();
            var e = self.childs.GetEnumerator();
            e.MoveNext();
            var pre = e.Current;
            while (e.MoveNext())
            {
                self.RemoveChild(pre);
                pre = e.Current;
            }
            self.RemoveChild(pre);

        }

        public static void Release<Tnode>(this Tnode self) where Tnode : ITreeable<Tnode>
        {
            if (self.root != null)
                self.root.RemoveChild(self);
        }

        public static Tnode GetNode<Tnode>(this Tnode self, params int[] path) where Tnode : ITreeable<Tnode>
        {
            var r = self;
            foreach (var p in path)
                r = r.childs.ElementAt(p);
            return r;
        }

        public static int[] GetPath<Tnode>(this Tnode self) where Tnode : ITreeable<Tnode>
        {
            Stack<int> path = new Stack<int>();
            Tnode r = self.root;
            Tnode c = self;
            while (r != null)
            {
                path.Push(r.childs.IndexOf(c));
                c = r;
                r = r.root;
            }
            return path.ToArray();
        }

        public static Tnode GetRoot<Tnode>(this Tnode self) where Tnode : ITreeable<Tnode>
        {
            Tnode cur = self;
            Tnode root = self.root;
            while(root != null)
            {
                cur = root;
                root = root.root;
            }
            return cur;
        }
        public static Tnode GetRoot<Tnode>(this Tnode self, int maxDepth) where Tnode : ITreeable<Tnode>
        {
            Tnode cur = self;
            Tnode root = self.root;
            for(int i = 0; i < maxDepth && root != null; i++)
            {
                cur = root;
                root = root.root;
            }
            return cur;
        }
        public static int[] GetSize<Tnode>(this Tnode self, int depth) where Tnode : ITreeable<Tnode>
        {
            List<int> ws = new List<int>();
            int i = -1;
            foreach (IEnumerable<Tnode> ns in self.ByLevels())
                if (i++ < depth)
                    ws.Add(ns.Count());
            return ws.ToArray();
        }

        public static int GetWidth<Tnode>(this Tnode self, int depth) where Tnode : ITreeable<Tnode>
        {
            int i = -1;
            foreach (IEnumerable<Tnode> ns in self.ByLevels())
                if (i++ == depth)
                    return ns.Count();
            return 0;
        }

        public static bool IsRoot<Tnode>(this Tnode self, Tnode node) where Tnode : ITreeable<Tnode>
        {
            Tnode r = self.root;
            while (r != null)
                if (node.Equals(r))
                    return true;
                else
                    r = r.root;
            return false;
        }

        public static void SetRoot<Tnode>(this Tnode self, Tnode node) where Tnode : ITreeable<Tnode>
        {
            self.Release();
            self.root = node;
        }

        public static IEnumerable<IEnumerable<Tnode>> ByLevels<Tnode>(this Tnode self) where Tnode: ITreeable<Tnode>
        {
            return new TreeNumerator<Tnode>(self);
        }
        public static IEnumerable<IEnumerable<Tnode>> ByLevels<Tnode>(this Tnode self, Predicate<Tnode> predicate) where Tnode : ITreeable<Tnode>
        {
            return new TreeNumerator<Tnode>(self, predicate);
        }
        public static IEnumerable<Tnode> ByElements<Tnode>(this Tnode self) where Tnode : ITreeable<Tnode>
        {
            return new TreeNumerator<Tnode>(self);
        }
        public static IEnumerable<Tnode> ByElements<Tnode>(this Tnode self, Predicate<Tnode> predicate) where Tnode : ITreeable<Tnode>
        {
            return new TreeNumerator<Tnode>(self, predicate);
        }
    }

    public class TreeNumerator<Tnode> : IEnumerable<Tnode>, IEnumerable<IEnumerable<Tnode>> where Tnode : ITreeable<Tnode>
    {
        Tnode node;
        Predicate<Tnode> predicate;
        public TreeNumerator(Tnode node, Predicate<Tnode> predicate = null)
        {
            this.node = node;
            this.predicate = predicate;
        }


        public IEnumerator<Tnode> GetEnumerator()
        {
            foreach (IEnumerable<Tnode> ns in (IEnumerable<IEnumerable<Tnode>>)this)
                foreach (var n in ns)
                    yield return n;
        }

        IEnumerator<IEnumerable<Tnode>> IEnumerable<IEnumerable<Tnode>>.GetEnumerator()
        {
            if (predicate == null)
            {
                IEnumerable<Tnode> c = new Tnode[] { node };
                while (c.Any())
                {
                    yield return c;
                    c = c.SelectMany(n => n.childs);
                }
            }
            else if(predicate(node))
            {
                IEnumerable<Tnode> c = new Tnode[] { node };
                while (c.Any())
                {
                    yield return c;
                    c = c.SelectMany(n => n.childs).Where(p => predicate(p));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Tnode>)this).GetEnumerator();
        }
    }

    public class ForestNumerable<Tnode> : IEnumerable<Tnode>, IEnumerable<IEnumerable<Tnode>> where Tnode : ITreeable<Tnode>
    {
        IEnumerable<Tnode> forest;
        public ForestNumerable(IEnumerable<Tnode> forest)
        {
            this.forest = forest;
        }
        public ForestNumerable(IForest<Tnode> forest)
        {
            this.forest = forest.trees;
        }

        public IEnumerator<Tnode> GetEnumerator()
        {
            foreach (IEnumerable<Tnode> ns in (IEnumerable<IEnumerable<Tnode>>)this)
                foreach (var n in ns)
                    yield return n;
        }

        IEnumerator<IEnumerable<Tnode>> IEnumerable<IEnumerable<Tnode>>.GetEnumerator()
        {
            IEnumerable<Tnode> c = forest;
            while (c.Any())
            {
                yield return c;
                c = c.SelectMany(n => n.childs);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (IEnumerable<Tnode> ns in (IEnumerable<IEnumerable<Tnode>>)this)
                foreach (var n in ns)
                    yield return n;
        }
    }
}
