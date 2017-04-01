using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRLib.Serialization;

namespace DRLib.Structures.Tree
{
    public class TreeNode<TItem> :
        IHaveParent<TreeNode<TItem>>,
        IHaveChilds<TreeNode<TItem>>,
        ITreemaker<TreeNode<TItem>>
    {
        public TreeNode<TItem> parent { get { return _parent; } }
        TreeNode<TItem> _parent;

        public IEnumerable<TreeNode<TItem>> childs { get { return _childs; } }
        [Addon]
        List<TreeNode<TItem>> _childs = new List<TreeNode<TItem>>();

        public TItem item { get; set; }

        public TreeNode(TItem item)
        {
            this.item = item;
        }

        public TreeNode<TItem> AddChild(TItem item)
        {
            var r = new TreeNode<TItem>(item);
            AddChild(r);
            return r;
        }

        public void AddChild(TreeNode<TItem> child)
        {
            child.Release();
            child._parent = this;
            _childs.Add(child);
        }

        public bool RemoveChild(TreeNode<TItem> child)
        {
            if(_childs.Remove(child))
            {
                child._parent = null;
                return true;
            }
            return false;
        }

        [PostDeserialize]
        void PostDeserialize()
        {
            foreach (var c in _childs)
                c._parent = this;
        }
    }
}
