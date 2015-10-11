using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stoffi.Core.Interfaces;
using Stoffi.Core.ViewModels.Interface;
using System.Collections.ObjectModel;

namespace Stoffi.Core.Tests.ViewModels.Interface
{
    [TestClass]
    public class ShellViewModelTest
    {
        [TestMethod]
        public void CreatesNavigationItems()
        {
            var vm = new ShellViewModel(null);
            var items = vm.NavItems as ObservableCollection<IPageViewModel>;
            Assert.AreEqual(5, items.Count, "Didn't create exactly five navigation items");
        }
    }
}
