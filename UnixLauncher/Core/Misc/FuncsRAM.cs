using System.Runtime.InteropServices;

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
