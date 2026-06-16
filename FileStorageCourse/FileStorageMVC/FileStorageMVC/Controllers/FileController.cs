using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using FileStorageMVC.Models;
using FileStorageMVC.Repositories;
using System.Collections.Generic;
using System.Globalization;

namespace FileStorageMVC.Controllers
{
    public class FileController : Controller
    {
        private readonly string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];

        private int GetMaxFileSizeBytes()
        {
            int parsed;
            return int.TryParse(ConfigurationManager.AppSettings["MaxFileSizeBytes"], out parsed) && parsed > 0
                ? parsed
                : 10 * 1024 * 1024;
        }

        private string FormatFileSizeLimit(int bytes)
        {
            if (bytes >= 1024 * 1024)
            {
                return (bytes / 1024d / 1024d).ToString("0.##", CultureInfo.InvariantCulture) + " МБ";
            }

            if (bytes >= 1024)
            {
                return (bytes / 1024d).ToString("0.##", CultureInfo.InvariantCulture) + " КБ";
            }

            return bytes + " Б";
        }

        private string ResolveUploadFolderPath()
        {
            if (string.IsNullOrWhiteSpace(uploadFolder))
            {
                return Server.MapPath("~/App_Data/Uploads");
            }

            if (Path.IsPathRooted(uploadFolder))
            {
                return uploadFolder;
            }

            if (uploadFolder.StartsWith("~"))
            {
                return Server.MapPath(uploadFolder);
            }

            string normalized = uploadFolder.TrimStart('/', '\\').Replace("\\", "/");
            return Server.MapPath("~/" + normalized);
        }

        private void PrepareUploadViewData()
        {
            ViewBag.MaxFileSizeText = FormatFileSizeLimit(GetMaxFileSizeBytes());
        }

        // GET: File/UploadFile
        public ActionResult UploadFile()
        {
            PrepareUploadViewData();
            if (string.Equals(Request.QueryString["error"], "maxsize", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Файл слишком большой. Максимум " + ViewBag.MaxFileSizeText + ".";
            }

            return View();
        }

        // POST: File/UploadFile
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            PrepareUploadViewData();
            int maxFileSize = GetMaxFileSizeBytes();

            if (file == null || file.ContentLength == 0)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Выберите файл.";
                return View();
            }

            if (file.ContentLength > maxFileSize)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Файл слишком большой. Максимум " + FormatFileSizeLimit(maxFileSize) + ".";
                return View();
            }

            try
            {
                string savedFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string serverFolder = ResolveUploadFolderPath();

                if (!Directory.Exists(serverFolder))
                {
                    Directory.CreateDirectory(serverFolder);
                }

                string fullPath = Path.Combine(serverFolder, savedFileName);
                file.SaveAs(fullPath);

                var fileRecord = new FileRecord
                {
                    OriginalFileName = file.FileName,
                    SavedFileName = savedFileName,
                    ContentType = file.ContentType,
                    Size = file.ContentLength,
                    UploadDate = DateTime.Now
                };

                var repo = new FileRepository();
                int newId = repo.AddFile(fileRecord);

                ViewBag.IsSuccess = true;
                ViewBag.UploadedFileId = newId;
                ViewBag.Message = $"Файл загружен. Id = {newId}";
                return View();
            }
            catch
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Ошибка сохранения файла. Проверьте путь UploadFolder в Web.config.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFile(int id)
        {
            var repo = new FileRepository();
            var fileRecord = repo.GetFileById(id);
            if (fileRecord == null)
            {
                TempData["ErrorMessage"] = "Файл для удаления не найден.";
                return RedirectToAction("List");
            }

            try
            {
                string serverFolder = ResolveUploadFolderPath();
                string fullPath = Path.Combine(serverFolder, fileRecord.SavedFileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                repo.DeleteFile(id);
                TempData["SuccessMessage"] = "Файл успешно удалён.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Ошибка при удалении файла.";
            }

            return RedirectToAction("List");
        }

        // GET: File/List
        public ActionResult List()
        {
            var repo = new FileRepository();
            IEnumerable<FileRecord> files = repo.GetAllFiles();
            return View(files);
        }

        // GET: File/DownloadFile/{id}
        public ActionResult DownloadFile(int id)
        {
            var repo = new FileRepository();
            var fileRecord = repo.GetFileById(id);
            if (fileRecord == null)
            {
                TempData["ErrorMessage"] = "Файл не найден в базе данных.";
                return RedirectToAction("List");
            }

            string serverFolder = ResolveUploadFolderPath();
            string fullPath = Path.Combine(serverFolder, fileRecord.SavedFileName);
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "Файл найден в базе, но отсутствует на диске.";
                return RedirectToAction("List");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, fileRecord.ContentType, fileRecord.OriginalFileName);
        }
    }
}
