using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DRLib.Parsing.SimpleParser;
using DRLib.Parsing.XML;
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
                    <!-- nested to a elements <A ... -->
                    <b name= nested/>
                    <c/>
                    <d>
                        <e> Value </e>
                    </d>
                </a>";
            XMLParser parser = new XMLParser();
            
            var result = parser.Parse(source);

            var element = result.childs.First();
            var nested = element.childs.First();

            Assert.AreEqual("a", element.name);
            Assert.AreEqual("name", element.attributes.Keys.First());
            Assert.AreEqual("xml", element.attributes.Values.First());

            Assert.AreEqual("b", nested.name);
            Assert.AreEqual("name", nested.attributes.Keys.First());
            Assert.AreEqual("nested", nested.attributes.Values.First());

            var dElement = element.childs.Find(c => c.name == "d");
            var eElement = dElement.childs.First();
            Assert.AreEqual(" Value ", eElement.value);
        }
    }

}
