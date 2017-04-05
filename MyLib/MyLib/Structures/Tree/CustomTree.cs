using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Structures.Tree
{
    public struct CustomTree<T> : IHaveChilds<CustomTree<T>>
    {
        public delegate IEnumerable<T> ChildsGetter(T root);

        T tree;
        ChildsGetter childsOf;

        IEnumerable<CustomTree<T>> IHaveChilds<CustomTree<T>>.childs
        {
            get { return new ChildSet(this); }
        }

        public CustomTree(T tree, ChildsGetter childs)
        {
            this.tree = tree;
            this.childsOf = childs;
        }

        struct ChildSet : IEnumerable<CustomTree<T>>
        {
            CustomTree<T> root;

            public ChildSet(CustomTree<T> root)
            {
                this.root = root;
            }

            public IEnumerator<CustomTree<T>> GetEnumerator()
            {
                foreach (var c in root.childsOf(root.tree))
                    yield return new CustomTree<T>(c, root.childsOf);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
