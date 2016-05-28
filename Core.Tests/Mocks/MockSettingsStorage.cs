using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stoffi.Core.Services;

namespace Stoffi.Core.Tests.Mocks
{
    public class MockSettingsStorage : ISettingsStorage
    {
        public Dictionary<string, object> Settings = new Dictionary<string, object>();

        public async Task<bool> Exists(string name)
        {
            await Task.CompletedTask;
            return Settings.Keys.Contains<string>(name);
        }

        public async Task<T> Read<T>(string name, T otherwise)
        {
            return await Exists(name) ? (T)Settings[name] : otherwise;
        }

        public async Task Remove(string name)
        {
            if (await Exists(name))
                Settings.Remove(name);
            await Task.CompletedTask;
        }

        public async Task Write<T>(string name, T value)
        {
            Settings[name] = value as object;
            await Task.CompletedTask;
        }
    }
}
