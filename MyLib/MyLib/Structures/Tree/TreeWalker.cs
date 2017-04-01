using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRLib.Modern;
using DRLib.Algoriphms;

namespace DRLib.Structures.Tree
{
    public static class TreeWalker
    {
        public static TNode GetNode<TNode>(this TNode self, params int[] path)
            where TNode : IHaveChilds<TNode>
        {
            return GetNode(self, (IEnumerable<int>)path);
        }

        public static TNode GetNode<TNode>(this TNode self, IEnumerable<int> path)
            where TNode : IHaveChilds<TNode>
        {
            var r = self;
            foreach (var p in path)
                r = r.childs.ElementAt(p);
            return r;
        }

        public static Stack<int> GetPath<TNode>(this TNode self)
            where TNode : IHaveParent<TNode>, IHaveChilds<TNode>
        {
            TNode root;
            return GetPath(self, out root);
        }
        public static Stack<int> GetPath<TNode>(this TNode self, out TNode root)
            where TNode : IHaveParent<TNode>, IHaveChilds<TNode>
        {
            Stack<int> path = new Stack<int>();
            TNode parent = self.parent;
            root = self;
            while (parent != null)
            {
                path.Push(parent.childs.IndexOf(root));
                root = parent;
                parent = parent.parent;
            }
            return path;
        }

        public static TNode GetRoot<TNode>(this TNode self)
            where TNode : IHaveParent<TNode>
        {
            TNode cur = self;
            TNode parent = self.parent;
            while (parent != null)
            {
                cur = parent;
                parent = parent.parent;
            }
            return cur;
        }
        public static TNode GetRoot<TNode>(this TNode self, int maxDepth)
            where TNode : IHaveParent<TNode>
        {
            TNode cur = self;
            TNode root = self.parent;
            for (int i = 0; i < maxDepth && root != null; i++)
            {
                cur = root;
                root = root.parent;
            }
            return cur;
        }

        public static int GetWidth<TNode>(this TNode self, int depth)
            where TNode : IHaveChilds<TNode>
        {
            int i = -1;
            foreach (IEnumerable<TNode> ns in self.ByLevels())
                if (i++ == depth)
                    return ns.Count();
            return 0;
        }

        public static int GetDepth<TNode>(this TNode node)
            where TNode : IHaveChilds<TNode>
        {
            return node.ByLevels().Count();
        }

        public static bool IsParent<TNode>(this TNode self, TNode node)
            where TNode : IHaveParent<TNode>
        {
            TNode r = self.parent;
            while (r != null)
                if (node.Equals(r))
                    return true;
                else
                    r = r.parent;
            return false;
        }

        public static IEnumerable<IEnumerable<TNode>> ByLevels<TNode>(this TNode self)
            where TNode : IHaveChilds<TNode>
        {
            return new TreeNumerator<TNode>(self);
        }
        public static IEnumerable<IEnumerable<TNode>> ByLevels<TNode>(this TNode self, Predicate<TNode> predicate)
            where TNode : IHaveChilds<TNode>
        {
            return new TreeNumerator<TNode>(self, predicate);
        }
        public static IEnumerable<TNode> ByElements<TNode>(this TNode self)
            where TNode : IHaveChilds<TNode>
        {
            return new TreeNumerator<TNode>(self);
        }
        public static IEnumerable<TNode> ByElements<TNode>(this TNode self, Predicate<TNode> predicate)
            where TNode : IHaveChilds<TNode>
        {
            return new TreeNumerator<TNode>(self, predicate);
        }
    }
}
