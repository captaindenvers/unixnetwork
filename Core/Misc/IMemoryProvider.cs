using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Misc
{
    internal interface IMemoryProvider
    {
        bool GetRAM(out long TotalMemoryInKilobytes);
    }
}
