using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stoffi.Core.Tests.Mocks;
using Stoffi.Core.Services;

namespace Stoffi.Core.Tests.Services
{
    [TestClass]
    public class SettingsServiceTest
    {
        [TestMethod]
        public async void Read_Existing_ReturnsCastedObject()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            storage.Settings["test"] = 42;
            Assert.AreEqual(42, await service.Read<int>("test", 0));
        }

        [TestMethod]
        public async void Read_NonExisting_ReturnsOtherwise()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            Assert.AreEqual(1337, await service.Read<int>("test", 1337));
        }

        [TestMethod]
        public async void Read_WrongType_ThrowsException()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            storage.Settings["test"] = "sample string";
            try
            {
                await service.Read<int>("test", 0);
                Assert.Fail("No exception was thrown");
            }
            catch (InvalidCastException ex)
            {
                Assert.AreEqual("Specified cast is not valid.", ex.Message);
            }
            catch { Assert.Fail("Wrong exception was thrown"); }
        }

        [TestMethod]
        public async void Write_NonExisting_CreatesSetting()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            await service.Write<int>("test", 42);
            Assert.AreEqual(42, storage.Settings["test"]);
        }

        [TestMethod]
        public async void Write_Existing_OverwritesSetting()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            storage.Settings["test"] = 1337;
            await service.Write<int>("test", 42);
            Assert.AreEqual(42, storage.Settings["test"]);
        }

        [TestMethod]
        public async void Exists_Existing_ReturnsTrue()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            storage.Settings["test"] = 42;
            Assert.IsTrue(await service.Exists("test"));
        }

        [TestMethod]
        public async void Exists_NonExisting_ReturnsFalse()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            Assert.IsFalse(await service.Exists("test"));
        }

        [TestMethod]
        public async void Remove_Existing_RemovesSetting()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            storage.Settings["test"] = 42;
            await service.Remove("test");
            Assert.IsFalse(storage.Settings.ContainsKey("test"));
        }

        [TestMethod]
        public async void Remove_NonExisting_DoesNothing()
        {
            var storage = new MockSettingsStorage();
            var service = new SettingsService(storage);
            await service.Remove("test");
            Assert.IsFalse(storage.Settings.ContainsKey("test"));
        }
    }
}
