using UnixLauncher.Core.Misc;

namespace UnixLauncher.Core.Test
{
    internal class NegativeRAM : IMemoryProvider
    {
        public bool GetRAM(out long totalMemoryInKilobytes)
        {
            totalMemoryInKilobytes = -100;

            return true;
        }
    }
}
