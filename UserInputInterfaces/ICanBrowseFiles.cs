using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInputInterfaces
{
    public interface ICanBrowseFiles
    {
        public String Browse();
        public String BrowseSaveFile(string defaultFileName, string defaultExtension, string filter);
    }
}
