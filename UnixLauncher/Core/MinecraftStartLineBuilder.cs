using System.Text;
using UnixLauncher.Core.Misc;

namespace UnixLauncher.Core
{
    internal class MinecraftStartLineBuilder
    {
        private readonly StringBuilder stringBuilder;
        private readonly IMemoryProvider memoryProvider;

        private const long MIN_RAM = 1024; // MB
        private const int PART_FROM_MAX_RAM = 70; // %
        private readonly long MAX_RAM;

        public MinecraftStartLineBuilder(IMemoryProvider memoryProvider)
        {
            // --- DI
            this.memoryProvider = memoryProvider;

            // Get RAM
            memoryProvider.GetRAM(out long kbRAM);

            if (kbRAM < 1)
                throw new ArgumentException("Core error: IMemoryProvider gave out too " +
                    "little RAM (How does it even work if we have <1KB of RAM?)");

            // --- Calculating part of max RAM
            long mbRAM = kbRAM / 1024;
            MAX_RAM = (mbRAM * PART_FROM_MAX_RAM) / 100;

            stringBuilder = new();
            stringBuilder.Append("java ");
        }

        public MinecraftStartLineBuilder SetXmx(int megabytes)
        {
            CheckRAM(megabytes);

            // Пример, как оно выглядело бы без этой функции
            // Если будут странные приведения у stringBuilder, заменим на это
            //stringBuilder.Append("-Xmx");
            //stringBuilder.Append(megabytes);
            //stringBuilder.Append("m ");
            AddArgument("-Xmx", megabytes, "m ");

            return this;
        }

        public MinecraftStartLineBuilder SetXms(int megabytes)
        {
            CheckRAM(megabytes);

            AddArgument("-Xms", megabytes, "m ");

            return this;
        }

        public string Build() { return stringBuilder.ToString(); }

        /// <summary>
        /// Добавляет аргумент в stringBuilder.
        /// В переменной after в конце оставляйте пробел.
        /// </summary>
        private void AddArgument(string before, object? arg, string after)
        {
            stringBuilder.Append(before);
            stringBuilder.Append(arg);
            stringBuilder.Append(after);
        }

        private void CheckRAM(int megabytes) 
        {
            if (megabytes < MIN_RAM)
                throw new ArgumentException($"RAM can not be lower {MIN_RAM} MB.");

            if (megabytes > MAX_RAM)
                throw new ArgumentException($"RAM can not be higher {MAX_RAM} MB. ({PART_FROM_MAX_RAM}% from max RAM)");
        }
    }
}
