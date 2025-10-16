namespace LocationHeatMapApp
{
    /// <summary>
    /// Main application class
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the application
        /// </summary>
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}