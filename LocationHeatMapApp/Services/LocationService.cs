using LocationHeatMapApp.Data;
using LocationHeatMapApp.Models;

namespace LocationHeatMapApp.Services
{
    /// <summary>
    /// Service for tracking and managing user location
    /// </summary>
    public class LocationService
    {
        private readonly DatabaseService _databaseService;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isTracking;

        /// <summary>
        /// Event raised when a new location is received
        /// </summary>
        public event EventHandler<LocationPoint> LocationUpdated;

        /// <summary>
        /// Gets whether location tracking is currently active
        /// </summary>
        public bool IsTracking => _isTracking;

        /// <summary>
        /// Initializes the location service
        /// </summary>
        /// <param name="databaseService">Database service for storing locations</param>
        public LocationService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Starts continuous location tracking
        /// </summary>
        /// <param name="intervalSeconds">Interval between location updates in seconds</param>
        public async Task StartTrackingAsync(int intervalSeconds = 10)
        {
            if (_isTracking)
                return;

            _isTracking = true;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                while (_isTracking && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await TrackCurrentLocationAsync();
                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when tracking is stopped
            }
        }

        /// <summary>
        /// Stops location tracking
        /// </summary>
        public void StopTracking()
        {
            _isTracking = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Gets the current location once and saves it
        /// </summary>
        private async Task TrackCurrentLocationAsync()
        {
            try
            {
                // Request location permissions if not already granted
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        return;
                    }
                }

                // Get current location with high accuracy
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    var locationPoint = new LocationPoint
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Timestamp = DateTime.UtcNow,
                        Accuracy = location.Accuracy
                    };

                    // Save to database
                    await _databaseService.SaveLocationAsync(locationPoint);

                    // Notify subscribers
                    LocationUpdated?.Invoke(this, locationPoint);
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Location is not supported on this device
            }
            catch (PermissionException)
            {
                // Location permission was denied
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                System.Diagnostics.Debug.WriteLine($"Error tracking location: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the last known location without requesting a new one
        /// </summary>
        /// <returns>Last known location or null</returns>
        public async Task<Location> GetLastKnownLocationAsync()
        {
            try
            {
                return await Geolocation.GetLastKnownLocationAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}