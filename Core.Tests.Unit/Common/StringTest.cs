using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Stoffi.Core.Common.Tests
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void CapitalizeLowercaseString()
        {
            Assert.AreEqual("Foo", StringHelper.Capitalize("foo"));
        }

        [TestMethod]
        public void CapitalizeUppercaseString()
        {
            Assert.AreEqual("Foo", StringHelper.Capitalize("FOO"));
        }

        [TestMethod]
        public void CapitalizeEmptyString()
        {
            Assert.AreEqual("", StringHelper.Capitalize(""));
        }

        [TestMethod]
        public void CapitalizeNull()
        {
            Assert.AreEqual("", StringHelper.Capitalize(null));
        }

        [TestMethod]
        public void TitleizeTwoWords()
        {
            Assert.AreEqual("Lorem Ipsum", StringHelper.Titleize("lorem ipsum"));
        }

        [TestMethod]
        public void TitleizeWithShortWord()
        {
            Assert.AreEqual("Lorem with Ipsum", StringHelper.Titleize("LorEM WItH ipSUM"));
        }

        [TestMethod]
        public void TitleizeEmptyString()
        {
            Assert.AreEqual("", StringHelper.Titleize(""));
        }

        [TestMethod]
        public void TitleizeNull()
        {
            Assert.AreEqual("", StringHelper.Titleize(null));
        }

        [TestMethod]
        public void ClassifyTwoWords()
        {
            Assert.AreEqual("LoremIpsum", StringHelper.Classify("loREM iPsum"));
        }

        [TestMethod]
        public void ClassifyEmptyString()
        {
            Assert.AreEqual("", StringHelper.Classify(""));
        }

        [TestMethod]
        public void ClassifyNull()
        {
            Assert.AreEqual("", StringHelper.Classify(null));
        }
    }
}
