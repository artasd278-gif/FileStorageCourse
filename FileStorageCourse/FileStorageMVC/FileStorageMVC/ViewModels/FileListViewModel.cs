using System.Collections.Generic;
using FileStorageMVC.Models;

namespace FileStorageMVC.ViewModels
{
    public class FileListViewModel
    {
        public IEnumerable<FileRecord> Files { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}
