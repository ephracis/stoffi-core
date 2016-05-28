using Microsoft.Practices.Unity;

namespace Stoffi.Core.Services
{
    /// <summary>
    /// Used for locating an instance of the IoC container.
    /// </summary>
    public class Container
    {
        public static IUnityContainer Instance { get; private set; }
        static Container()
        {
            Instance = Instance ?? new UnityContainer();
        }
    }
}
