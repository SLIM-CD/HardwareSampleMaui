using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Core.Primitives;
using HardwareSampleMaui.Commands;
using HardwareSampleMaui.Pages;
using HardwareSampleMaui.Services;
using SlimCDDevice;
using SlimCDTypeLib;

namespace HardwareSampleMaui.ViewModels;

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class DeviceViewModel : INotifyPropertyChanged
{
    private const string None = "None";

    #region Backing Fields
    private IAssemblyInfo? _selectedDeviceAssembly;
    private string _selectedAssembliesPath = "";
    private bool _canCancelAssembliesDiscovery;
    private bool _uiEntryEnabled = true;
    private string _loadedAssemblyName = None;
    private string _error = "";

    #endregion

    #region Assemblies Discovery Properties
    public ObservableCollection<IAssemblyInfo> DeviceAssemblies { get; } = [];
    public IAssemblyInfo? SelectedDeviceAssembly
    {
        get => _selectedDeviceAssembly;
        set
        {
            if (Equals(value, _selectedDeviceAssembly)) 
                return;
            _selectedDeviceAssembly = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanLoadSelectedAssembly));
        }
    }
    public ICommand DiscoverAssemblies { get; }

    public string SelectedAssembliesPath
    {
        get => _selectedAssembliesPath;
        set
        {
            if (value == _selectedAssembliesPath) 
                return;
            _selectedAssembliesPath = value;
            OnPropertyChanged();
        }
    }
    public ICommand SelectAssembliesPath { get; }

    #endregion

    #region Assembly Loading Properties
    private IDeviceService DeviceService { get; }
    public string LoadedAssemblyName
    {
        get => _loadedAssemblyName;
        set
        {
            if (value == _loadedAssemblyName)
                return;
            _loadedAssemblyName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGoToConnection));
        }
    }
    public ICommand LoadSelectedAssembly { get; }

    #endregion

    #region Visibility Properties
    public bool UiEntryEnabled
    {
        get => _uiEntryEnabled;
        set
        {
            if (value == _uiEntryEnabled) 
                return;
            _uiEntryEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSelectDeviceAssemblies));
            OnPropertyChanged(nameof(CanLoadSelectedAssembly));
            OnPropertyChanged(nameof(CanGoToConnection));
        }
    }
    public bool CanSelectDeviceAssemblies => UiEntryEnabled && DeviceAssemblies.Count > 0;
    public bool CanLoadSelectedAssembly => UiEntryEnabled && SelectedDeviceAssembly != null;
    public bool CanCancelAssembliesDiscovery
    {
        get => _canCancelAssembliesDiscovery;
        set
        {
            if (value == _canCancelAssembliesDiscovery)
                return;
            _canCancelAssembliesDiscovery = value;
            OnPropertyChanged();
        }
    }
    public bool CanGoToConnection => UiEntryEnabled && !string.IsNullOrEmpty(LoadedAssemblyName) && LoadedAssemblyName != None;
    public bool IsFolderSelectionSupported { get; } = 
        OperatingSystem.IsWindows() || 
        OperatingSystem.IsAndroid() || 
        OperatingSystem.IsIOSVersionAtLeast(11) || 
        OperatingSystem.IsMacCatalystVersionAtLeast(14);

    #endregion

    public ICommand NavigateNext { get; }

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

    public DeviceViewModel(
        IDeviceService deviceService,
        DiscoverAssembliesCommand discoverAssemblies, 
        SelectAssembliesPathCommand selectAssembliesPath,
        LoadSelectedAssemblyCommand loadSelectedAssembly)
    {
        DeviceService = deviceService;

        discoverAssemblies.Starting += OnAssembliesDiscoveryStarting;
        discoverAssemblies.Finished += OnAssembliesDiscoveryFinished;
        discoverAssemblies.CanCancelChanged += OnDiscoveryCanCancelChanged;
        DiscoverAssemblies = discoverAssemblies;
        DiscoverAssemblies.Execute(null);

        selectAssembliesPath.Starting += OnPathSelectionStarting;
        selectAssembliesPath.Finished += OnPathSelectionFinished;
        SelectAssembliesPath = selectAssembliesPath;

        loadSelectedAssembly.Starting += OnAssemblyLoadingStarting;
        loadSelectedAssembly.Finished += OnAssemblyLoadingFinished;
        LoadSelectedAssembly = loadSelectedAssembly;

        NavigateNext = new Command(OnNavigateToNextPage);
    }

    private static async void OnNavigateToNextPage()
    {
        await Shell.Current.GoToAsync(nameof(ConnectionPage));
    }

    #region Assemblies Discovery 
    private void OnAssembliesDiscoveryStarting(object? sender, EventArgs eventArgs)
    {
        UiEntryEnabled = false;
    }

    private void OnAssembliesDiscoveryFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        try
        {
            var previousSelectedAssemblyName = SelectedDeviceAssembly?.Name;
            SelectedDeviceAssembly = null;

            #region Check for Errors
            var response = eventArgs.Response;
            if (response.IsFailure)
            {
                Error = response.Message;
                return;
            }

            if (response.Data is not IReadOnlyCollection<IAssemblyInfo> assemblies || assemblies.Count == 0)
            {
                Error = "Couldn't discover any device assembly";
                return;
            }

            Error = "";

            #endregion

            SetDeviceAssemblies(assemblies);
            SetSelectedDeviceAssembly(previousSelectedAssemblyName);
        }
        finally
        {
            UiEntryEnabled = true;
        }
    }

    private void SetDeviceAssemblies(IEnumerable<IAssemblyInfo> deviceAssemblies)
    {
        DeviceAssemblies.Clear();
        foreach (var deviceAssembly in deviceAssemblies) 
            DeviceAssemblies.Add(deviceAssembly);
    }

    private void SetSelectedDeviceAssembly(string? selectedDeviceAssemblyName)
    {
        if (string.IsNullOrEmpty(selectedDeviceAssemblyName))
            return;

        foreach (var deviceAssembly in DeviceAssemblies)
        {
            if (!deviceAssembly.Name.Equals(selectedDeviceAssemblyName, StringComparison.OrdinalIgnoreCase))
                continue;
            SelectedDeviceAssembly = deviceAssembly;
            break;
        }
    }

    private void OnDiscoveryCanCancelChanged(object sender, BooleanEventArgs eventArgs)
    {
        CanCancelAssembliesDiscovery = eventArgs.Value;
    }

    #endregion

    #region Assemblies Path Selection
    private void OnPathSelectionStarting(object? sender, EventArgs eventArgs)
    {
        UiEntryEnabled = false;
    }

    private void OnPathSelectionFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        try
        {
            #region Check for Errors
            var response = eventArgs.Response;
            if (response.IsFailure)
            {
                Error = response.Message;
                return;
            }

            #endregion

            Error = "";

            SelectedAssembliesPath = response.Data is Folder folder && !string.IsNullOrEmpty(folder.Path)
                ? folder.Path
                : "";
        }
        finally
        {
            UiEntryEnabled = true;
        }
    }

    #endregion

    #region Assembly Loading
    private void OnAssemblyLoadingStarting(object? sender, EventArgs e)
    {
        UiEntryEnabled = false;
    }

    private void OnAssemblyLoadingFinished(object sender, CommandResponseEventArgs eventArgs)
    {
        try
        {
            #region Check for Errors
            var response = eventArgs.Response;
            if (response.IsFailure)
            {
                Error = response.Message;
                return;
            }

            #endregion

            Error = "";

            var selectedAssembly = SelectedDeviceAssembly;
            if (response.Data is not ISlimCDDeviceAsync device || selectedAssembly == null)
            {
                Error = "Invalid loaded assembly data";
                return;
            }
            
            DeviceService.SetDevice(device, selectedAssembly);
            LoadedAssemblyName = DeviceService.AssemblyInfo?.Name ?? "";
        }
        finally
        {
            UiEntryEnabled = true;
        }
    }

    #endregion

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion
}