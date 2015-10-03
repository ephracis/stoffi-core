using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stoffi.Core.Interfaces;

namespace Stoffi.Core.ViewModels.Interface
{
    public class DashboardViewModel : ViewModel, IPageViewModel
    {
        /// <summary>
        /// Gets the name of this page.
        /// </summary>
        public string Name { get { return "dashboard"; } }

        /// <summary>
        /// Gets the name of the symbol to use for representing this page.
        /// </summary>
        public string Symbol { get { return "home"; } }

        private string content = "Dashboard";
        public string Content
        {
            get { return content; }
            set { SetProperty<string>(ref content, value, nameof(Content)); }
        }
    }
}
