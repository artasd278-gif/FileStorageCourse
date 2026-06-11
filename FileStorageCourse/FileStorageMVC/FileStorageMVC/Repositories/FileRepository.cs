using System.Linq;
using FileStorageMVC.Models;

namespace FileStorageMVC.Repositories
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
    }
}
