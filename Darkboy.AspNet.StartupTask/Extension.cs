using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Darkboy.AspNet.StartupTask;

public static class Extension {
    public static async Task RunStartupTasksAsync(this IHost host) {
        await using var scope = host.Services.CreateAsyncScope();

        foreach (var task in scope.ServiceProvider.GetServices<IStartupTask>().OrderBy(it => it.Order)) {
            await task.RunAsync();
        }
    }

    public static IServiceCollection AddStartupTask<T>(this IServiceCollection services) where T : class, IStartupTask {
        return services.AddTransient<IStartupTask, T>();
    }
}
