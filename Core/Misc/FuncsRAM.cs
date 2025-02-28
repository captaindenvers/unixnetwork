using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Misc
{
    internal partial class FuncsRAM : IMemoryProvider
    {
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        public bool GetRAM(out long TotalMemoryInKilobytes)
        {
            bool result;

            result = GetPhysicallyInstalledSystemMemory(out TotalMemoryInKilobytes);

            return result;
        }
    }
}
