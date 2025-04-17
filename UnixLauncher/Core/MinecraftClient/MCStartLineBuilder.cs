using UnixLauncher.Core.Logger;
using UnixLauncher.Core.Misc;

namespace UnixLauncher.Core.MinecraftClient
{
    internal class MCStartLineBuilder
    {
        private readonly MCStartLine buildInstance;
        private readonly IMemoryProvider memoryProvider;
        private readonly ILogger logger;

        private const long MIN_RAM = 1024; // MB

        // Больше этой части нельзя выделить оперативку
        // не уверен, насколько это обосновано, но кажется, лучше оставить для системы
        private const int PART_FROM_MAX_RAM = 70; // %
        private readonly long MAX_RAM;

        // TODO:
        // - проверь аймеморипровайдер

        public MCStartLineBuilder(IMemoryProvider memoryProvider, ILogger logger)
        {
            // --- DI
            this.memoryProvider = memoryProvider;
            this.logger = logger;

            // --- Get RAM
            memoryProvider.GetRAM(out long kbRAM);

            if (kbRAM < 1)
            {
                ArgumentException exception =
new("Core error: IMemoryProvider gave out too little RAM (How does it even work if we have <1KB of RAM?)");

                using (logger.BeginScope(this.GetType()))
                    logger.ErrorAsync(exception);

                throw exception;
            }    

            // --- Calculating part of max RAM
            long mbRAM = kbRAM / 1024;
            MAX_RAM = mbRAM * PART_FROM_MAX_RAM / 100;

            buildInstance = new();

            using (logger.BeginScope(this.GetType()))
                logger.DebugAsync("Load OK!");
        }

        public async Task<MCStartLineBuilder> SetXmx(int megabytes)
        {
            await CheckRAM(megabytes);

            buildInstance.AddArgument("-Xmx", megabytes, "m ");

            using (logger.BeginScope(this.GetType()))
                await logger.TraceAsync($"Set {megabytes} Xmx");

            return this;
        }

        public async Task<MCStartLineBuilder> SetXms(int megabytes)
        {
            await CheckRAM(megabytes);

            buildInstance.AddArgument("-Xms", megabytes, "m ");

            using (logger.BeginScope(this.GetType()))
                await logger.TraceAsync($"Set {megabytes} Xms");

            return this;
        }

        // Если MCStartLine начнет обрастать функционалом,
        // вместо string надо начать отдавать сам объект
        public string Build() { return buildInstance.ToString(); }

        private async Task CheckRAM(int megabytes) 
        {
            if (megabytes < MIN_RAM)
            {
                var argumentException = 
                    new ArgumentException($"RAM can not be lower {MIN_RAM} MB.", nameof(megabytes));
                await logger.ErrorAsync(argumentException);
                throw argumentException;
            }

            if (megabytes > MAX_RAM)
            {
                var argumentException = 
                    new ArgumentException($"RAM can not be higher {MAX_RAM} MB. ({PART_FROM_MAX_RAM}% from max RAM)", nameof(megabytes));
                await logger.ErrorAsync(argumentException);
                throw argumentException;
            }

            return;
        }
    }
}
