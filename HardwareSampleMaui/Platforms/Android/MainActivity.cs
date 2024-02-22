using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using HardwareSampleMaui.Services;

// ReSharper disable once CheckNamespace
namespace HardwareSampleMaui;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    ConfigurationChanges = 
        ConfigChanges.ScreenSize | 
        ConfigChanges.Orientation | 
        ConfigChanges.UiMode | 
        ConfigChanges.ScreenLayout | 
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        PlatformSpecific.AndroidContext = this;
        
        IDTech.Maui.Comm.IDTechBinding.Init(this); // In a case of IDTechNeo.Maui

        base.OnCreate(savedInstanceState);

        Platform.Init(this, savedInstanceState);

        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            RequestPermissions([
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessNetworkState,
                Manifest.Permission.BluetoothPrivileged,
                Manifest.Permission.Bluetooth,
                Manifest.Permission.BluetoothAdmin,
                Manifest.Permission.ModifyAudioSettings,
                Manifest.Permission.RecordAudio,
                Manifest.Permission.ReadExternalStorage
            ], 0);
        }

        IDTech.Maui.Comm.IDTechBinding.enableUSB();

        // Those references needed for the linking not to throw the entire assemblies
        _ = typeof(IDTechNeo.IDTechNeoHal).FullName;
        _ = typeof(IngenicoUpp.IngenicoUppHal).FullName;
    }
}