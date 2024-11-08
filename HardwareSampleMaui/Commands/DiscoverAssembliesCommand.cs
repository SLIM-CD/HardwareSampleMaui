using SlimCDDevice;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class DiscoverAssembliesCommand : CommandBase
{
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

            var discoverResponse = parameter is string assembliesPath && assembliesPath != ""
                ? await Assemblies.DiscoverUsingPath(nameof(ISlimCDDeviceAsync), assembliesPath, Cts.Token) // Discover device assemblies using the provided path
                : await Assemblies.DiscoverAsync(nameof(ISlimCDDeviceAsync), Cts.Token);                    // Discover device assemblies at the executable location

            OnFinished(discoverResponse.IsSuccess 
                ? Response<object>.Success(discoverResponse.Data)
                : Response<object>.Failure(discoverResponse.Message));
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