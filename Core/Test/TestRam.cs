using UnixLauncher.Core.Misc;

namespace UnixLauncher.Core.Test
{
    // TODO:
    // - сделать норм классы для проверки, а не эту залупу
    internal class TestRam : IMemoryProvider
    {
        public bool GetRAM(out long TotalMemoryInKilobytes)
        {
            TotalMemoryInKilobytes = -100;

            return true;
        }
    }
}
