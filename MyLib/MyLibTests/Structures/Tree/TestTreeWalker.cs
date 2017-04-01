using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DRLib.Structures.Tree;

namespace MyLibTests.Structures.Tree
{
    [TestClass]
    public class TestTreeWalker
    {
        [TestMethod]
        public void TestAddChild()
        {
            TreeNode<int> parent = new TreeNode<int>(1);
            var child = parent.AddChild(2);

            Assert.AreEqual(parent, child.parent);
            Assert.AreEqual(parent.childs.First(), child);
        }
        [TestMethod]
        public void TestAddChildToChild()
        {
            TreeNode<int> parent = new TreeNode<int>(1);
            var child = parent.AddChild(2);
            var childOfChild = new TreeNode<int>(3);
            parent.AddChild(childOfChild, 0);

            Assert.AreEqual(child, childOfChild.parent);
        }
        [TestMethod]
        public void TestRemoveChild()
        {
            TreeNode<int> parent = new TreeNode<int>(1);
            var child = parent.AddChild(2);
            parent.RemoveChild(child);

            Assert.AreEqual(0, parent.childs.Count());
        }
        [TestMethod]
        public void TestRemoveChildOfChild()
        {
            TreeNode<int> parent = new TreeNode<int>(1);
            var child = parent.AddChild(2);
            var childOfChild = new TreeNode<int>(3);
            parent.AddChild(childOfChild, 0);
            parent.RemoveChild(childOfChild, 0);
            Assert.AreEqual(0, child.childs.Count());
        }
        [TestMethod]
        public void TestGetRoot()
        {
            TreeNode<int> parent = new TreeNode<int>(1);
            var child = parent.AddChild(2);
            var childOfChild = new TreeNode<int>(3);
            parent.AddChild(childOfChild, 0);

            Assert.AreEqual(parent, childOfChild.GetRoot());
        }
        [TestMethod]
        public void TestIsParent()
        {
            TreeNode<int> parent = new TreeNode<int>(1);
            var child = parent.AddChild(2);
            var childOfChild = new TreeNode<int>(3);
            parent.AddChild(childOfChild, 0);

            var notParent = child.AddChild(4);

            Assert.IsTrue(childOfChild.IsParent(parent));
            Assert.IsFalse(childOfChild.IsParent(notParent));
        }

        [TestMethod]
        public void TestGetPath()
        {
            TreeNode<int> parent = new TreeNode<int>(1);
            var child = parent.AddChild(2);
            var childOfChild = new TreeNode<int>(3);
            parent.AddChild(childOfChild, 0);

            var secondChild = child.AddChild(4);

            Assert.IsTrue(new int[] { 0, 1 }.Zip(secondChild.GetPath(), (l, r) => l == r).All(b => b));
        }
        [TestMethod]
        public void TestByElements()
        {
            TreeNode<int> one = new TreeNode<int>(1);
            var two = one.AddChild(2);
            var three = new TreeNode<int>(3);
            one.AddChild(three, 0);

            var four = two.AddChild(4);

            Assert.AreEqual(4, one.ByElements().Count());
        }
        [TestMethod]
        public void TestByLevels()
        {
            TreeNode<int> first0 = new TreeNode<int>(1);
            var second0 = first0.AddChild(2);
            var third0 = new TreeNode<int>(3);
            first0.AddChild(third0, 0);

            var third1 = second0.AddChild(4);

            Assert.AreEqual(3, first0.ByLevels().Count());
        }
    }
}
