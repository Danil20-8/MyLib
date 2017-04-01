using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Structures.Tree
{
    public class TreeNumerator<TNode> : IEnumerable<TNode>, IEnumerable<IEnumerable<TNode>>
        where TNode : IHaveChilds<TNode>
    {
        TNode node;
        Predicate<TNode> predicate;
        public TreeNumerator(TNode node, Predicate<TNode> predicate = null)
        {
            this.node = node;
            this.predicate = predicate;
        }


        public IEnumerator<TNode> GetEnumerator()
        {
            foreach (IEnumerable<TNode> ns in (IEnumerable<IEnumerable<TNode>>)this)
                foreach (var n in ns)
                    yield return n;
        }

        IEnumerator<IEnumerable<TNode>> IEnumerable<IEnumerable<TNode>>.GetEnumerator()
        {
            if (predicate == null)
            {
                IEnumerable<TNode> c = new TNode[] { node };
                while (c.Any())
                {
                    yield return c;
                    c = c.SelectMany(n => n.childs);
                }
            }
            else if (predicate(node))
            {
                IEnumerable<TNode> c = new TNode[] { node };
                while (c.Any())
                {
                    yield return c;
                    c = c.SelectMany(n => n.childs).Where(p => predicate(p));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TNode>)this).GetEnumerator();
        }
    }
}
