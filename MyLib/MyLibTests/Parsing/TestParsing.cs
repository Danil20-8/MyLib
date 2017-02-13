using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLib.Parsing.SimpleParser;
using MyLib.Parsing.XML;
namespace MyLibTests.Parsing
{
    [TestClass]
    public class TestParsing
    {
        [TestMethod]
        public void TestCSSParser()
        {
            string source = "class{ name = Vitalya; age = \"19\"; }";
            CSSParser parser = new CSSParser();
            parser.Parse(source);
            var result = parser.cssResult;

            Assert.AreEqual("class", result.First().Key);

            Assert.AreEqual("name", result.Values.First().First().Key);
            Assert.AreEqual("Vitalya", result.Values.First().First().Value);

            Assert.AreEqual("age", result.Values.First().Last().Key);
            Assert.AreEqual("19", result.Values.First().Last().Value);
        }

        [TestMethod]
        public void TestXMLParser()
        {
            string source =
                @"<a name=xml>
                    <b name= nested\>
                    <c\>
                    <d>
                        <e\>
                    <\d>
                <\a>";
            XMLParser parser = new XMLParser();
            parser.Parse(source);
            var result = parser.xmlResult;

            var element = result.trees.First();
            var nested = element.childs.First();

            Assert.AreEqual("a", element.item.name);
            Assert.AreEqual("name", element.item.attributes.Keys.First());
            Assert.AreEqual("xml", element.item.attributes.Values.First());

            Assert.AreEqual("b", nested.item.name);
            Assert.AreEqual("name", nested.item.attributes.Keys.First());
            Assert.AreEqual("nested", nested.item.attributes.Values.First());
        }
    }

}
