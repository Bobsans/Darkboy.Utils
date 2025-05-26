namespace Darkboy.AspNet.StarupTask;

public interface IStartupTask {
    public int Order { get; }
    public Task RunAsync();
}
