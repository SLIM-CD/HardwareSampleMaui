using HardwareSampleMaui.Services;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class DiscoverCommCommand(IDeviceService deviceService) : CommandBase
{
    private IDeviceService DeviceService { get; } = deviceService;

    public override async void Execute(object? parameter)
    {
        var executing = false;
        var executionSync = ExecutionSync;
        if (executionSync == null)
            return;

        try
        {
            executing = await executionSync.WaitAsync(0);
            if (!executing)
                return;

            OnStarting();

            CanBeExecuted = false;

            CanCancel = true;
            Cts = new CancellationTokenSource();

            var device = DeviceService.Device;
            if (device == null)
            {
                OnFinished(Response<object>.Failure("The device library is not found"));
                return;
            }

            if (device is not ICommManager commManager)
            {
                OnFinished(Response<object>.Failure("The device library doesn't support communication management functionality."));
                return;
            }

            if (parameter is not ConnectionType connectionType)
            {
                OnFinished(Response<object>.Failure("Invalid connection type"));
                return;
            }

            var channelsResponse = await Task.Run(() => commManager.DiscoverComm(connectionType, 10000, CancellationToken.None));
            OnFinished(channelsResponse.IsSuccess
                ? Response<object>.Success(channelsResponse.Data)
                : Response<object>.Failure(channelsResponse.Message));
        }
        catch (Exception ex)
        {
            OnFinished(Response<object>.Failure(ex.Message));
        }
        finally
        {
            if (executing)
            {
                CanCancel = false;
                Cts?.Dispose();
                Cts = null;

                CanBeExecuted = true;

                executionSync.Release();
            }
        }
    }
}