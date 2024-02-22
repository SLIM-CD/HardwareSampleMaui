using SlimCDDevice;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

public sealed class DiscoverAssembliesCommand : CommandBase
{
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