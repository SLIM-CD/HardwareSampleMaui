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

    public Func<Action, Task> Invoker { get; set; } = MacCatalystInvoker;
    public Func<Func<Task>, Task> InvokerAsync { get; set; } = MacCatalystInvokerAsync;

    private static async Task MacCatalystInvoker(Action action)
    {
        if (MainThread.IsMainThread)
            action.Invoke();
        else
            await MainThread.InvokeOnMainThreadAsync(action);
    }

    private static async Task MacCatalystInvokerAsync(Func<Task> func)
    {
        if (MainThread.IsMainThread)
            await func();
        else
            await MainThread.InvokeOnMainThreadAsync(func);
    }

    public Func<string, ISlimCDDeviceAsync, Task> Initializer { get; set; } = MacCatalystInitializer;
    private static Task MacCatalystInitializer(string deviceAssemblyName, ISlimCDDeviceAsync deviceLibrary)
    {
        return Task.CompletedTask;
    }
}