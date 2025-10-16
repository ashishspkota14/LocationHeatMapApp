using LocationHeatMapApp.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace LocationHeatMapApp.Helpers
{
    /// <summary>
    /// Helper class for rendering heat maps on maps
    /// </summary>
    public static class HeatMapHelper
    {
        /// <summary>
        /// Creates circle overlays to represent location density
        /// </summary>
        /// <param name="map">Map control to add overlays to</param>
        /// <param name="locations">List of location points</param>
        /// <param name="radius">Radius of each heat point in meters</param>
        public static void CreateHeatMap(Map map, List<LocationPoint> locations, double radius = 50)
        {
            // Clear existing map elements
            map.MapElements.Clear();

            if (locations == null || !locations.Any())
                return;

            // Group nearby locations to create density
            var groupedLocations = GroupNearbyLocations(locations, 0.001); // ~100 meters

            foreach (var group in groupedLocations)
            {
                var intensity = CalculateIntensity(group.Count, groupedLocations.Max(g => g.Count));
                var color = GetHeatColor(intensity);

                // Create a circle for each heat point
                var circle = new Circle
                {
                    Center = new Location(group.Latitude, group.Longitude),
                    Radius = new Distance(radius),
                    StrokeColor = color,
                    StrokeWidth = 2,
                    FillColor = color.WithAlpha(0.3f)
                };

                map.MapElements.Add(circle);
            }

            // Center map on the locations
            CenterMapOnLocations(map, locations);
        }

        /// <summary>
        /// Groups nearby locations to create density clusters
        /// </summary>
        private static List<LocationGroup> GroupNearbyLocations(List<LocationPoint> locations, double threshold)
        {
            var groups = new List<LocationGroup>();

            foreach (var location in locations)
            {
                var existingGroup = groups.FirstOrDefault(g =>
                    Math.Abs(g.Latitude - location.Latitude) < threshold &&
                    Math.Abs(g.Longitude - location.Longitude) < threshold);

                if (existingGroup != null)
                {
                    existingGroup.Count++;
                }
                else
                {
                    groups.Add(new LocationGroup
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Count = 1
                    });
                }
            }

            return groups;
        }

        /// <summary>
        /// Calculates intensity based on point count
        /// </summary>
        private static double CalculateIntensity(int count, int maxCount)
        {
            return Math.Min((double)count / maxCount, 1.0);
        }

        /// <summary>
        /// Gets a color based on heat intensity (blue -> green -> yellow -> red)
        /// </summary>
        private static Color GetHeatColor(double intensity)
        {
            if (intensity < 0.25)
            {
                // Blue to Cyan
                var ratio = intensity / 0.25;
                return Color.FromRgb(0, (int)(255 * ratio), 255);
            }
            else if (intensity < 0.5)
            {
                // Cyan to Green
                var ratio = (intensity - 0.25) / 0.25;
                return Color.FromRgb(0, 255, (int)(255 * (1 - ratio)));
            }
            else if (intensity < 0.75)
            {
                // Green to Yellow
                var ratio = (intensity - 0.5) / 0.25;
                return Color.FromRgb((int)(255 * ratio), 255, 0);
            }
            else
            {
                // Yellow to Red
                var ratio = (intensity - 0.75) / 0.25;
                return Color.FromRgb(255, (int)(255 * (1 - ratio)), 0);
            }
        }

        /// <summary>
        /// Centers the map on all locations with appropriate zoom
        /// </summary>
        private static void CenterMapOnLocations(Map map, List<LocationPoint> locations)
        {
            if (!locations.Any())
                return;

            var minLat = locations.Min(l => l.Latitude);
            var maxLat = locations.Max(l => l.Latitude);
            var minLon = locations.Min(l => l.Longitude);
            var maxLon = locations.Max(l => l.Longitude);

            var centerLat = (minLat + maxLat) / 2;
            var centerLon = (minLon + maxLon) / 2;

            var latDelta = Math.Abs(maxLat - minLat);
            var lonDelta = Math.Abs(maxLon - minLon);

            // Calculate appropriate distance for zoom
            var distance = Math.Max(latDelta, lonDelta) * 111000; // Convert to meters
            distance = Math.Max(distance, 500); // Minimum 500 meters

            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(centerLat, centerLon),
                Distance.FromMeters(distance * 1.5)));
        }

        /// <summary>
        /// Helper class for grouping locations
        /// </summary>
        private class LocationGroup
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int Count { get; set; }
        }
    }
} 