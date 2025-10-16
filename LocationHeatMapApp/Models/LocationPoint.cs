using SQLite;

namespace LocationHeatMapApp.Models
{
    /// <summary>
    /// Represents a location point stored in the database
    /// </summary>
    [Table("locations")]
    public class LocationPoint
    {
        /// <summary>
        /// Primary key for the location record
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Latitude coordinate of the location
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude coordinate of the location
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Timestamp when the location was recorded
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Accuracy of the location reading in meters
        /// </summary>
        public double? Accuracy { get; set; }
    }
}