using FileStorageMVC.Models;

namespace FileStorageMVC.Repositories
{
    public interface IFileRepository
    {
        int AddFile(FileRecord file);
        FileRecord GetFileById(int id);
    }
}
