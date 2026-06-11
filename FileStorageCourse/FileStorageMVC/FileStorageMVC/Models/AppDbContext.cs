using System.Data.Entity;

namespace FileStorageMVC.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection") { }

        public DbSet<FileRecord> Files { get; set; }
    }
}
