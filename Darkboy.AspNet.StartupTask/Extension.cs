using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Darkboy.AspNet.StarupTask;

public static class Extension {
    public static async Task RunStartupTasksAsync(this IHost host) {
        await using var scope = host.Services.CreateAsyncScope();

        foreach (var task in scope.ServiceProvider.GetServices<IStartupTask>().OrderBy(it => it.Order)) {
            await task.RunAsync();
        }
    }

    public static IHostApplicationBuilder AddStartupTask<T>(this IHostApplicationBuilder builder) where T : class, IStartupTask {
        builder.Services.AddTransient<IStartupTask, T>();
        return builder;
    }
}
