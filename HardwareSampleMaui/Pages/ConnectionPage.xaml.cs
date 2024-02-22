using HardwareSampleMaui.ViewModels;

namespace HardwareSampleMaui.Pages;

public partial class ConnectionPage : ContentPage
{
	public ConnectionPage(ConnectionViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}