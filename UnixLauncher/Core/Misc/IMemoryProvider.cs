namespace UnixLauncher.Core.Misc
{
    // TODO:
    // - Сделать проверку, можно ли взять оперативку с заданным числом
    // типо, доступна ли такая память
    //
    // - Удалить лишний коммент в MCStartLineBuilder.cs после выполнения.
    internal interface IMemoryProvider
    {
        bool GetRAM(out long totalMemoryInKilobytes);
    }
}
