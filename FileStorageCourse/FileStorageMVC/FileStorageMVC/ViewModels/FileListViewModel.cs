using System.Collections.Generic;
using FileStorageMVC.Core.Entities;

namespace FileStorageMVC.ViewModels
{
    public class FileListViewModel
    {
        public IEnumerable<FileRecord> Files { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}
