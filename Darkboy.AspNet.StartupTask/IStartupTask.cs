namespace Darkboy.AspNet.StartupTask;

/// <summary>
/// Represents a task executed during the startup process of the application.
/// </summary>
public interface IStartupTask {
    /// <summary>
    /// Gets the execution order of the startup task.
    /// Tasks with lower order values are executed before tasks with higher order values.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Executes the startup task asynchronously with cancellation support.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the task.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task RunAsync(CancellationToken cancellationToken);
}
