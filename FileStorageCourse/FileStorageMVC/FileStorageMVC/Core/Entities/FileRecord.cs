using System;

namespace FileStorageMVC.Core.Entities
{
    public class FileRecord
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; }
        public string SavedFileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
