using System.Data.Entity;
using FileStorageMVC.Core.Entities;

namespace FileStorageMVC.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection")
        {
        }

        public DbSet<FileRecord> Files { get; set; }
    }
}
