using Stoffi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.ViewModels.Interface
{
    public class MusicViewModel : ViewModel, IPageViewModel
    {
        /// <summary>
        /// Gets the name of this page.
        /// </summary>
        public string Name { get { return "music"; } }

        /// <summary>
        /// Gets the name of the symbol to use for representing this page.
        /// </summary>
        public string Symbol { get { return "audio"; } }

        private string content = "Music";
        public string Content
        {
            get { return content; }
            set { SetProperty<string>(ref content, value, nameof(Content)); }
        }
    }
}
