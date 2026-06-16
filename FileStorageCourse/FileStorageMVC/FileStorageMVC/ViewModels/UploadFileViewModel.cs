namespace FileStorageMVC.ViewModels
{
    public class UploadFileViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? UploadedFileId { get; set; }
        public string MaxFileSizeText { get; set; }
    }
}
