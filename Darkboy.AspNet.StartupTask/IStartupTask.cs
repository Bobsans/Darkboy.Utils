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
    /// Executes the startup task asynchronously. This method is intended to be implemented
    /// by classes adhering to the <see cref="IStartupTask"/> interface and serves as the entry
    /// point for the task's execution logic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task RunAsync();
}
