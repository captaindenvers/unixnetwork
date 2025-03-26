using System.Text;
using UnixLauncher.Core.Misc;

namespace UnixLauncher.Core.MinecraftClient
{
    internal class MCStartLineBuilder
    {
        private readonly MCStartLine buildInstance;
        private readonly IMemoryProvider memoryProvider;


        private const long MIN_RAM = 1024; // MB

        // Больше этой части нельзя выделить оперативку
        // не уверен, насколько это обосновано, но кажется, лучше оставить для системы
        private const int PART_FROM_MAX_RAM = 70; // %
        private readonly long MAX_RAM;

        // TODO:
        // - проверь аймеморипровайдер

        public MCStartLineBuilder(IMemoryProvider memoryProvider)
        {
            // --- DI
            this.memoryProvider = memoryProvider;

            // --- Get RAM
            memoryProvider.GetRAM(out long kbRAM);

            if (kbRAM < 1)
                throw new ArgumentException("Core error: IMemoryProvider gave out too " +
                    "little RAM (How does it even work if we have <1KB of RAM?)");

            // --- Calculating part of max RAM
            long mbRAM = kbRAM / 1024;
            MAX_RAM = mbRAM * PART_FROM_MAX_RAM / 100;

            buildInstance = new();
        }

        public MCStartLineBuilder SetXmx(int megabytes)
        {
            CheckRAM(megabytes);

            buildInstance.AddArgument("-Xmx", megabytes, "m ");

            return this;
        }

        public MCStartLineBuilder SetXms(int megabytes)
        {
            CheckRAM(megabytes);

            buildInstance.AddArgument("-Xms", megabytes, "m ");

            return this;
        }

        // Если MCStartLine начнет обрастать функционалом,
        // вместо string надо начать отдавать сам объект
        public string Build() { return buildInstance.ToString(); }

        private void CheckRAM(int megabytes) 
        {
            if (megabytes < MIN_RAM)
                throw new ArgumentException($"RAM can not be lower {MIN_RAM} MB.", nameof(megabytes));

            if (megabytes > MAX_RAM)
                throw new ArgumentException($"RAM can not be higher {MAX_RAM} MB. ({PART_FROM_MAX_RAM}% from max RAM)", nameof(megabytes));
        }
    }
}
