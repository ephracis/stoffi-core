using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Interfaces
{
    public interface ISettingsService
    {
        object GetValue(string name);

        void SetValue(string name, object value);
    }
}
