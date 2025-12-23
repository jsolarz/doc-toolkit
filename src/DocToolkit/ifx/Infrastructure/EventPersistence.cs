using System.Text.Json;
using DocToolkit.ifx.Events;
using Microsoft.Data.Sqlite;

namespace DocToolkit.ifx.Infrastructure;

/// <summary>
/// Handles event persistence to SQLite database.
/// </summary>
internal class EventPersistence : IDisposable
{
    private readonly string _connectionString;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the EventPersistence.
    /// </summary>
    /// <param name="dbPath">Path to SQLite database file</param>
    public EventPersistence(string? dbPath = null)
    {
        _connectionString = $"Data Source={dbPath ?? "events.db"};";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTableCommand = @"
            CREATE TABLE IF NOT EXISTS Events (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EventId TEXT NOT NULL UNIQUE,
                EventType TEXT NOT NULL,
                EventData TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Pending',
                RetryCount INTEGER NOT NULL DEFAULT 0,
                LastError TEXT,
                CreatedAt TEXT NOT NULL,
                ProcessedAt TEXT
            );

            CREATE INDEX IF NOT EXISTS IX_Events_EventType ON Events(EventType);
            CREATE INDEX IF NOT EXISTS IX_Events_Status ON Events(Status);
            CREATE INDEX IF NOT EXISTS IX_Events_Timestamp ON Events(Timestamp);
        ";

        using var command = new SqliteCommand(createTableCommand, connection);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Saves an event to the database.
    /// </summary>
    /// <param name="eventData">Event to save</param>
    public void SaveEvent(IEvent eventData)
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var insertCommand = @"
                INSERT INTO Events (EventId, EventType, EventData, Timestamp, Status, CreatedAt)
                VALUES (@EventId, @EventType, @EventData, @Timestamp, @Status, @CreatedAt)
            ";

            using var command = new SqliteCommand(insertCommand, connection);
            command.Parameters.AddWithValue("@EventId", eventData.EventId.ToString());
            command.Parameters.AddWithValue("@EventType", eventData.EventType);
            command.Parameters.AddWithValue("@EventData", JsonSerializer.Serialize(eventData));
            command.Parameters.AddWithValue("@Timestamp", eventData.Timestamp.ToString("O"));
            command.Parameters.AddWithValue("@Status", "Pending");
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));

            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Marks an event as processed.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    public void MarkEventProcessed(Guid eventId)
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var updateCommand = @"
                UPDATE Events 
                SET Status = 'Processed', ProcessedAt = @ProcessedAt
                WHERE EventId = @EventId
            ";

            using var command = new SqliteCommand(updateCommand, connection);
            command.Parameters.AddWithValue("@EventId", eventId.ToString());
            command.Parameters.AddWithValue("@ProcessedAt", DateTime.UtcNow.ToString("O"));

            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Marks an event as failed and increments retry count.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="error">Error message</param>
    /// <returns>New retry count</returns>
    public int MarkEventFailed(Guid eventId, string error)
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Get current retry count
            var selectCommand = "SELECT RetryCount FROM Events WHERE EventId = @EventId";
            using var selectCmd = new SqliteCommand(selectCommand, connection);
            selectCmd.Parameters.AddWithValue("@EventId", eventId.ToString());
            var retryCount = Convert.ToInt32(selectCmd.ExecuteScalar() ?? 0);

            var updateCommand = @"
                UPDATE Events 
                SET Status = 'Failed', 
                    RetryCount = RetryCount + 1,
                    LastError = @Error
                WHERE EventId = @EventId
            ";

            using var command = new SqliteCommand(updateCommand, connection);
            command.Parameters.AddWithValue("@EventId", eventId.ToString());
            command.Parameters.AddWithValue("@Error", error);

            command.ExecuteNonQuery();

            return retryCount + 1;
        }
    }

    /// <summary>
    /// Gets pending events that need to be processed.
    /// </summary>
    /// <param name="maxCount">Maximum number of events to retrieve</param>
    /// <returns>List of pending events</returns>
    public List<PersistedEvent> GetPendingEvents(int maxCount = 100)
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var selectCommand = @"
                SELECT EventId, EventType, EventData, Timestamp, RetryCount
                FROM Events
                WHERE Status = 'Pending'
                ORDER BY Timestamp ASC
                LIMIT @MaxCount
            ";

            using var command = new SqliteCommand(selectCommand, connection);
            command.Parameters.AddWithValue("@MaxCount", maxCount);

            var events = new List<PersistedEvent>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                events.Add(new PersistedEvent
                {
                    EventId = Guid.Parse(reader.GetString(0)),
                    EventType = reader.GetString(1),
                    EventData = reader.GetString(2),
                    Timestamp = DateTime.Parse(reader.GetString(3)),
                    RetryCount = reader.GetInt32(4)
                });
            }

            return events;
        }
    }

    /// <summary>
    /// Gets failed events that can be retried.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retries allowed</param>
    /// <param name="maxCount">Maximum number of events to retrieve</param>
    /// <returns>List of failed events that can be retried</returns>
    public List<PersistedEvent> GetFailedEventsForRetry(int maxRetries = 3, int maxCount = 100)
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var selectCommand = @"
                SELECT EventId, EventType, EventData, Timestamp, RetryCount
                FROM Events
                WHERE Status = 'Failed' AND RetryCount < @MaxRetries
                ORDER BY Timestamp ASC
                LIMIT @MaxCount
            ";

            using var command = new SqliteCommand(selectCommand, connection);
            command.Parameters.AddWithValue("@MaxRetries", maxRetries);
            command.Parameters.AddWithValue("@MaxCount", maxCount);

            var events = new List<PersistedEvent>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                events.Add(new PersistedEvent
                {
                    EventId = Guid.Parse(reader.GetString(0)),
                    EventType = reader.GetString(1),
                    EventData = reader.GetString(2),
                    Timestamp = DateTime.Parse(reader.GetString(3)),
                    RetryCount = reader.GetInt32(4)
                });
            }

            return events;
        }
    }

    /// <summary>
    /// Resets failed events back to pending status for retry.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    public void ResetEventForRetry(Guid eventId)
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var updateCommand = @"
                UPDATE Events 
                SET Status = 'Pending', LastError = NULL
                WHERE EventId = @EventId
            ";

            using var command = new SqliteCommand(updateCommand, connection);
            command.Parameters.AddWithValue("@EventId", eventId.ToString());

            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Gets the count of pending events.
    /// </summary>
    /// <returns>Number of pending events</returns>
    public int GetPendingEventCount()
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var countCommand = "SELECT COUNT(*) FROM Events WHERE Status = 'Pending'";
            using var command = new SqliteCommand(countCommand, connection);
            return Convert.ToInt32(command.ExecuteScalar() ?? 0);
        }
    }

    /// <summary>
    /// Gets the count of failed events.
    /// </summary>
    /// <returns>Number of failed events</returns>
    public int GetFailedEventCount()
    {
        lock (_lockObject)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var countCommand = "SELECT COUNT(*) FROM Events WHERE Status = 'Failed'";
            using var command = new SqliteCommand(countCommand, connection);
            return Convert.ToInt32(command.ExecuteScalar() ?? 0);
        }
    }

    public void Dispose()
    {
        // SQLite connections are disposed automatically
    }
}

/// <summary>
/// Represents a persisted event from the database.
/// </summary>
internal class PersistedEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int RetryCount { get; set; }
}
