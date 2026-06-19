using System.Collections.Generic;
using System.IO;
using FileStorageMVC.Core.Entities;

namespace FileStorageMVC.Application.Abstractions
{
    public interface IFileStorageService
    {
        UploadPageData GetUploadPageData(string errorCode = null);
        UploadResult Upload(UploadFileRequest request);
        IEnumerable<FileRecord> GetAllFiles();
        DeleteResult Delete(int id);
        DownloadResult GetDownload(int id);
    }

    public class UploadPageData
    {
        public string ErrorMessage { get; set; }
        public string MaxFileSizeText { get; set; }
    }

    public class UploadFileRequest
    {
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get; set; }
        public Stream ContentStream { get; set; }
    }

    public class UploadResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? UploadedFileId { get; set; }
        public string MaxFileSizeText { get; set; }
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
