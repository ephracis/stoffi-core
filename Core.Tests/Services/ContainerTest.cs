using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stoffi.Core.Services;

namespace Stoffi.Core.Tests.Services
{
    [TestClass]
    public class ContainerTest
    {
        [TestMethod]
        public void Instance_ReturnsSingleton()
        {
            var a = Container.Instance;
            var b = Container.Instance;
            Assert.AreSame(a, b);
        }
    }
}
