using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using HardwareSampleMaui.Commands;
using HardwareSampleMaui.Pages;
using HardwareSampleMaui.Services;
using HardwareSampleMaui.ViewModels;
using SlimCDDevice;
using System.Reflection;

#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace HardwareSampleMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseMauiCommunityToolkit();
        builder.Services
            //.AddSingleton(typeof(Services.ILogger), new Services.LoggerTcpIpService(new IPEndPoint(IPAddress.Parse("192.168.0.10"), 4080)))
            .AddSingleton(FolderPicker.Default)
            .AddSingleton<IDeviceService, DeviceService>()
            .AddSingleton<DiscoverAssembliesCommand>()
            .AddSingleton<SelectAssembliesPathCommand>()
            .AddSingleton<LoadSelectedAssemblyCommand>()
            .AddSingleton<DiscoverCommCommand>()
            .AddSingleton<ConnectCommand>()
            .AddSingleton<DisconnectCommand>()
            .AddSingleton<StartTransactionCommand>()
            .AddSingleton<CancelTransactionCommand>()
            .AddSingleton<PlatformSpecificService>()
            .AddSingleton<DevicePage>()
            .AddSingleton<DeviceViewModel>()
            .AddSingleton<ConnectionPage>()
            .AddSingleton<ConnectionViewModel>()
            .AddSingleton<TransactionPage>()
            .AddSingleton<TransactionViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Exclude the current app from device assemblies discovery.
        Assemblies.ExcludeAssemblies.Add(MethodBase.GetCurrentMethod()!.DeclaringType!.Assembly.GetName().Name);

        return builder.Build();
    }
}