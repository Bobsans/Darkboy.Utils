using Microsoft.Extensions.Hosting;

namespace Darkboy.AspNet.StartupTask.Tests;

public class StartupTaskExtensionsTests {
    [Fact]
    public async Task RunStartupTasksAsync_ExecutesImmediateTasksInOrder() {
        ImmediateTaskState.Reset();

        using var host = new HostBuilder()
            .ConfigureServices(services => {
                services.AddStartupTask<ImmediateTaskOrderTwo>();
                services.AddStartupTask<ImmediateTaskOrderOne>();
            })
            .Build();

        await host.RunStartupTasksAsync(CancellationToken.None);

        Assert.Equal(new[] { "first", "second" }, ImmediateTaskState.Executions.ToArray());
    }

    [Fact]
    public async Task RunStartupTasksAsync_StartsBackgroundTasksWithoutBlocking() {
        BlockingBackgroundTaskState.Reset();
        using var cts = new CancellationTokenSource();

        using var host = new HostBuilder()
            .ConfigureServices(services => { services.AddBackgroundStartupTask<BlockingBackgroundTask>(); })
            .Build();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await host.RunStartupTasksAsync(cts.Token);
        stopwatch.Stop();

        await BlockingBackgroundTaskState.Started.Task.WaitAsync(TimeSpan.FromSeconds(2), cts.Token);

        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(500));

        await cts.CancelAsync();
        BlockingBackgroundTaskState.Release.TrySetResult(true);
    }

    [Fact]
    public async Task RunStartupTasksAsync_BackgroundExceptionDoesNotStopNextTasks() {
        BackgroundExceptionState.Reset();

        using var host = new HostBuilder()
            .ConfigureServices(services => {
                services.AddBackgroundStartupTask<FaultedBackgroundTask>();
                services.AddBackgroundStartupTask<SignalBackgroundTask>();
            })
            .Build();

        await host.RunStartupTasksAsync(CancellationToken.None);

        await BackgroundExceptionState.SecondTaskRan.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(BackgroundExceptionState.SecondTaskRan.Task.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task RunStartupTasksAsync_RespectsCancellationForImmediateTasks() {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        using var host = new HostBuilder()
            .ConfigureServices(services => { services.AddStartupTask<ImmediateTaskOrderOne>(); })
            .Build();

        await Assert.ThrowsAsync<OperationCanceledException>(() => host.RunStartupTasksAsync(cts.Token));
    }

    #region Tasks

    private static class ImmediateTaskState {
        public static readonly List<string> Executions = [];

        public static void Reset() {
            Executions.Clear();
        }
    }

    private sealed class ImmediateTaskOrderOne : IStartupTask {
        public int Order => 1;

        public Task RunAsync(CancellationToken cancellationToken) {
            ImmediateTaskState.Executions.Add("first");
            return Task.CompletedTask;
        }
    }

    private sealed class ImmediateTaskOrderTwo : IStartupTask {
        public int Order => 2;

        public Task RunAsync(CancellationToken cancellationToken) {
            ImmediateTaskState.Executions.Add("second");
            return Task.CompletedTask;
        }
    }

    private static class BlockingBackgroundTaskState {
        public static TaskCompletionSource<bool> Started { get; private set; } = NewTcs();
        public static TaskCompletionSource<bool> Release { get; private set; } = NewTcs();

        public static void Reset() {
            Started = NewTcs();
            Release = NewTcs();
        }

        private static TaskCompletionSource<bool> NewTcs() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class BlockingBackgroundTask : IBackgroundStartupTask {
        public Task RunAsync(CancellationToken cancellationToken) {
            BlockingBackgroundTaskState.Started.TrySetResult(true);
            return BlockingBackgroundTaskState.Release.Task.WaitAsync(cancellationToken);
        }

    }

    private static class BackgroundExceptionState {
        public static TaskCompletionSource<bool> SecondTaskRan { get; private set; } = NewTcs();

        public static void Reset() {
            SecondTaskRan = NewTcs();
        }

        private static TaskCompletionSource<bool> NewTcs() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class FaultedBackgroundTask : IBackgroundStartupTask {
        public int Order => 0;

        public Task RunAsync(CancellationToken cancellationToken) => throw new InvalidOperationException("Intentional failure");
    }

    private sealed class SignalBackgroundTask : IBackgroundStartupTask {
        public int Order => 1;

        public Task RunAsync(CancellationToken cancellationToken) {
            BackgroundExceptionState.SecondTaskRan.TrySetResult(true);
            return Task.CompletedTask;
        }
    }

    #endregion
}
