using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnixLauncher.Core.Misc;

namespace UnixLauncher.Core.Test
{
    internal class TestRam : IMemoryProvider
    {
        public bool GetRAM(out long TotalMemoryInKilobytes)
        {
            TotalMemoryInKilobytes = -100;

            return true;
        }
    }
}
