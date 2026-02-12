namespace Darkboy.AspNet.StartupTask;

/// <summary>
/// Represents a background task executed during the startup process of the application.
/// </summary>
public interface IBackgroundStartupTask {
    /// <summary>
    /// Gets the execution order of the background startup task.
    /// Tasks with lower order values are executed before tasks with higher order values.
    /// </summary>
    public int Order => 0;

    /// <summary>
    /// Executes the background task as part of the application's startup process.
    /// This method is intended to run asynchronously and may perform long-running operations.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the task.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execution of the background task.</returns>
    public Task RunAsync(CancellationToken cancellationToken);
}
