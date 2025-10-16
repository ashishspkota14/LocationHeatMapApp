using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LocationHeatMapApp.Data;
using LocationHeatMapApp.Models;
using LocationHeatMapApp.Services;

namespace LocationHeatMapApp.ViewModels
{
    /// <summary>
    /// ViewModel for the main view, handling location tracking and UI state
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly LocationService _locationService;
        private readonly DatabaseService _databaseService;
        private bool _isTracking;
        private int _locationCount;
        private string _statusMessage;

        /// <summary>
        /// Event for property change notifications
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event raised when heat map needs to be refreshed
        /// </summary>
        public event EventHandler<List<LocationPoint>> HeatMapRefreshRequested;

        /// <summary>
        /// Gets or sets whether location tracking is active
        /// </summary>
        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                _isTracking = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TrackingButtonText));
            }
        }

        /// <summary>
        /// Gets the text for the tracking button based on current state
        /// </summary>
        public string TrackingButtonText => IsTracking ? "Stop Tracking" : "Start Tracking";

        /// <summary>
        /// Gets or sets the count of stored locations
        /// </summary>
        public int LocationCount
        {
            get => _locationCount;
            set
            {
                _locationCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the status message displayed to the user
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command to toggle location tracking
        /// </summary>
        public ICommand ToggleTrackingCommand { get; }

        /// <summary>
        /// Command to refresh the heat map display
        /// </summary>
        public ICommand RefreshHeatMapCommand { get; }

        /// <summary>
        /// Command to clear all stored locations
        /// </summary>
        public ICommand ClearDataCommand { get; }

        /// <summary>
        /// Initializes the view model and commands
        /// </summary>
        public MainViewModel(LocationService locationService, DatabaseService databaseService)
        {
            _locationService = locationService;
            _databaseService = databaseService;

            // Subscribe to location updates
            _locationService.LocationUpdated += OnLocationUpdated;

            // Initialize commands
            ToggleTrackingCommand = new Command(async () => await ToggleTrackingAsync());
            RefreshHeatMapCommand = new Command(async () => await RefreshHeatMapAsync());
            ClearDataCommand = new Command(async () => await ClearDataAsync());

            // Initialize UI
            StatusMessage = "Ready to track";
            _ = UpdateLocationCountAsync();
        }

        /// <summary>
        /// Toggles location tracking on/off
        /// </summary>
        private async Task ToggleTrackingAsync()
        {
            if (IsTracking)
            {
                // Stop tracking
                _locationService.StopTracking();
                IsTracking = false;
                StatusMessage = "Tracking stopped";
            }
            else
            {
                // Start tracking
                StatusMessage = "Starting tracking...";
                IsTracking = true;

                // Start tracking in background (every 10 seconds)
                _ = Task.Run(async () => await _locationService.StartTrackingAsync(10));

                StatusMessage = "Tracking active";
            }
        }

        /// <summary>
        /// Handles location update events
        /// </summary>
        private async void OnLocationUpdated(object sender, LocationPoint location)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                StatusMessage = $"Location updated: {location.Latitude:F6}, {location.Longitude:F6}";
                await UpdateLocationCountAsync();
                await RefreshHeatMapAsync();
            });
        }

        /// <summary>
        /// Updates the location count from the database
        /// </summary>
        private async Task UpdateLocationCountAsync()
        {
            LocationCount = await _databaseService.GetLocationCountAsync();
        }

        /// <summary>
        /// Refreshes the heat map by loading all locations
        /// </summary>
        private async Task RefreshHeatMapAsync()
        {
            var locations = await _databaseService.GetLocationsAsync();
            HeatMapRefreshRequested?.Invoke(this, locations);
            StatusMessage = $"Heat map updated with {locations.Count} points";
        }

        /// <summary>
        /// Clears all stored location data after confirmation
        /// </summary>
        private async Task ClearDataAsync()
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Clear Data",
                "Are you sure you want to delete all location data?",
                "Yes",
                "No");

            if (answer)
            {
                await _databaseService.ClearAllLocationsAsync();
                await UpdateLocationCountAsync();
                await RefreshHeatMapAsync();
                StatusMessage = "All data cleared";
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}