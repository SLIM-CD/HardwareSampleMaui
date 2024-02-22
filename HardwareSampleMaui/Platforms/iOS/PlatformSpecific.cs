using SlimCDTypeLib;

// ReSharper disable once CheckNamespace
namespace HardwareSampleMaui.Services;

public partial class PlatformSpecificService
{
    public partial IPlatformSpecific Get() => new PlatformSpecific();
}

internal class PlatformSpecific : IPlatformSpecific
{
    public ICommManager? CommManager { get; set; }

    public Func<Task>? DoEvents { get; set; }

    public Func<Action, Task> Invoker { get; set; } = IosInvoker;
    public Func<Func<Task>, Task> InvokerAsync { get; set; } = IosInvokerAsync;

    private static async Task IosInvoker(Action action)
    {
        if (MainThread.IsMainThread)
            action.Invoke();
        else
            await MainThread.InvokeOnMainThreadAsync(action);
    }

    private static async Task IosInvokerAsync(Func<Task> func)
    {
        if (MainThread.IsMainThread)
            await func();
        else
            await MainThread.InvokeOnMainThreadAsync(func);
    }

    public Func<string, ISlimCDDeviceAsync, Task> Initializer { get; set; } = IosInitializer;
    private static Task IosInitializer(string deviceAssemblyName, ISlimCDDeviceAsync deviceLibrary)
    {
        return Task.CompletedTask;
    }
}