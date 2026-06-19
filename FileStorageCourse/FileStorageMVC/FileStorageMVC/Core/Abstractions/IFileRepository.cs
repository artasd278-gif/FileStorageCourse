using System.Collections.Generic;
using FileStorageMVC.Core.Entities;

namespace FileStorageMVC.Core.Abstractions
{
    public interface IFileRepository
    {
        int AddFile(FileRecord file);
        FileRecord GetFileById(int id);
        IEnumerable<FileRecord> GetAllFiles();
        void DeleteFile(int id);
    }
}
