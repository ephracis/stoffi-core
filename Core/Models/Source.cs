using Microsoft.Practices.Prism.Mvvm;

namespace Stoffi.Core.Models
{
    public class Source : BindableBase
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string ForeignId { get; set; }
        public string Path { get; set; }
    }
}
