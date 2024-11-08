using CommunityToolkit.Maui.Storage;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class SelectAssembliesPathCommand(IFolderPicker folderPicker) : CommandBase
{
    public IFolderPicker FolderPicker { get; } = folderPicker;

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

            FolderPickerResult result;
            if (OperatingSystem.IsWindows() ||
                OperatingSystem.IsAndroid() ||
                OperatingSystem.IsIOSVersionAtLeast(11) ||
                OperatingSystem.IsMacCatalystVersionAtLeast(14))
            {
                result = await FolderPicker.PickAsync(Cts.Token);
            }
            else
            {
                OnFinished(Response<object>.Failure("Folder selection is not supported on this platform"));
                return;
            }
                
            OnFinished(result.IsSuccessful
                ? Response<object>.Success(result.Folder)
                : Response<object>.Failure(result.Exception.Message));
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