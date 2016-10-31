using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyLib.Modern;
using MyLib.Algoriphms;
using MyLib.Serialization;
namespace MyLib
{
    public class CustomTree<Tsource, Tnode> : ITreeable<CustomTree<Tsource, Tnode>>
    {
        public readonly Tsource source;
        public Tnode node { get { return selector(source); } }
        Func<Tnode, IEnumerable<Tsource>> getChilds;
        Func<Tsource, Tnode> selector;
        Func<Tnode, Tsource> getRoot;
        CustomTree<Tsource, Tnode> ITreeable<CustomTree<Tsource, Tnode>>.root
        {
            get
            {
                return new CustomTree<Tsource, Tnode>(getRoot(node), selector, getChilds, getRoot);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<CustomTree<Tsource, Tnode>> childs
        {
            get
            {
                return getChilds(selector(source)).Select(n => new CustomTree<Tsource, Tnode> (n, selector, getChilds, getRoot));
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public CustomTree(Tsource source, Func<Tsource, Tnode> selector, Func<Tnode, IEnumerable<Tsource>> getChilds = null, Func<Tnode, Tsource> getRoot = null)
        {

            this.source = source;
            this.selector = selector;
            this.getChilds = getChilds;
            this.getRoot = getRoot;
        }
        public void AddChild(CustomTree<Tsource, Tnode> child)
        {
            throw new NotImplementedException();
        }

        public void RemoveChild(CustomTree<Tsource, Tnode> child)
        {
            throw new NotImplementedException();
        }
    }

    public class Forest<T> : IEnumerable<T>, IEnumerable<IEnumerable<T>> where T : ITreeable<T>
    {
        [Addon]
        List<T> _trees;
        public IEnumerable<T> trees{ get { return _trees; } }

        public Forest(IEnumerable<T> trees)
        {
            this._trees = trees.ToList();
        }
        public Forest(T tree)
        {
            this._trees = new List<T> { tree };
        }
        public Forest()
        {
            _trees = new List<T>();
        }
        public void AddTree(T tree)
        {
            _trees.Add(tree);
        }
        public bool RemoveTree(T tree)
        {
            return _trees.Remove(tree);
        }
        public T GetNode(params int[] path)
        {
            var r = _trees[path[0]];
            foreach (var p in path.Skip(1))
                r = r.childs.ElementAt(p);
            return r;
        }

        public int[] GetPath(T node)
        {
            Stack<int> path = new Stack<int>();
            T r = node.root;
            T c = node;
            while (r != null)
            {
                path.Push(r.childs.IndexOf(c));
                c = r;
                r = r.root;
            }
            path.Push(_trees.IndexOf(c));
            return path.ToArray();
        }
        public Forest<Tresult> BuildForest<Tresult>(Func<T, Tresult> nodeSelector) where Tresult : ITreeable<Tresult>
        {
            return new Forest<Tresult>(_trees.Select(t => t.BuildTree(nodeSelector)));
        }
        public IEnumerable<T> ByElements()
        {
            return this;
        }
        public IEnumerable<IEnumerable<T>> ByLevels()
        {
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (IEnumerable<T> ns in (IEnumerable<IEnumerable<T>>)this)
                foreach (var n in ns)
                    yield return n;
        }
        IEnumerator<IEnumerable<T>> IEnumerable<IEnumerable<T>>.GetEnumerator()
        {
            IEnumerable<T> c = _trees;
            while (c.Any())
            {
                yield return c;
                c = c.SelectMany(n => n.childs);
            }
        }
    }
}
