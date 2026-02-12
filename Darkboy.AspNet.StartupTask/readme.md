Darkboy.AspNet.StartupTask
--------------------------------------------------

A simple ASP.net extension provides a helper functionality for run startup tasks

Usage:

```csharp
class ExampleStartupTask : IStartupTask {
    public int Order => 0;
    
    public async Task RunAsync(CancellationToken cancellationToken) {
        // do stuff
    }
}

class ExampleBackgroundStartupTask : IBackgroundStartupTask {
    public int Order => 0;

    public async Task RunAsync(CancellationToken cancellationToken) {
        // do stuff
    }
}
```

```csharp
var builder = WebApplication.CreateBuilder();

builder.Services.AddStartupTask<ExampleStartupTask>();
builder.Services.AddBackgroundStartupTask<ExampleBackgroundStartupTask>();

var app = builder.Build();

await app.RunStartupTasksAsync(CancellationToken.None);

await app.RunAsync();
```

Notes:
- `IStartupTask` and `IBackgroundStartupTask` require `RunAsync(CancellationToken)` for graceful shutdown.
- Background tasks are still started without blocking app startup and are executed in `Order`.
