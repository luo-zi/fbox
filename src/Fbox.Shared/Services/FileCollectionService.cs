using Fbox.Shared.Data;
using Fbox.Shared.Models;

namespace Fbox.Shared.Services;

public class FileCollectionService
{
    private readonly FileRepository _repo;

    public FileCollectionService()
    {
        var db = new DatabaseManager();
        _repo = new FileRepository(db);
    }

    public void AddFiles(IEnumerable<string> paths) => _repo.AddRange(paths);

    public List<FileEntry> GetAllFiles() => _repo.GetAllAndMarkSeen();

    public List<FileEntry> GetNewFiles() => _repo.GetNewAndMarkSeen();

    public List<FileEntry> GetAllFilesUnmarked() => _repo.GetAll();

    public List<string> GetPaths() => _repo.GetPathsOnly();

    public (int total, int unseen) GetCount() => _repo.GetCount();

    public void Clear() => _repo.Clear();

    public void Remove(int id) => _repo.Remove(id);
}
