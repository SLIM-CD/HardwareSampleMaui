using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using DeviceFramework.CommonTypes;
using HardwareSampleMaui.Commands;
using HardwareSampleMaui.Pages;
using HardwareSampleMaui.Services;
using SlimCDTypeLib;
using ICommand = System.Windows.Input.ICommand;

namespace HardwareSampleMaui.ViewModels;

public class ConnectionViewModel : IConnectionParameters, INotifyPropertyChanged
{
    #region Backing Fields
    private static readonly ICommChannel NoneSelectedChannel = new CommChannel { Id = "None", Name = "None", Description = "None" };
    private ConnectionType _selectedConnectionType;
    private ICommChannel? _selectedCommChannel;
    private string _ipAddress = "";
    private int _ipPort;
    private string _connectionStatus = "Disconnected";
    private string _connectedDeviceMake = "";
    private string _connectedDeviceModel = "";
    private bool _isConnected;
    private bool _canGoPreviousPage = true;
    private bool _uiEntryEnabled = true;
    private string _error = "";

    #endregion

    public IDeviceService DeviceService { get; }

    public ObservableCollection<ConnectionType> ConnectionTypes { get; }
    public ConnectionType SelectedConnectionType
    {
        get => _selectedConnectionType;
        set
        {
            if (Equals(value, _selectedConnectionType)) 
                return;
            _selectedConnectionType = value;
            SelectedCommChannel = null;
            CommChannels.Clear();
            Error = "";
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanDiscoverComm));
            OnPropertyChanged(nameof(CanEnterIpAddress));
            OnPropertyChanged(nameof(IsIpAddressVisible));
            OnPropertyChanged(nameof(IsCommChannelsVisible));
        }
    }

    public ObservableCollection<ICommChannel> CommChannels { get; }
    public ICommChannel? SelectedCommChannel
    {
        get => _selectedCommChannel;
        set
        {
            if (ReferenceEquals(value, NoneSelectedChannel))
                value = null;

            if (_selectedCommChannel == null && value == null ||
                _selectedCommChannel != null && value != null && 
                _selectedCommChannel.Id == value.Id && 
                _selectedCommChannel.Name == value.Name &&
                _selectedCommChannel.Description == value.Description)
            {
                OnPropertyChanged();
                return;
            }

            _selectedCommChannel = value;
            OnPropertyChanged();

            if (SelectedConnectionType != ConnectionType.TcpIp)
                _ipAddress = "";
            
            OnPropertyChanged(nameof(IpAddress));
            OnPropertyChanged(nameof(IsIpAddressVisible));
            OnPropertyChanged(nameof(IsCommChannelsVisible));
            OnPropertyChanged(nameof(CanConnect));
        }
    }
    public ICommand DiscoverComm { get; }

    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            if (_ipAddress == value)
                return;
            _ipAddress = value;
            SelectedCommChannel = GetCommChannel(value);
            OnPropertyChanged();
        }
    }
    public int IpPort
    {
        get => _ipPort; // The value can be set if the device use a non-standard port. Port 0 will be interpreted by the device libraries as "use the default port".
        set
        {
            _ipPort = value;
            OnPropertyChanged();
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set
        {
            if (value == _connectionStatus) 
                return;
            _connectionStatus = value;
            OnPropertyChanged();
        }
    }
    public string ConnectedDeviceMake
    {
        get => _connectedDeviceMake;
        set
        {
            if (value == _connectedDeviceMake) 
                return;
            _connectedDeviceMake = value;
            OnPropertyChanged();
        }
    }
    public string ConnectedDeviceModel
    {
        get => _connectedDeviceModel;
        set
        {
            if (value == _connectedDeviceModel) 
                return;
            _connectedDeviceModel = value;
            OnPropertyChanged();
        }
    }

    public ICommand Connect { get; }
    public ICommand Disconnect { get; }
    public ICommand NavigateNext { get; }

    #region Visibility Properties
    public bool IsCommChannelsVisible => SelectedConnectionType != ConnectionType.TcpIp;
    public bool IsIpAddressVisible => SelectedConnectionType == ConnectionType.TcpIp;
    
    public bool CanSelectConnectionTypes => UiEntryEnabled && ConnectionTypes.Count != 0 && !IsConnected;
    public bool CanSelectCommChannels => UiEntryEnabled && CanSelectConnectionTypes && CommChannels.Count > 1 && !IsConnected;
    public bool CanDiscoverComm => UiEntryEnabled && CanSelectConnectionTypes && SelectedConnectionType != ConnectionType.Unknown && !IsConnected;
    public bool CanEnterIpAddress => UiEntryEnabled && CanSelectConnectionTypes && SelectedConnectionType != ConnectionType.Unknown && !IsConnected;

    public bool CanConnect => UiEntryEnabled && SelectedConnectionType != ConnectionType.Unknown && SelectedCommChannel != null && !IsConnected;
    public bool CanDisconnect => UiEntryEnabled && IsConnected;

    public bool CanGoToTransaction => UiEntryEnabled && IsConnected;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (value == _isConnected) 
                return;
            _isConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSelectConnectionTypes));
            OnPropertyChanged(nameof(CanSelectCommChannels));
            OnPropertyChanged(nameof(CanDiscoverComm));
            OnPropertyChanged(nameof(CanEnterIpAddress));
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanDisconnect));
            OnPropertyChanged(nameof(CanGoToTransaction));
            CanGoPreviousPage = !value;
        }
    }

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
            OnPropertyChanged(nameof(CanSelectConnectionTypes));
            OnPropertyChanged(nameof(CanSelectCommChannels));
            OnPropertyChanged(nameof(CanDiscoverComm));
            OnPropertyChanged(nameof(CanEnterIpAddress));
            OnPropertyChanged(nameof(CanDisconnect));
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanGoToTransaction));
        }
    }

    #endregion

    public string Error
    {
        get => _error;
        set
        {
            if (value == _error) 
                return;
            _error = value;
            OnPropertyChanged();
        }
    }

    public ConnectionViewModel(
        IDeviceService deviceService, 
        DiscoverCommCommand discoverComm,
        ConnectCommand connect,
        DisconnectCommand disconnect)
    {
        ConnectionTypes = [];
        ConnectionTypes.CollectionChanged += OnConnectionTypesCollectionChanged;

        CommChannels = [NoneSelectedChannel];
        CommChannels.CollectionChanged += OnCommChannelsCollectionChanged;

        DeviceService = deviceService;
        if (DeviceService is INotifyPropertyChanged deviceServiceNotifier) 
            deviceServiceNotifier.PropertyChanged += OnDeviceChanged;

        discoverComm.Starting += OnDiscoverCommStarting;
        discoverComm.Finished += OnDiscoverCommFinished;
        DiscoverComm = discoverComm;

        connect.Starting += OnConnectStarting;
        connect.Finished += OnConnectFinished;
        Connect = connect;

        disconnect.Starting += OnDisconnectStarting;
        disconnect.Finished += OnDisconnectFinished;
        Disconnect = disconnect;

        NavigateNext = new Command(OnNavigateToNextPage);

        OnDeviceChanged(); // Populate connection types.
    }

    private void OnConnectionTypesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanSelectConnectionTypes));
    }

    private void OnCommChannelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanSelectCommChannels));
    }

    private static async void OnNavigateToNextPage()
    {
        await Shell.Current.GoToAsync(nameof(TransactionPage));
    }

    private void OnDeviceChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs is not DeviceServiceChangedEventArgs deviceServiceEventArgs)
            return;

        if (deviceServiceEventArgs.PreviousDeviceInstance is ICommManager previousCommManager)
            previousCommManager.DeviceConnectedChanged -= OnDeviceConnectedChanged;

        OnDeviceChanged();
    }

    private void OnDeviceChanged()
    {
        CommChannels.Clear();
        CommChannels.Add(NoneSelectedChannel);
        SelectedCommChannel = CommChannels.First();
        ConnectionTypes.Clear();
        IpAddress = "";
        
        var device = DeviceService.Device;
        if (device == null)
            return;
        
        if (device is not ICommManager commManager)
        {
            Error = "The device library doesn't support communication management functionality.";
            return;
        }

        commManager.DeviceConnectedChanged += OnDeviceConnectedChanged;

        foreach (var connectionType in commManager.SupportedConnectionTypes)
            ConnectionTypes.Add(connectionType);
    }

    private void OnDeviceConnectedChanged(object sender, DeviceConnectedEventArgs eventArgs)
    {
        if (eventArgs.IsConnected)
        {
            ConnectionStatus = "Connected";
            IsConnected = true;
        }
        else
        {
            ConnectedDeviceMake = "";
            ConnectedDeviceModel = "";
            ConnectionStatus = "Disconnected";
            IsConnected = false;
        }
    }

    private ICommChannel? GetCommChannel(string value)
    {
        return TryGetIpAddress(value, out var ipAddress) switch
        {
            ValidationResult.Valid => new CommChannel { Id = IpPort != 0 ? $"{ipAddress}:{IpPort}" : ipAddress?.ToString() ?? "" },
            ValidationResult.Invalid => null,
            ValidationResult.NotEntered => null,
            _ => SelectedCommChannel
        };
    }
    public enum ValidationResult { NotEntered, Valid, Invalid }
    private static ValidationResult TryGetIpAddress(string ipAddressStr, out IPAddress? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddressStr))
        {
            ipAddress = null;
            return ValidationResult.NotEntered;
        }

        if (ipAddressStr.Count(ch => ch == '.') != 3 && !ipAddressStr.Contains(":") ||
            !IPAddress.TryParse(ipAddressStr, out var parsedIpAddress))
        {
            ipAddress = null;
            return ValidationResult.Invalid;
        }
        ipAddress = parsedIpAddress;
        return ValidationResult.Valid;
    }
    
    private void OnDiscoverCommStarting(object? sender, EventArgs eventArgs)
    {
        UiEntryEnabled = false;
        Error = "";
    }
    private void OnDiscoverCommFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        try
        {
            var response = eventArgs.Response;
            if (response.IsFailure)
            {
                Error = response.Message;
                return;
            }

            CommChannels.Clear();
            CommChannels.Add(NoneSelectedChannel);
            SelectedCommChannel = CommChannels.First();
            if (response.Data is not IReadOnlyCollection<ICommChannel> channels)
                return;

            foreach (var channel in channels) 
                CommChannels.Add(channel);
        }
        finally
        {
            UiEntryEnabled = true;
        }
    }

    private void OnConnectStarting(object? sender, EventArgs eventArgs)
    {
        UiEntryEnabled = false;
        Error = "";
        ConnectedDeviceMake = "";
        ConnectedDeviceModel = "";
        ConnectionStatus = "Connecting";
    }
    private void OnConnectFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        try
        {
            var response = eventArgs.Response;
            if (response.IsFailure)
            {
                Error = response.Message;
                ConnectedDeviceMake = "";
                ConnectedDeviceModel = "";
                ConnectionStatus = "Disconnected";
                return;
            }

            var deviceInfo = response.Data as SlimCDTypeLib.IDeviceInfo;
            ConnectedDeviceMake = deviceInfo?.Make ?? "";
            ConnectedDeviceModel = deviceInfo?.Model ?? "";
        }
        finally
        {
            UiEntryEnabled = true;
        }
    }

    private void OnDisconnectStarting(object? sender, EventArgs e)
    {
        UiEntryEnabled = false;
        Error = "";
    }
    private void OnDisconnectFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        UiEntryEnabled = true;
    }

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion
}