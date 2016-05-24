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

        public bool Exists(string name)
        {
            return Settings.Keys.Contains<string>(name);
        }

        public T Read<T>(string name, T otherwise)
        {
            return Exists(name) ? (T)Settings[name] : otherwise;
        }

        public void Remove(string name)
        {
            if (Exists(name))
                Settings.Remove(name);
        }

        public void Write<T>(string name, T value)
        {
            Settings[name] = value as object;
        }
    }
}
