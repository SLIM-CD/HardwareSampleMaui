using HardwareSampleMaui.ViewModels;

namespace HardwareSampleMaui.Pages;

public partial class TransactionPage : ContentPage
{
	public TransactionPage(TransactionViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}