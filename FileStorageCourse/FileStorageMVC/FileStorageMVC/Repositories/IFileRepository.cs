using FileStorageMVC.Models;
using System.Collections.Generic;

namespace FileStorageMVC.Repositories
{
    public interface IFileRepository
    {
        int AddFile(FileRecord file);
        FileRecord GetFileById(int id);
        IEnumerable<FileRecord> GetAllFiles();
    }
}
