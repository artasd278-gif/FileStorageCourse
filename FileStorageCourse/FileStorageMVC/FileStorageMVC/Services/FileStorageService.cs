using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;
using FileStorageMVC.Models;
using FileStorageMVC.Repositories;
using FileStorageMVC.ViewModels;

namespace FileStorageMVC.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileRepository _repository;
        private readonly string _uploadFolder;
        private readonly string _maxFileSizeRaw;

        public FileStorageService(IFileRepository repository)
        {
            _repository = repository;
            _uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];
            _maxFileSizeRaw = ConfigurationManager.AppSettings["MaxFileSizeBytes"];
        }

        public UploadFileViewModel BuildUploadViewModel(string errorCode = null)
        {
            var vm = new UploadFileViewModel
            {
                MaxFileSizeText = FormatFileSizeLimit(GetMaxFileSizeBytes())
            };

            if (!string.Equals(errorCode, "maxsize", StringComparison.OrdinalIgnoreCase))
            {
                return vm;
            }

            vm.IsSuccess = false;
            vm.Message = "Файл слишком большой. Максимум " + vm.MaxFileSizeText + ".";
            return vm;
        }

        public UploadFileViewModel Upload(HttpPostedFileBase file)
        {
            var vm = BuildUploadViewModel();
            int maxFileSize = GetMaxFileSizeBytes();

            if (file == null || file.ContentLength <= 0)
            {
                vm.IsSuccess = false;
                vm.Message = "Выберите файл.";
                return vm;
            }

            if (file.ContentLength > maxFileSize)
            {
                vm.IsSuccess = false;
                vm.Message = "Файл слишком большой. Максимум " + vm.MaxFileSizeText + ".";
                return vm;
            }

            string savedFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string serverFolder = ResolveUploadFolderPath();
            string fullPath = Path.Combine(serverFolder, savedFileName);

            try
            {
                if (!Directory.Exists(serverFolder))
                {
                    Directory.CreateDirectory(serverFolder);
                }

                file.SaveAs(fullPath);
            }
            catch
            {
                vm.IsSuccess = false;
                vm.Message = "Ошибка сохранения файла. Проверьте путь UploadFolder в Web.config.";
                return vm;
            }

            try
            {
                var fileRecord = new FileRecord
                {
                    OriginalFileName = file.FileName,
                    SavedFileName = savedFileName,
                    ContentType = file.ContentType,
                    Size = file.ContentLength,
                    UploadDate = DateTime.Now
                };

                int newId = _repository.AddFile(fileRecord);

                vm.IsSuccess = true;
                vm.UploadedFileId = newId;
                vm.Message = "Файл загружен. Id = " + newId;
                return vm;
            }
            catch
            {
                // Компенсация: если БД не сохранилась, удаляем физический файл.
                SafeDelete(fullPath);
                vm.IsSuccess = false;
                vm.Message = "Ошибка записи в базу данных. Загрузка отменена.";
                return vm;
            }
        }

        public System.Collections.Generic.IEnumerable<FileRecord> GetAllFiles()
        {
            return _repository.GetAllFiles();
        }

        public DeleteResult Delete(int id)
        {
            var fileRecord = _repository.GetFileById(id);
            if (fileRecord == null)
            {
                return new DeleteResult { IsSuccess = false, Message = "Файл для удаления не найден." };
            }

            string fullPath = Path.Combine(ResolveUploadFolderPath(), fileRecord.SavedFileName);
            string tempPath = fullPath + ".deleting-" + Guid.NewGuid();
            bool movedToTemp = false;

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Move(fullPath, tempPath);
                    movedToTemp = true;
                }

                _repository.DeleteFile(id);

                if (movedToTemp && File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                return new DeleteResult { IsSuccess = true, Message = "Файл успешно удалён." };
            }
            catch
            {
                // Компенсация: возвращаем файл на место, если БД/операция упала.
                if (movedToTemp && File.Exists(tempPath) && !File.Exists(fullPath))
                {
                    File.Move(tempPath, fullPath);
                }

                return new DeleteResult { IsSuccess = false, Message = "Ошибка при удалении файла." };
            }
        }

        public DownloadResult GetDownload(int id)
        {
            var fileRecord = _repository.GetFileById(id);
            if (fileRecord == null)
            {
                return new DownloadResult { IsSuccess = false, Message = "Файл не найден в базе данных." };
            }

            string fullPath = Path.Combine(ResolveUploadFolderPath(), fileRecord.SavedFileName);
            if (!File.Exists(fullPath))
            {
                return new DownloadResult { IsSuccess = false, Message = "Файл найден в базе, но отсутствует на диске." };
            }

            return new DownloadResult
            {
                IsSuccess = true,
                FullPath = fullPath,
                ContentType = string.IsNullOrWhiteSpace(fileRecord.ContentType) ? "application/octet-stream" : fileRecord.ContentType,
                OriginalFileName = string.IsNullOrWhiteSpace(fileRecord.OriginalFileName) ? fileRecord.SavedFileName : fileRecord.OriginalFileName
            };
        }

        private int GetMaxFileSizeBytes()
        {
            int parsed;
            return int.TryParse(_maxFileSizeRaw, out parsed) && parsed > 0
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
            string FallbackPath()
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Uploads");
            }

            if (string.IsNullOrWhiteSpace(_uploadFolder))
            {
                return HostingEnvironment.MapPath("~/App_Data/Uploads") ?? FallbackPath();
            }

            if (Path.IsPathRooted(_uploadFolder))
            {
                return _uploadFolder;
            }

            if (_uploadFolder.StartsWith("~"))
            {
                return HostingEnvironment.MapPath(_uploadFolder) ?? FallbackPath();
            }

            string normalized = _uploadFolder.TrimStart('/', '\\').Replace("\\", "/");
            return HostingEnvironment.MapPath("~/" + normalized) ?? FallbackPath();
        }

        private static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Логирование можно добавить на следующем этапе.
            }
        }
    }
}
