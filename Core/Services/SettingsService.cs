using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Services
{
    /// <summary>
    /// Service for managing application settings.
    /// 
    /// It wraps around a platform specific settings storage driver,
    /// thus exposing app settings to the platform agnostic Core code.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        /// <summary>
        /// An driver for storing the settings.
        /// 
        /// This code is platform specific and thus abstracted away from
        /// the service itself
        /// </summary>
        private ISettingsStorage storage;

        public SettingsService(ISettingsStorage storage)
        {
            this.storage = storage;
        }

        /// <summary>
        /// Check if a given setting exists.
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <returns>True if the setting exists, otherwise false</returns>
        public bool Exists(string name)
        {
            return storage.Exists(name);
        }

        /// <summary>
        /// Read the value of a given setting.
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="name">The name of the setting</param>
        /// <param name="otherwise">The default value to return if the setting is not found</param>
        /// <returns>The setting if found, `otherwise` if not</returns>
        public T Read<T>(string name, T otherwise)
        {
            return storage.Read<T>(name, otherwise);
        }

        /// <summary>
        /// Remove a given setting.
        /// </summary>
        /// <param name="name">The name of the setting</param>
        public void Remove(string name)
        {
            storage.Remove(name);
        }

        /// <summary>
        /// Save a setting.
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="name">The name of the setting</param>
        /// <param name="value">The value of the setting</param>
        public void Write<T>(string name, T value)
        {
            storage.Write<T>(name, value);
        }
    }
}
