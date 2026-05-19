using Microsoft.Data.Sqlite;

namespace Fbox.Shared.Data;

public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager()
    {
        var dbDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "fbox");
        Directory.CreateDirectory(dbDir);
        var dbPath = Path.Combine(dbDir, "fbox.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public void Initialize()
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS files (
                id    INTEGER PRIMARY KEY AUTOINCREMENT,
                path  TEXT    NOT NULL UNIQUE,
                added TEXT    NOT NULL DEFAULT (datetime('now','localtime')),
                seen  INTEGER NOT NULL DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS idx_files_seen ON files(seen);
            """;
        cmd.ExecuteNonQuery();
    }
}
