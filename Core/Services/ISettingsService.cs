using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Services
{
    /// <summary>
    /// Interface for managing settings.
    /// </summary>
    /// <remarks>
    /// The SERVICE is just a wrapper around the platform specific
    /// ISettingsStorage, allowing us to expose the settings to
    /// the platform agnostic Core code.
    /// </remarks>
    public interface ISettingsService
    {
        T Read<T>(string name, T otherwise);
        void Write<T>(string name, T value);
        bool Exists(string name);
        void Remove(string name);
    }

    /// <summary>
    /// Interface for storing settings.
    /// </summary>
    public interface ISettingsStorage
    {
        T Read<T>(string name, T otherwise);
        void Write<T>(string name, T value);
        bool Exists(string name);
        void Remove(string name);
    }
}
