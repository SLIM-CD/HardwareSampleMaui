namespace HardwareSampleMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        #if WINDOWS
        window.Activated += OnWindowActivated;
        #endif

        return window;
    }

    #if WINDOWS
    private const int WINDOW_WIDTH = 400;
    private const int WINDOW_HEIGHT = 600;
    private const string WINDOW_TITLE = "Hardware Sample MAUI App";
    private static async void OnWindowActivated(object? sender, EventArgs e)
    {
        if (sender is not Window window) 
            return;

        window.Title = WINDOW_TITLE;

        // Resize the window
        window.Width = WINDOW_WIDTH;
        window.Height = WINDOW_HEIGHT;

        // Yield for the window to finish resizing
        await window.Dispatcher.DispatchAsync(() => { });

        #pragma warning disable S125 // Commented-out code
        // This is how you can center the window
        // var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
        // window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
        // window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;
        #pragma warning restore S125 // Commented-out code
    }
    #endif
}