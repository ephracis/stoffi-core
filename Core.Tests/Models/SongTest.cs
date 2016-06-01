using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stoffi.Core.Common;
using Stoffi.Core.Models;

namespace Stoffi.Core.Tests.Common
{
    [TestClass]
    public class SongTest
    {
        [TestMethod]
        public void PathId_YouTube_ReturnsId()
        {
            var song = new Song();
            song.Path = "stoffi://song/youtube/Test123";
            Assert.AreEqual("Test123", song.GetPathId());
        }
    }
}
