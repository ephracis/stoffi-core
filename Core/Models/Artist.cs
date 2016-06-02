using Microsoft.Practices.Prism.Mvvm;

namespace Stoffi.Core.Models
{
    public class Artist : BindableBase
    {
        public int Id { get; set; }
        public string Url { get; set; }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }
    }
}
