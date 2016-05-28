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
        Task<T> Read<T>(string name, T otherwise);
        Task Write<T>(string name, T value);
        Task<bool> Exists(string name);
        Task Remove(string name);
    }

    /// <summary>
    /// Interface for storing settings.
    /// </summary>
    public interface ISettingsStorage
    {
        Task<T> Read<T>(string name, T otherwise);
        Task Write<T>(string name, T value);
        Task<bool> Exists(string name);
        Task Remove(string name);
    }
}
