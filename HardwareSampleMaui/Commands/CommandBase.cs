using SlimCDTypeLib;
using System.Windows.Input;

namespace HardwareSampleMaui.Commands;

public abstract class CommandBase : ICommand, ICancelable, IDisposable, IAsyncDisposable
{
    #region ICommand CanExecute Implementation
    private bool _canBeExecuted = true;
    protected bool CanBeExecuted
    {
        get => _canBeExecuted;
        set
        {
            if (_canBeExecuted == value)
                return;
            _canBeExecuted = value;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CanExecute(object? parameter) => CanBeExecuted;

    public event EventHandler? CanExecuteChanged;

    #endregion

    #region ICancelable Implementation
    private bool _canCancel;
    public bool CanCancel
    {
        get => _canCancel;
        set
        {
            if (_canCancel == value)
                return;
            _canCancel = value;
            OnCanCancelChanged(value);
        }
    }

    public event BooleanEventHandler? CanCancelChanged;

    protected CancellationTokenSource? Cts { get; set; }

    public async Task CancelAsync()
    {
        if (!CanCancel)
            return;

        var cancelTask = Cts?.CancelAsync();
        if (cancelTask == null)
            return;

        await cancelTask;
    }

    #endregion

    protected SemaphoreSlim? ExecutionSync { get; set; } = new(1, 1);

    public event EventHandler? Starting;
    public event CommandResponseEventHandler? Finished;

    public abstract void Execute(object? parameter);

    #region Event Invokators
    protected virtual void OnStarting() => Starting?.Invoke(this, EventArgs.Empty);
    protected virtual void OnFinished(IResponse<object?> response) => Finished?.Invoke(this, new CommandResponseEventArgs(response));
    protected virtual void OnCanCancelChanged(bool value) => CanCancelChanged?.Invoke(this, new BooleanEventArgs(value));

    #endregion

    #region IDisposable Implementation
    public void Dispose()
    {
        CastAndDispose(Cts);
        CastAndDispose(ExecutionSync);
        Cts = null;
        ExecutionSync = null;

        return;

        static void CastAndDispose(object? resource)
        {
            if (resource is IDisposable disposable)
                disposable.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CastAndDispose(Cts);
        await CastAndDispose(ExecutionSync);

        return;

        static async ValueTask CastAndDispose(object? resource)
        {
            if (resource is not IDisposable disposable)
                return;
            if (disposable is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                disposable.Dispose();
        }
    }

    #endregion
}