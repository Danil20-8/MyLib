using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Structures.Tree
{
    public interface IHaveParent<TNode> where TNode : IHaveParent<TNode>
    {
        TNode parent { get; }
    }
    public interface IHaveChilds<TNode> where TNode : IHaveChilds<TNode>
    {
        IEnumerable<TNode> childs { get; }
    }

    public interface ITreemaker<TNode>
        where TNode : ITreemaker<TNode>
    {
        void AddChild(TNode child);
        bool RemoveChild(TNode child);
    }
}
