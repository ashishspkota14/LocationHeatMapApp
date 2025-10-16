using SQLite;
using LocationHeatMapApp.Models;

namespace LocationHeatMapApp.Data
{
    /// <summary>
    /// Service for managing SQLite database operations
    /// </summary>
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        /// <summary>
        /// Initializes the database service and creates tables
        /// </summary>
        /// <param name="dbPath">Path to the SQLite database file</param>
        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<LocationPoint>().Wait();
        }

        /// <summary>
        /// Retrieves all location points from the database
        /// </summary>
        /// <returns>List of all location points</returns>
        public Task<List<LocationPoint>> GetLocationsAsync()
        {
            return _database.Table<LocationPoint>().ToListAsync();
        }

        /// <summary>
        /// Retrieves location points within a specific date range
        /// </summary>
        /// <param name="startDate">Start date for filtering</param>
        /// <param name="endDate">End date for filtering</param>
        /// <returns>Filtered list of location points</returns>
        public Task<List<LocationPoint>> GetLocationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return _database.Table<LocationPoint>()
                           .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                           .ToListAsync();
        }

        /// <summary>
        /// Saves a new location point to the database
        /// </summary>
        /// <param name="location">Location point to save</param>
        /// <returns>Number of rows affected</returns>
        public Task<int> SaveLocationAsync(LocationPoint location)
        {
            return _database.InsertAsync(location);
        }

        /// <summary>
        /// Deletes all location points from the database
        /// </summary>
        /// <returns>Number of rows deleted</returns>
        public Task<int> ClearAllLocationsAsync()
        {
            return _database.DeleteAllAsync<LocationPoint>();
        }

        /// <summary>
        /// Gets the count of stored location points
        /// </summary>
        /// <returns>Total count of location points</returns>
        public Task<int> GetLocationCountAsync()
        {
            return _database.Table<LocationPoint>().CountAsync();
        }
    }
}