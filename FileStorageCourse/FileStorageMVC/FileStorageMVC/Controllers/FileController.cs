using System.Web;
using System.Web.Mvc;
using FileStorageMVC.Application.Abstractions;
using FileStorageMVC.ViewModels;

namespace FileStorageMVC.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileStorageService _fileStorageService;

        public FileController()
            : this((IFileStorageService)DependencyResolver.Current.GetService(typeof(IFileStorageService)))
        {
        }

        public FileController(IFileStorageService fileStorageService)
        {
            if (fileStorageService == null)
            {
                throw new System.ArgumentNullException("fileStorageService");
            }

            _fileStorageService = fileStorageService;
        }

        // GET: File/UploadFile
        public ActionResult UploadFile()
        {
            var pageData = _fileStorageService.GetUploadPageData(Request.QueryString["error"]);
            return View(new UploadFileViewModel
            {
                IsSuccess = false,
                Message = pageData.ErrorMessage,
                MaxFileSizeText = pageData.MaxFileSizeText
            });
        }

        // POST: File/UploadFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            UploadFileRequest request = null;
            if (file != null)
            {
                request = new UploadFileRequest
                {
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    ContentLength = file.ContentLength,
                    ContentStream = file.InputStream
                };
            }

            var result = _fileStorageService.Upload(request);
            return View(new UploadFileViewModel
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message,
                UploadedFileId = result.UploadedFileId,
                MaxFileSizeText = result.MaxFileSizeText
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFile(int id)
        {
            var result = _fileStorageService.Delete(id);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction("List");
        }

        // GET: File/List
        public ActionResult List()
        {
            var vm = new FileListViewModel
            {
                Files = _fileStorageService.GetAllFiles(),
                ErrorMessage = TempData["ErrorMessage"] as string,
                SuccessMessage = TempData["SuccessMessage"] as string
            };

            return View(vm);
        }

        // GET: File/DownloadFile/{id}
        public ActionResult DownloadFile(int id)
        {
            var download = _fileStorageService.GetDownload(id);
            if (!download.IsSuccess)
            {
                TempData["ErrorMessage"] = download.Message;
                return RedirectToAction("List");
            }

            return File(download.FullPath, download.ContentType, download.OriginalFileName);
        }
    }
}
