using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginCore.Core
{
    public interface IModule
    {
        string Name { get; set; }
        string Uri { get; set; }
    }
}
