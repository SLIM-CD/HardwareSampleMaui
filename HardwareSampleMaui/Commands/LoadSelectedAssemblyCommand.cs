using HardwareSampleMaui.Services;
using SlimCDDevice;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

public class LoadSelectedAssemblyCommand(PlatformSpecificService platformSpecificService) : CommandBase
{
    private IPlatformSpecific PlatformSpecific { get; } = platformSpecificService.Get();

    public override async void Execute(object? parameter)
    {
        var executing = false;
        var executionSync = ExecutionSync;
        if (executionSync == null)
            return;

        try
        {
            if (!(executing = await executionSync.WaitAsync(0)))
                return;

            OnStarting();

            CanBeExecuted = false;
                
            if (parameter is not IAssemblyInfo selectedDeviceAssembly)
            {
                OnFinished(Response<object>.Failure("Device assembly has not been selected"));
                return;
            }
                
            CanCancel = true;
            Cts = new CancellationTokenSource();

            var response = await Assemblies.GetInstance(selectedDeviceAssembly, PlatformSpecific, Cts.Token);
            OnFinished(response.IsSuccess
                ? Response<object>.Success(response.Data)
                : Response<object>.Failure(response.Message));
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