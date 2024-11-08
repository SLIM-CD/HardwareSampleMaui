using HardwareSampleMaui.Services;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class CancelTransactionCommand(IDeviceService deviceService) : CommandBase
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

            var reply = await device.DeviceTransactionCancelAsync();
            OnFinished(reply != null 
                ? Response<object>.Success(reply)
                : Response<object>.Failure("Cancellation attempt did not return a valid response"));
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