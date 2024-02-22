namespace HardwareSampleMaui.Commands;

internal interface ICancelable
{
    bool CanCancel { get; }
    event BooleanEventHandler? CanCancelChanged;
    Task CancelAsync();
}