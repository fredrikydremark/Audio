namespace Scada
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        /*
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        */
        
        protected override Window CreateWindow(IActivationState ?activationState)
        {
            Window myWindow = new Window(new AppShell());
            if (OperatingSystem.IsWindows())
            {
                myWindow.Width = 1000;
                myWindow.Height = 600;
            }
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            myWindow.IsMaximizable = true;
            myWindow.X = (displayInfo.Width / displayInfo.Density - myWindow.Width) / 2;
            myWindow.Y = (displayInfo.Height / displayInfo.Density - myWindow.Height) / 2;
            return myWindow;
        }
    }
}