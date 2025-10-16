using LocationHeatMapApp.Helpers;
using LocationHeatMapApp.Models;
using LocationHeatMapApp.ViewModels;

namespace LocationHeatMapApp.Views
{
    /// <summary>
    /// Main view for displaying the heat map and controls
    /// </summary>
    public partial class MainView : ContentPage
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Initializes the main view with its view model
        /// </summary>
        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Subscribe to heat map refresh events
            _viewModel.HeatMapRefreshRequested += OnHeatMapRefreshRequested;
        }

        /// <summary>
        /// Handles the page appearing event
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Request location permissions
            await RequestLocationPermissionAsync();

            // Load initial heat map data
            await RefreshHeatMapAsync();
        }

        /// <summary>
        /// Handles the page disappearing event
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Clean up event subscription
            _viewModel.HeatMapRefreshRequested -= OnHeatMapRefreshRequested;
        }

        /// <summary>
        /// Requests location permissions from the user
        /// </summary>
        private async Task RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Denied",
                            "Location permission is required to track your location.",
                            "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    $"Failed to request location permission: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Handles heat map refresh requests from the view model
        /// </summary>
        private void OnHeatMapRefreshRequested(object sender, List<LocationPoint> locations)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                HeatMapHelper.CreateHeatMap(HeatMap, locations, 100);
            });
        }

        /// <summary>
        /// Refreshes the heat map with current data
        /// </summary>
        private async Task RefreshHeatMapAsync()
        {
            // Trigger refresh through view model
            if (_viewModel.RefreshHeatMapCommand.CanExecute(null))
            {
                _viewModel.RefreshHeatMapCommand.Execute(null);
            }
        }
    }
}