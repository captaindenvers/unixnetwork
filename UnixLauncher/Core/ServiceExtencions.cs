using Microsoft.Extensions.DependencyInjection;

namespace UnixLauncher.Core
{
    public static class ServiceExtensions
    {
        public static void AddFactory<TForm>(this IServiceCollection services)
            where TForm : class
        {
            services.AddTransient<TForm>();
            services.AddSingleton<Func<TForm>>(x => () => x.GetService<TForm>()!);
            services.AddSingleton<IAbstractFactory<TForm>, AbstractFactory<TForm>>();
        }
    }
}