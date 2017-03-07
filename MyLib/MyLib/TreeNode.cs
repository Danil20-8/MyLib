using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using DRLib.Serialization;
using DRLib.Modern;

namespace DRLib
{
    public class TreeNode<T> : IEnumerable<TreeNode<T>>, IEnumerable<IEnumerable<TreeNode<T>>>, ITreeable<TreeNode<T>>
    {
        [Addon]
        [NoSerializeBinder]
        protected List<TreeNode<T>> nodes = new List<TreeNode<T>>();
        protected TreeNode<T> root;
        [ConstructorArg(0)]
        public T item;

        TreeNode<T> ITreeable<TreeNode<T>>.root{ get { return root; }set { root = value; } }

        public IEnumerable<TreeNode<T>> childs{ get { return nodes; } }

        public TreeNode(T item)
        {
            this.item = item;
        }
        [PostDeserialize]
        void PostDeserialize()
        {
            foreach (var n in nodes)
                n.SetRoot(this);
        }

        public void AddNode(T item, params int[] path)
        {
            AddNode(new TreeNode<T>(item), path);
        }

        public void AddNode(TreeNode<T> node, params int[] path)
        {
            TreeNode<T> r = path.Length > 0 ? GetNode(path) : this;
            r.nodes.Add(node);
            node.SetRoot(r);
        }

        public Tnode BuildTree<Tnode>(Action<Tnode, Tnode> builder, Func<TreeNode<T>, Tnode> selector)
        {
            Tnode root = selector(this);
            IEnumerable<Tuple<TreeNode<T>, Tnode>> pre = new Tuple<TreeNode<T>, Tnode>[] { new Tuple<TreeNode<T>, Tnode>(this, root) };
            List<Tuple<TreeNode<T>, Tnode>> curr;
            foreach (var n in this.ByLevels().Skip(1))
            {
                curr = new List<Tuple<TreeNode<T>, Tnode>>();
                foreach (var p in pre)
                    foreach (var c in p.Item1.nodes)
                    {
                        Tnode t = selector(c);
                        builder(p.Item2, t);
                        curr.Add(new Tuple<TreeNode<T>, Tnode>(c, t));
                    }
                pre = curr;
            }
            return root;
        }
        public Tnode BuildTree<Tnode, Titem>(Func<T, Titem> itemSelector, Func<Titem, Tnode> selector) where Tnode : TreeNode<Titem>
        {
            Tnode root = selector(itemSelector(this.item));
            IEnumerable<Tuple<TreeNode<T>, Tnode>> pre = new Tuple<TreeNode<T>, Tnode>[] { new Tuple<TreeNode<T>, Tnode>(this, root) };
            List<Tuple<TreeNode<T>, Tnode>> curr;
            foreach (var n in this.ByLevels().Skip(1))
            {
                curr = new List<Tuple<TreeNode<T>, Tnode>>();
                foreach (var p in pre)
                    foreach (var c in p.Item1.nodes)
                    {
                        Tnode t = selector(itemSelector(c.item));
                        p.Item2.AddNode(t);
                        curr.Add(new Tuple<TreeNode<T>, Tnode>(c, t));
                    }
                pre = curr;
            }
            return root;
        }
        public static TreeNode<T> BuildCustomTree<Tcust>(Tcust dendritic, Func<Tcust, T> selector, Func<Tcust, IEnumerable<Tcust>> getChilds)
        {
            TreeNode<T> root = new TreeNode<T>(selector(dendritic));
            IEnumerable<Tuple<Tcust, TreeNode<T>>> pre = new Tuple<Tcust, TreeNode<T>>[] { new Tuple<Tcust, TreeNode<T>>(dendritic, root) };
            List<Tuple<Tcust, TreeNode<T>>> curr;
            foreach (var n in getChilds(dendritic))
            {
                curr = new List<Tuple<Tcust, TreeNode<T>>>();
                foreach (var p in pre)
                    foreach (var c in getChilds(p.Item1))
                    {
                        TreeNode<T> t = new TreeNode<T>(selector(c));
                        p.Item2.AddNode(t);
                        curr.Add(new Tuple<Tcust, TreeNode<T>>(c, t));
                    }
                pre = curr;
            }
            return root;
        }
        public void Clear()
        {
            foreach (var n in nodes)
                n.Clear();
            nodes.Clear();
        }

        public void Release()
        {
            if (root != null)
                root.nodes.Remove(this);
        }

        public TreeNode<T> GetNode(params int[] path)
        {
            var r = this;
            foreach (var p in path)
                r = r.nodes[p];
            return r;
        }
        public TreeNode<T> GetNode(T item)
        {
            foreach (var n in this)
                if (n.item.Equals(item))
                    return n;
            return null;
        }
        public int[] GetPath()
        {
            Stack<int> path = new Stack<int>();
            TreeNode<T> r = root;
            TreeNode<T> c = this;
            while (r != null)
            {
                path.Push(r.nodes.IndexOf(c));
                c = r;
                r = r.root;
            }
            return path.ToArray();
        }

        public int[] GetSize(int depth)
        {
            List<int> ws = new List<int>();
            int i = -1;
            foreach (IEnumerable<TreeNode<T>> ns in this)
                if (i++ < depth)
                    ws.Add(ns.Count());
            return ws.ToArray();
        }

        public int GetWidth(int depth)
        {
            int i = -1;
            foreach (IEnumerable<TreeNode<T>> ns in this)
                if (i++ == depth)
                    return ns.Count();
            return 0;
        }

        public bool IsRoot(TreeNode<T> node)
        {
            TreeNode<T> r = this.root;
            while (r != null)
                if (node == r)
                    return true;
                else
                    r = r.root;
            return false;
        }
        public bool IsRoot(T item)
        {
            TreeNode<T> r = this.root;
            while (r != null)
                if (item.Equals(r.item))
                    return true;
                else
                    r = r.root;
            return false;
        }

        public void SetRoot(TreeNode<T> node)
        {
            root = node;
        }

        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            foreach (IEnumerable<TreeNode<T>> ns in (IEnumerable<IEnumerable<TreeNode<T>>>)this)
                foreach (var n in ns)
                    yield return n;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<TreeNode<T>> c = new TreeNode<T>[] { this };
            while (c.Any())
            {
                yield return c;
                c = c.SelectMany(n => n.nodes);
            }
        }
        IEnumerator<IEnumerable<TreeNode<T>>> IEnumerable<IEnumerable<TreeNode<T>>>.GetEnumerator()
        {
            IEnumerable<TreeNode<T>> c = new TreeNode<T>[] { this };
            while (c.Any())
            {
                yield return c;
                c = c.SelectMany(n => n.nodes);
            }
        }
        public IEnumerable<IEnumerable<TreeNode<T>>> ByLevels()
        {
            return this;
        }
        public IEnumerable<TreeNode<T>> ByElements()
        {
            return this;
        }
        public override string ToString()
        {
            string s = item.ToString() + '[';
            foreach (var n in nodes)
                s += n.ToString();
            s += ']';
            return s;
        }
        public string ToString(Func<T, string> itemToString)
        {
            string s = itemToString(item) + '[';
            foreach (var n in nodes)
                s += n.ToString(itemToString);
            s += ']';
            return s;
        }
        public static TreeNode<T> Parse(string source)
        {
            var parseMethod = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });

            Stack<TreeNode<T>> ins = new Stack<TreeNode<T>>();
            TreeNode<T> root = null;

            int p = 0;
            int n = 0;

            while (p < source.Length)
            {
                if (source[p] == '[')
                {
                    var node = new TreeNode<T>((T)parseMethod.Invoke(null, new object[] { (source.Substring(n, p - n)) }));
                    if (root != null)
                        root.AddNode(node);
                    root = node;
                    ins.Push(node);
                    n = p + 1;
                }
                if (source[p] == ']')
                {
                    n = p + 1;
                    root = ins.Pop();
                }
                p++;
            }

            return root;
        }

        public void AddChild(TreeNode<T> child)
        {
            nodes.Add(child);
        }

        public void RemoveChild(TreeNode<T> child)
        {
            nodes.Remove(child);
        }
    }
}
