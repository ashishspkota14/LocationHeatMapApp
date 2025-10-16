using LocationHeatMapApp.Data;
using LocationHeatMapApp.Services;
using LocationHeatMapApp.ViewModels;
using LocationHeatMapApp.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace LocationHeatMapApp
{
    /// <summary>
    /// Main program class for configuring the MAUI application
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MAUI application
        /// </summary>
        /// <returns>Configured MauiApp instance</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register database service as singleton
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
            builder.Services.AddSingleton<DatabaseService>(s => new DatabaseService(dbPath));

            // Register location service as singleton
            builder.Services.AddSingleton<LocationService>();

            // Register view model and view
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}