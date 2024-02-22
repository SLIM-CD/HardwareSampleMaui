using Android.Bluetooth;
using Android.Content;
using CommManagerAndroid;
using SlimCDTypeLib;

// ReSharper disable once CheckNamespace
namespace HardwareSampleMaui.Services;

public partial class PlatformSpecificService
{
    public partial IPlatformSpecific Get() => new PlatformSpecific();
}

internal class PlatformSpecific : IPlatformSpecific
{
    public static Context? AndroidContext { get; set; }

    public ICommManager? CommManager { get; set; } = new CommManager();

    public Func<Task>? DoEvents { get; set; }

    public Func<Action, Task> Invoker { get; set; } = AndroidInvoker;
    public Func<Func<Task>, Task> InvokerAsync { get; set; } = AndroidInvokerAsync;

    private static async Task AndroidInvoker(Action action)
    {
        if (MainThread.IsMainThread)
            action.Invoke();
        else
            await MainThread.InvokeOnMainThreadAsync(action);
    }

    private static async Task AndroidInvokerAsync(Func<Task> func)
    {
        if (MainThread.IsMainThread)
            await func();
        else
            await MainThread.InvokeOnMainThreadAsync(func);
    }

    public Func<string, ISlimCDDeviceAsync, Task> Initializer { get; set; } = AndroidInitializer;
    private static Task AndroidInitializer(string deviceAssemblyName, ISlimCDDeviceAsync deviceLibrary)
    {
        if (AndroidContext == null)
            throw new ArgumentException("Invalid usage, AndroidContext has not been set", nameof(AndroidContext));
        Bluetooth.Initialize(AndroidContext.GetSystemService(Context.BluetoothService) as BluetoothManager);
        return Task.CompletedTask;
    }
}