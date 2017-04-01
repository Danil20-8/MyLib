using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DRLib.Structures.Tree;

namespace MyLibTests.Structures.Tree
{
    [TestClass]
    public class TestTreeMaker
    {
        [TestMethod]
        public void TestClear()
        {
            TreeNode<int> bin = new TreeNode<int>(1);
            bin.AddChild(2);
            bin.AddChild(3);
            bin.AddChild(4);

            bin.Clear();

            Assert.AreEqual(0, bin.childs.Count());
        }
        [TestMethod]
        public void TestBuildTree()
        {
            TreeNode<int> intTree = new TreeNode<int>(1);
            intTree.AddChild(2);
            intTree.AddChild(3);
            intTree.AddChild(4);

            var floatTree = intTree.BuildTree(t => new TreeNode<float>(t.item));

            Assert.IsTrue(intTree.ByElements().Zip(floatTree.ByElements(), (i, f) => i.item == (int)f.item).All(b => b));
        }
        [TestMethod]
        public void TestBreakTree()
        {
            TreeNode<int> tree = new TreeNode<int>(1);
            tree.AddChild(2).AddChild(5);
            tree.AddChild(3);
            tree.AddChild(4).AddChild(6).AddChild(7);

            var elements = tree.ByElements().ToArray();

            tree.Break();

            Assert.IsTrue(elements.All(e => e.parent == null && e.childs.Count() == 0));
        }
    }
}
