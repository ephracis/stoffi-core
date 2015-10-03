using Stoffi.Core.Interfaces;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Stoffi.Core.ViewModels.Interface
{
    /// <summary>
    /// The ViewModel for the main page view.
    /// </summary>
    public class ShellViewModel : ViewModel
    {
        #region Properties
        /// <summary>
        /// The navigation bar's currently selected navigation item.
        /// </summary>
        private object selectedItem;

        /// <summary>
        /// Gets or sets the currently selected navigation item.
        /// </summary>
        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                SetProperty(ref selectedItem, value, nameof(SelectedItem));
                SaveSelectedNavigation();
            }
        }

        private ObservableCollection<IPageViewModel> navItems = new ObservableCollection<IPageViewModel>();

        /// <summary>
        /// Gets or sets a list of all navigation items.
        /// </summary>
        public object NavItems
        {
            get { return navItems as object; }
            set { navItems = value as ObservableCollection<IPageViewModel>; } 
        }
        #endregion

        /// <summary>
        /// Creates a new instance of the ViewModel for the main page view.
        /// </summary>
        public ShellViewModel()
        {
            InitializeNavItems();
            LoadSelectedNavigation();
        }

        #region Methods

        /// <summary>
        /// Create the navigation items.
        /// </summary>
        private void InitializeNavItems()
        {
            navItems.Add(new DashboardViewModel());
            navItems.Add(new PlaylistsViewModel());
            navItems.Add(new MusicViewModel());
            navItems.Add(new TimelineViewModel());
            navItems.Add(new SettingsViewModel());
        }

        /// <summary>
        /// Loads the selected navigation item from cache, or sets to
        /// default value if no cache was found.
        /// </summary>
        private void LoadSelectedNavigation()
        {
            if (navItems.Count == 0)
                return; // nothing to do...

            var i = 0;

            // check for a value in settings
            var settings = ApplicationData.Current.RoamingSettings;
            if (settings.Values.ContainsKey("SelectedNavigationIndex"))
            {
                try { i = (int)settings.Values["SelectedNavigationIndex"]; }
                catch { }
            }

            // ensure the index is valid
            if (i >= navItems.Count || i < 0)
                i = 0;

            selectedItem = navItems[i];
        }

        /// <summary>
        /// Save the currently selected navigation to cache.
        /// </summary>
        private void SaveSelectedNavigation()
        {
            if (!navItems.Contains(selectedItem as IPageViewModel))
                return;
            var i = navItems.IndexOf(selectedItem as IPageViewModel);
            ApplicationData.Current.RoamingSettings.Values["SelectedNavigationIndex"] = i;
        }

        #endregion


    }
}
