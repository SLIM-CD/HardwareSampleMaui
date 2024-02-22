namespace HardwareSampleMaui;

public partial class App : Application
{

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

#if WINDOWS
        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 600;
        private const string WINDOW_TITLE = "Hardware Sample MAUI App";

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Activated += OnWindowActivated;
            return window;
        }
        
        private async void OnWindowActivated(object? sender, EventArgs e)
        {
            if (sender is not Window window) 
                return;

            window.Title = WINDOW_TITLE;

            // Resize the window
            window.Width = WINDOW_WIDTH;
            window.Height = WINDOW_HEIGHT;

            // Yield for the window to finish resizing
            await window.Dispatcher.DispatchAsync(() => { });

            // Center the window
            //var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            //window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
            //window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;
        }
#endif
}