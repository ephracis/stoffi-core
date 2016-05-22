using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stoffi.Core.Common;

namespace Stoffi.Core.Tests.Common
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void Capitalize_LowercaseString()
        {
            Assert.AreEqual("Foo", StringHelper.Capitalize("foo"));
        }

        [TestMethod]
        public void Capitalize_UppercaseString()
        {
            Assert.AreEqual("Foo", StringHelper.Capitalize("FOO"));
        }

        [TestMethod]
        public void Capitalize_EmptyString()
        {
            Assert.AreEqual("", StringHelper.Capitalize(""));
        }

        [TestMethod]
        public void Capitalize_Null_ReturnsEmpty()
        {
            Assert.AreEqual("", StringHelper.Capitalize(null));
        }

        [TestMethod]
        public void Titleize_TwoWords()
        {
            Assert.AreEqual("Lorem Ipsum", StringHelper.Titleize("lorem ipsum"));
        }

        [TestMethod]
        public void Titleize_WithShortWord()
        {
            Assert.AreEqual("Lorem with Ipsum", StringHelper.Titleize("LorEM WItH ipSUM"));
        }

        [TestMethod]
        public void Titleize_EmptyString()
        {
            Assert.AreEqual("", StringHelper.Titleize(""));
        }

        [TestMethod]
        public void Titleize_Null_ReturnsEmpty()
        {
            Assert.AreEqual("", StringHelper.Titleize(null));
        }

        [TestMethod]
        public void Classify_TwoWords()
        {
            Assert.AreEqual("LoremIpsum", StringHelper.Classify("loREM iPsum"));
        }

        [TestMethod]
        public void Classify_EmptyString()
        {
            Assert.AreEqual("", StringHelper.Classify(""));
        }

        [TestMethod]
        public void Classify_Null_ReturnsEmpty()
        {
            Assert.AreEqual("", StringHelper.Classify(null));
        }
    }
}
