using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UnixLauncher.Windows;
using UnixLauncher.Core;
using UnixLauncher.Core.Config;
using UnixLauncher.Core.Misc;
using UnixLauncher.Core.Tokens;
using UnixLauncher.Core.Providers;
using UnixLauncher.Core.MinecraftClient;
using UnixLauncher.Core.Logger;

namespace UnixLauncher
{
    public partial class App : Application
    {
        private IHost? _host;

        public App()
        {
            _host = CreateHostBuilder().Build();
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    ConfigureServices(services);
                });

        private static void ConfigureServices(IServiceCollection services)
        {
            // Конфигурация
            services.Configure<TokenProviderOptions>(options =>
                options.TokenDirectory = Path.Combine(AppDataProvider.GetFolder(), "User Secret"));
            services.Configure<LoggerOptions>(options =>
            {
                options.MinLogLevel = LogLevel.Trace;
                options.Directory = AppDataProvider.GetFolder();
                options.FileName = "logs.txt";
            });


            // Логи
            services.AddSingleton<ILogger, FileLogger>(provider =>
                new(provider.GetRequiredService<IOptions<LoggerOptions>>().Value));

            // Сервисы уровня приложения
            services.AddSingleton<MCStartLineBuilder>();
            services.AddSingleton<IMemoryProvider, FuncsRAM>();
            services.AddSingleton<ITokensProvider>(provider =>
                new FileTokenProvider(provider.GetRequiredService<IOptions<TokenProviderOptions>>().Value));
            services.AddSingleton<IConfig, DefaultConfig>();

            // UI компоненты
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginPopupWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host!.StartAsync();

            var startupForm = _host.Services.GetRequiredService<MainWindow>();
            startupForm.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}