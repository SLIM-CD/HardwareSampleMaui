using HardwareSampleMaui.ViewModels;

namespace HardwareSampleMaui.Pages;

public partial class DevicePage : ContentPage
{
    public DevicePage(DeviceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}