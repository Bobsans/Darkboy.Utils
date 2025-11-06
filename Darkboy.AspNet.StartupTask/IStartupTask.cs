namespace Darkboy.AspNet.StartupTask;

public interface IStartupTask {
    public int Order { get; }
    public Task RunAsync();
}
