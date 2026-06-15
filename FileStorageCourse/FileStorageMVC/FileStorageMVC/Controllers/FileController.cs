using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using FileStorageMVC.Models;
using FileStorageMVC.Repositories;
using System.Collections.Generic;

namespace FileStorageMVC.Controllers
{
    public class FileController : Controller
    {
        private readonly string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];
        private readonly int maxFileSize = int.Parse(ConfigurationManager.AppSettings["MaxFileSizeBytes"]);

        // GET: File/UploadFile
        public ActionResult UploadFile()
        {
            return View();
        }

        // POST: File/UploadFile
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Выберите файл.";
                return View();
            }

            if (file.ContentLength > maxFileSize)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = $"Файл слишком большой. Максимум {maxFileSize / 1024 / 1024} MB.";
                return View();
            }

            string savedFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string serverFolder = Server.MapPath(uploadFolder);

            if (!Directory.Exists(serverFolder))
                Directory.CreateDirectory(serverFolder);

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
                return HttpNotFound("Файл не найден.");
            }

            string serverFolder = Server.MapPath(uploadFolder);
            string fullPath = Path.Combine(serverFolder, fileRecord.SavedFileName);
            if (!System.IO.File.Exists(fullPath))
            {
                return HttpNotFound("Физический файл отсутствует.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, fileRecord.ContentType, fileRecord.OriginalFileName);
        }
    }
}
