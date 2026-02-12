namespace Darkboy.AspNet.StartupTask;

/// <summary>
/// Represents a background task executed during the startup process of the application.
/// </summary>
public interface IBackgroundStartupTask {
    /// <summary>
    /// Executes the background task as part of the application's startup process.
    /// This method is intended to run asynchronously and may perform long-running operations.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execution of the background task.</returns>
    public Task RunAsync();
}
