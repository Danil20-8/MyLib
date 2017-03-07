using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DRLib;
using System.Linq;
namespace MyLibTests
{
    [TestClass]
    public class TestTrees
    {
        [TestMethod]
        public void TestBuild()
        {
            TreeNode<int> t = new TreeNode<int>(10);
            t.AddNode(12);
            t.AddNode(14);

            Assert.AreEqual(3, t.ByElements().Count(), "first");
            Assert.AreEqual(3, t.BuildTree(n => new TreeNode<string>(n.item.ToString())).ByElements().Count(), "second");
        }
        [TestMethod]
        public void TestBuildTreeOfTree()
        {
            TreeNode<int> t = new TreeNode<int>(10);
            t.AddNode(12);
            t.AddNode(14);

            Assert.AreEqual(3, t.ByElements().Count(), "first");
            Assert.AreEqual(3, t.BuildTree(n => new TreeNode<object>(n)).ByElements().Count(), "second");
            Assert.AreEqual(3, t.ByElements().Count(), "first");
        }
    }
}
