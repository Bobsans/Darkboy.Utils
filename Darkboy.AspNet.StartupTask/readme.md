Darkboy.AspNet.StartupTask
--------------------------------------------------

A simple ASP.net extension provides a helper functionality for run startup tasks

Usage:

```csharp
class ExampleStartupTask : IStartupTask {
    public int Order => 0;
    
    public async Task RunAsync() {
        // do stuff
    }
}

class ExampleBackgroundStartupTask : IBackgroundStartupTask {
    public async Task RunAsync() {
        // do stuff
    }
}
```

```csharp
var builder = WebApplication.CreateBuilder();

builder.Services.AddStartupTask<ExampleStartupTask>();
builder.Services.AddBackgroundStartupTask<ExampleBackgroundStartupTask>();

var app = builder.Build();

await app.RunStartupTasksAsync();

await app.RunAsync();
```
