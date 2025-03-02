namespace UnixLauncher.Core.Misc
{
    internal interface IMemoryProvider
    {
        bool GetRAM(out long totalMemoryInKilobytes);
    }
}
