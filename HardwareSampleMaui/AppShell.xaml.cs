using HardwareSampleMaui.Pages;

namespace HardwareSampleMaui;

// ReSharper disable once RedundantExtendsListEntry
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ConnectionPage), typeof(ConnectionPage));
        Routing.RegisterRoute(nameof(TransactionPage), typeof(TransactionPage));
    }
}