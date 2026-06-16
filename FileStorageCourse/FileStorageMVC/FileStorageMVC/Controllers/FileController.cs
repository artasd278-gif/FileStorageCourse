using System.Web;
using System.Web.Mvc;
using FileStorageMVC.Repositories;
using FileStorageMVC.Services;
using FileStorageMVC.ViewModels;

namespace FileStorageMVC.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileStorageService _fileStorageService = new FileStorageService(new FileRepository());

        // GET: File/UploadFile
        public ActionResult UploadFile()
        {
            var vm = _fileStorageService.BuildUploadViewModel(Request.QueryString["error"]);
            return View(vm);
        }

        // POST: File/UploadFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            var vm = _fileStorageService.Upload(file);
            return View(vm);
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
