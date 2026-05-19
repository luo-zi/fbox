namespace Fbox.Shared.Models;

public class FileEntry
{
    public int Id { get; init; }
    public string Path { get; init; } = "";
    public string FileName => System.IO.Path.GetFileName(Path);
    public string AddedAt { get; init; } = "";
    public bool Seen { get; init; }
}
