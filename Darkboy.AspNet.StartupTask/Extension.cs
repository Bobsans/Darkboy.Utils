using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Darkboy.AspNet.StartupTask;

public static class Extension {
    /// <summary>
    /// Executes all registered startup tasks in the application. Immediate tasks are executed synchronously,
    /// while background tasks are executed asynchronously in a dedicated thread.
    /// </summary>
    /// <param name="host">An instance of <see cref="IHost"/> representing the application host.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunStartupTasksAsync(this IHost host) {
        await RunImmediateTasksAsync(host);

        _ = Task.Run(() => RunBackgroundTasksAsync(host));
    }

    private static async Task RunImmediateTasksAsync(IHost host) {
        await using var scope = host.Services.CreateAsyncScope();

        foreach (var task in scope.ServiceProvider.GetServices<IStartupTask>().OrderBy(it => it.Order)) {
            await task.RunAsync();
        }
    }

    private static async Task RunBackgroundTasksAsync(IHost host) {
        await using var scope = host.Services.CreateAsyncScope();

        foreach (var task in scope.ServiceProvider.GetServices<IBackgroundStartupTask>()) {
            await task.RunAsync();
        }
    }

    extension(IServiceCollection services) {
        /// <summary>
        /// Registers a startup task with the application's dependency injection container.
        /// Startup tasks are executed during the application startup process based on their defined order.
        /// </summary>
        /// <typeparam name="T">The type of the startup task to register, implementing <see cref="IStartupTask"/>.</typeparam>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddStartupTask<T>() where T : class, IStartupTask {
            return services.AddTransient<IStartupTask, T>();
        }

        /// <summary>
        /// Registers a background startup task with the application's dependency injection container.
        /// Background startup tasks are executed asynchronously after the application starts.
        /// </summary>
        /// <typeparam name="T">The type of the background startup task to register, implementing <see cref="IBackgroundStartupTask"/>.</typeparam>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddBackgroundStartupTask<T>() where T : class, IBackgroundStartupTask {
            return services.AddTransient<IBackgroundStartupTask, T>();
        }
    }
}
