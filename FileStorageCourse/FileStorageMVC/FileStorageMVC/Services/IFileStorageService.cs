using System.Collections.Generic;
using System.Web;
using FileStorageMVC.Models;
using FileStorageMVC.ViewModels;

namespace FileStorageMVC.Services
{
    public interface IFileStorageService
    {
        UploadFileViewModel BuildUploadViewModel(string errorCode = null);
        UploadFileViewModel Upload(HttpPostedFileBase file);
        IEnumerable<FileRecord> GetAllFiles();
        DeleteResult Delete(int id);
        DownloadResult GetDownload(int id);
    }

    public class DeleteResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class DownloadResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string FullPath { get; set; }
        public string ContentType { get; set; }
        public string OriginalFileName { get; set; }
    }
}
