using System.Collections.Generic;
using System.Linq;
using FileStorageMVC.Core.Abstractions;
using FileStorageMVC.Core.Entities;
using FileStorageMVC.Infrastructure.Data;

namespace FileStorageMVC.Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        public int AddFile(FileRecord file)
        {
            using (var db = new AppDbContext())
            {
                db.Files.Add(file);
                db.SaveChanges();
                return file.Id;
            }
        }

        public FileRecord GetFileById(int id)
        {
            using (var db = new AppDbContext())
            {
                return db.Files.Find(id);
            }
        }

        public IEnumerable<FileRecord> GetAllFiles()
        {
            using (var db = new AppDbContext())
            {
                return db.Files
                    .OrderByDescending(f => f.UploadDate)
                    .ToList();
            }
        }

        public void DeleteFile(int id)
        {
            using (var db = new AppDbContext())
            {
                var file = db.Files.Find(id);
                if (file == null)
                {
                    return;
                }

                db.Files.Remove(file);
                db.SaveChanges();
            }
        }
    }
}
