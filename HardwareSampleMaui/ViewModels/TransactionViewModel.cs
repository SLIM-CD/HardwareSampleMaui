using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HardwareSampleMaui.Commands;
using SlimCDTypeLib;

namespace HardwareSampleMaui.ViewModels;

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class TransactionViewModel : ITransactionParameters, INotifyPropertyChanged
{
    private const string Standby = "Standby";

    #region Backing Fields
    private string _status = Standby;
    private decimal _amount;
    private bool _isTransactionInProgress;
    private bool _isCancelInProgress;
    private bool _canGoPreviousPage = true;
    private bool _uiEntryEnabled = true;

    #endregion

    public string Status
    {
        get => _status;
        set
        {
            if (value == _status) 
                return;
            _status = value;
            OnPropertyChanged();
        }
    }
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (value == _amount) 
                return;
            _amount = value;
            OnPropertyChanged();
        }
    }

    public ICommand Start { get; }
    public ICommand Cancel { get; }

    private bool IsTransactionInProgress
    {
        get => _isTransactionInProgress;
        set
        {
            _isTransactionInProgress = value;
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanCancel));
        }
    }
    private bool IsCancelInProgress
    {
        get => _isCancelInProgress;
        set
        {
            if (value == _isCancelInProgress) 
                return;
            _isCancelInProgress = value;
            OnPropertyChanged(nameof(CanCancel));
        }
    }
    public bool CanStart => UiEntryEnabled && !IsTransactionInProgress;
    public bool CanCancel => IsTransactionInProgress && !IsCancelInProgress;
    public bool CanGoPreviousPage
    {
        get => _canGoPreviousPage;
        set
        {
            if (value == _canGoPreviousPage) 
                return;
            _canGoPreviousPage = value;
            OnPropertyChanged();
        }
    }
    public bool UiEntryEnabled
    {
        get => _uiEntryEnabled;
        set
        {
            if (value == _uiEntryEnabled)
                return;
            _uiEntryEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanStart));
        }
    }

    public TransactionViewModel(   
        StartTransactionCommand start,
        CancelTransactionCommand cancel)
    {
        start.Starting += OnTransactionStarting;
        start.Finished += OnTransactionFinished;
        Start = start;

        cancel.Starting += OnCancelStarting;
        cancel.Finished += OnCancelFinished;
        Cancel = cancel;
    }

    private void OnTransactionStarting(object? sender, EventArgs e)
    {
        UiEntryEnabled = false;
        CanGoPreviousPage = false;
        IsTransactionInProgress = true;
        Status = "Starting transaction";
    }

    private void OnTransactionFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        try
        {
            var response = eventArgs.Response;
            if (response.IsFailure)
            {
                Status = response.Message;
                return;
            }

            if (response.Data is not DeviceTransactionReply reply)
            {
                Status = "Unable to obtain the transaction reply";
                return;
            }

            Status =  
                $"Response: '{reply.response}'{Environment.NewLine}" +
                $"ResponseCode: '{reply.responsecode}'{Environment.NewLine}" +
                $"Description: '{reply.description}'{Environment.NewLine}" +
                $"GateId: '{reply.gateid}'{Environment.NewLine}" +
                $"Approved: '{reply.approved}'{Environment.NewLine}" +
                $"ApprovedAmt: '{reply.approvedamt}'{Environment.NewLine}" +
                $"AuthCode: '{reply.authcode}'";

            if (reply.responsecode == "0") 
                Amount = 0;
        }
        finally
        {
            IsCancelInProgress = false;
            IsTransactionInProgress = false;
            CanGoPreviousPage = true;
            UiEntryEnabled = true;
        }
    }

    private void OnCancelStarting(object? sender, EventArgs e)
    {
        IsCancelInProgress = true;
    }

    private static async void OnCancelFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        var response = eventArgs.Response;
        if (response.IsFailure)
        {
            await (Application.Current?.MainPage?.DisplayAlert("Error", response.Message, "OK") ?? Task.CompletedTask);
            return;
        }

        var reply = (DeviceTransactionCancelReply)response.Data!;
        if (reply.responsecode != "0") 
            await (Application.Current?.MainPage?.DisplayAlert("Error", reply.description, "OK") ?? Task.CompletedTask);
    }

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion
}