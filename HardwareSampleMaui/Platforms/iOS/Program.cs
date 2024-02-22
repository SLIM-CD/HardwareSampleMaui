using UIKit;

// ReSharper disable once CheckNamespace
namespace HardwareSampleMaui;

public class Program
{
    private static void Main(string[] args)
    {
        IDTech.Maui.Comm.IDTechBinding.Init();

        UIApplication.Main(args, null, typeof(AppDelegate));

        // Those references needed for the linking not to throw the entire assemblies
        _ = typeof(IDTechNeo.IDTechNeoHal).FullName;
        _ = typeof(IngenicoUpp.IngenicoUppHal).FullName;
    }
}