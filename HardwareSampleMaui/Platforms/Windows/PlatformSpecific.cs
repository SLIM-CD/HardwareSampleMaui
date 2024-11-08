using SlimCDTypeLib;

// ReSharper disable once CheckNamespace
namespace HardwareSampleMaui.Services;

public partial class PlatformSpecificService
{
#pragma warning disable CA1822 // Mark members as static
    public partial IPlatformSpecific Get() => new PlatformSpecific();
#pragma warning restore CA1822 // Mark members as static
}

internal class PlatformSpecific : IPlatformSpecific
{
    public ICommManager? CommManager { get; set; }

    public Func<Task>? DoEvents { get; set; }

    public Func<Action, Task> Invoker { get; set; } = WindowsInvoker;
    public Func<Func<Task>, Task> InvokerAsync { get; set; } = WindowsInvokerAsync;

    private static async Task WindowsInvoker(Action action)
    {
        if (MainThread.IsMainThread)
            action.Invoke();
        else
            await MainThread.InvokeOnMainThreadAsync(action);
    }

    private static async Task WindowsInvokerAsync(Func<Task> func)
    {
        if (MainThread.IsMainThread)
            await func();
        else
            await MainThread.InvokeOnMainThreadAsync(func);
    }

    public Func<string, ISlimCDDeviceAsync, Task> Initializer { get; set; } = WindowsInitializer;
    private static Task WindowsInitializer(string deviceAssemblyName, ISlimCDDeviceAsync deviceLibrary)
    {
        return Task.CompletedTask;
    }
}