using Microsoft.Data.Sqlite;
using Fbox.Shared.Models;

namespace Fbox.Shared.Data;

public class FileRepository
{
    private readonly DatabaseManager _db;

    public FileRepository(DatabaseManager db)
    {
        _db = db;
        _db.Initialize();
    }

    public void Add(string path)
    {
        var normalized = System.IO.Path.GetFullPath(path);
        using var conn = _db.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO files (path, seen) VALUES (@path, 0)
            ON CONFLICT(path) DO UPDATE SET seen = CASE WHEN seen=1 THEN 0 ELSE seen END
            """;
        cmd.Parameters.AddWithValue("@path", normalized);
        cmd.ExecuteNonQuery();
    }

    public void AddRange(IEnumerable<string> paths)
    {
        foreach (var path in paths)
            Add(path);
    }

    public List<FileEntry> GetAll()
    {
        using var conn = _db.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, path, added, seen FROM files ORDER BY added DESC";
        return ReadEntries(cmd);
    }

    public List<FileEntry> GetAllAndMarkSeen()
    {
        using var conn = _db.CreateConnection();
        using var tx = conn.BeginTransaction();

        var files = new List<FileEntry>();
        using (var selectCmd = conn.CreateCommand())
        {
            selectCmd.CommandText = "SELECT id, path, added, seen FROM files ORDER BY added DESC";
            selectCmd.Transaction = tx;
            files = ReadEntries(selectCmd);
        }

        using (var updateCmd = conn.CreateCommand())
        {
            updateCmd.CommandText = "UPDATE files SET seen = 1";
            updateCmd.Transaction = tx;
            updateCmd.ExecuteNonQuery();
        }

        tx.Commit();
        return files;
    }

    public List<FileEntry> GetNewAndMarkSeen()
    {
        using var conn = _db.CreateConnection();
        using var tx = conn.BeginTransaction();

        var files = new List<FileEntry>();
        using (var selectCmd = conn.CreateCommand())
        {
            selectCmd.CommandText = "SELECT id, path, added, seen FROM files WHERE seen = 0 ORDER BY added DESC";
            selectCmd.Transaction = tx;
            files = ReadEntries(selectCmd);
        }

        using (var updateCmd = conn.CreateCommand())
        {
            updateCmd.CommandText = "UPDATE files SET seen = 1";
            updateCmd.Transaction = tx;
            updateCmd.ExecuteNonQuery();
        }

        tx.Commit();
        return files;
    }

    public List<string> GetPathsOnly()
    {
        using var conn = _db.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT path FROM files ORDER BY added DESC";
        var paths = new List<string>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            paths.Add(reader.GetString(0));
        return paths;
    }

    public (int total, int unseen) GetCount()
    {
        using var conn = _db.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*), COALESCE(SUM(CASE WHEN seen=0 THEN 1 ELSE 0 END), 0) FROM files";
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (reader.GetInt32(0), reader.GetInt32(1));
        return (0, 0);
    }

    public void Clear()
    {
        using var conn = _db.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM files";
        cmd.ExecuteNonQuery();
    }

    public void Remove(int id)
    {
        using var conn = _db.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM files WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static List<FileEntry> ReadEntries(SqliteCommand cmd)
    {
        var files = new List<FileEntry>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            files.Add(new FileEntry
            {
                Id = reader.GetInt32(0),
                Path = reader.GetString(1),
                AddedAt = reader.GetString(2),
                Seen = reader.GetInt32(3) == 1
            });
        }
        return files;
    }
}
