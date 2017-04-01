using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRLib.Serialization;
using DRLib.Structures.Arrays;

namespace DRLib.Structures.Tree
{
    public class Forest<TNode> : IEnumerable<TNode>, IEnumerable<IEnumerable<TNode>>, IHaveChilds<TNode>
        where TNode : IHaveChilds<TNode>
    {
        [Addon]
        List<TNode> _trees;
        public IEnumerable<TNode> trees { get { return _trees; } }

        IEnumerable<TNode> IHaveChilds<TNode>.childs { get { return _trees; } }

        public Forest(IEnumerable<TNode> trees)
        {
            this._trees = trees.ToList();
        }
        public Forest(TNode tree)
        {
            this._trees = new List<TNode> { tree };
        }
        public Forest()
        {
            _trees = new List<TNode>();
        }
        public void AddTree(TNode tree)
        {
            _trees.Add(tree);
        }
        public bool RemoveTree(TNode tree)
        {
            return _trees.Remove(tree);
        }

        public Forest<TResult> BuildForest<TResult>(Func<TNode, TResult> nodeSelector)
            where TResult : IHaveParent<TResult>, IHaveChilds<TResult>, ITreemaker<TResult>
        {
            return new Forest<TResult>(_trees.Select(t => t.BuildTree(nodeSelector)));
        }
        public IEnumerable<TNode> ByElements()
        {
            return this;
        }
        public IEnumerable<IEnumerable<TNode>> ByLevels()
        {
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TNode>)this).GetEnumerator();
        }
        IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
        {
            foreach (IEnumerable<TNode> ns in (IEnumerable<IEnumerable<TNode>>)this)
                foreach (var n in ns)
                    yield return n;
        }
        IEnumerator<IEnumerable<TNode>> IEnumerable<IEnumerable<TNode>>.GetEnumerator()
        {
            IEnumerable<TNode> c = _trees;
            while (c.Any())
            {
                yield return c;
                c = c.SelectMany(n => n.childs);
            }
        }
    }
}
