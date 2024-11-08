using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlimCDDevice;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Services;

public interface IDeviceService
{
    ISlimCDDeviceAsync? Device { get; }
    IAssemblyInfo? AssemblyInfo { get; }
    void SetDevice(ISlimCDDeviceAsync device, IAssemblyInfo assemblyInfo);
}

public sealed class DeviceServiceChangedEventArgs(string? propertyName = null, object? previousDeviceInstance = null) : PropertyChangedEventArgs(propertyName)
{
    public object? PreviousDeviceInstance { get; } = previousDeviceInstance;
}

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class DeviceService : IDeviceService, INotifyPropertyChanged
{
    public ISlimCDDeviceAsync? Device { get; private set; }
    public IAssemblyInfo? AssemblyInfo { get; private set; }
    
    public void SetDevice(ISlimCDDeviceAsync device, IAssemblyInfo assemblyInfo)
    {
        var previousDeviceInstance = Device;
        Device = device;
        AssemblyInfo = assemblyInfo;
        OnPropertyChanged(previousDeviceInstance, nameof(Device));
    }

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(object? previousDeviceInstance = null, [CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new DeviceServiceChangedEventArgs(propertyName, previousDeviceInstance));
    #endregion
}