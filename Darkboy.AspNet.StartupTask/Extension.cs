using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Darkboy.AspNet.StartupTask;

public static class Extension {
    /// <summary>
    /// Executes all registered startup tasks in the application.
    /// Immediate tasks are executed synchronously and awaited.
    /// Background tasks are started asynchronously without blocking startup.
    /// </summary>
    /// <param name="host">An instance of <see cref="IHost"/> representing the application host.</param>
    /// <param name="cancellationToken">A token that can be used to cancel startup task execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunStartupTasksAsync(this IHost host, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(host);

        await RunImmediateTasksAsync(host, cancellationToken);

        var lifetime = host.Services.GetService<IHostApplicationLifetime>();
        var linkedCts = lifetime is null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.ApplicationStopping);

        StartBackgroundTasks(host, linkedCts);
    }

    private static void StartBackgroundTasks(IHost host, CancellationTokenSource cancellationTokenSource) {
        var logger = host.Services.GetService<ILoggerFactory>()?.CreateLogger(typeof(Extension));
        var backgroundTask = RunBackgroundTasksAsync(host, logger, cancellationTokenSource.Token);

        _ = backgroundTask.ContinueWith(continuation => {
            if (continuation.Exception is not null) {
                logger?.LogError(continuation.Exception, "Background startup tasks failed.");
            }
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);

        _ = backgroundTask.ContinueWith(_ => cancellationTokenSource.Dispose(), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
    }

    private static async Task RunImmediateTasksAsync(IHost host, CancellationToken cancellationToken) {
        await using var scope = host.Services.CreateAsyncScope();

        foreach (var task in scope.ServiceProvider.GetServices<IStartupTask>().OrderBy(it => it.Order)) {
            cancellationToken.ThrowIfCancellationRequested();
            await task.RunAsync(cancellationToken);
        }
    }

    private static async Task RunBackgroundTasksAsync(IHost host, ILogger? logger, CancellationToken cancellationToken) {
        await using var scope = host.Services.CreateAsyncScope();

        foreach (var task in scope.ServiceProvider.GetServices<IBackgroundStartupTask>().OrderBy(it => it.Order)) {
            if (cancellationToken.IsCancellationRequested) {
                return;
            }

            try {
                await task.RunAsync(cancellationToken);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                return;
            } catch (Exception ex) {
                logger?.LogError(ex, "Background startup task {TaskType} failed.", task.GetType().FullName);
            }
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
            ArgumentNullException.ThrowIfNull(services);
            return services.AddTransient<IStartupTask, T>();
        }

        /// <summary>
        /// Registers a background startup task with the application's dependency injection container.
        /// Background startup tasks are executed asynchronously after the application starts.
        /// </summary>
        /// <typeparam name="T">The type of the background startup task to register, implementing <see cref="IBackgroundStartupTask"/>.</typeparam>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddBackgroundStartupTask<T>() where T : class, IBackgroundStartupTask {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddTransient<IBackgroundStartupTask, T>();
        }
    }
}
