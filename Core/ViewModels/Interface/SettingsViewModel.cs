using Stoffi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.ViewModels.Interface
{
    public class SettingsViewModel : ViewModel, IPageViewModel
    {
        /// <summary>
        /// Gets the name of this page.
        /// </summary>
        public string Name { get { return "settings"; } }

        private string content = "Settings";
        public string Content
        {
            get { return content; }
            set { SetProperty<string>(ref content, value, nameof(Content)); }
        }
    }
}
