using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FileStorageMVC.Application.Abstractions;
using FileStorageMVC.Core.Abstractions;
using FileStorageMVC.Core.Entities;

namespace FileStorageMVC.Application.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileRepository _repository;
        private readonly IUploadSettingsProvider _settingsProvider;

        public FileStorageService(IFileRepository repository, IUploadSettingsProvider settingsProvider)
        {
            _repository = repository;
            _settingsProvider = settingsProvider;
        }

        public UploadPageData GetUploadPageData(string errorCode = null)
        {
            var pageData = new UploadPageData
            {
                MaxFileSizeText = FormatFileSizeLimit(_settingsProvider.GetMaxFileSizeBytes())
            };

            if (string.Equals(errorCode, "maxsize", StringComparison.OrdinalIgnoreCase))
            {
                pageData.ErrorMessage = "Файл слишком большой. Максимум " + pageData.MaxFileSizeText + ".";
            }

            return pageData;
        }

        public UploadResult Upload(UploadFileRequest request)
        {
            var pageData = GetUploadPageData();
            var result = new UploadResult
            {
                MaxFileSizeText = pageData.MaxFileSizeText
            };

            int maxFileSize = _settingsProvider.GetMaxFileSizeBytes();
            if (request == null || request.ContentLength <= 0 || request.ContentStream == null)
            {
                result.IsSuccess = false;
                result.Message = "Выберите файл.";
                return result;
            }

            if (request.ContentLength > maxFileSize)
            {
                result.IsSuccess = false;
                result.Message = "Файл слишком большой. Максимум " + pageData.MaxFileSizeText + ".";
                return result;
            }

            string extension = Path.GetExtension(request.OriginalFileName ?? string.Empty);
            string savedFileName = Guid.NewGuid() + extension;
            string uploadFolder = _settingsProvider.GetUploadFolderPath();
            string fullPath = Path.Combine(uploadFolder, savedFileName);

            try
            {
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                if (request.ContentStream.CanSeek)
                {
                    request.ContentStream.Position = 0;
                }
                using (var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    request.ContentStream.CopyTo(outputStream);
                }
            }
            catch
            {
                result.IsSuccess = false;
                result.Message = "Ошибка сохранения файла. Проверьте путь UploadFolder в Web.config.";
                return result;
            }

            try
            {
                var fileRecord = new FileRecord
                {
                    OriginalFileName = request.OriginalFileName,
                    SavedFileName = savedFileName,
                    ContentType = request.ContentType,
                    Size = request.ContentLength,
                    UploadDate = DateTime.Now
                };

                int newId = _repository.AddFile(fileRecord);
                result.IsSuccess = true;
                result.UploadedFileId = newId;
                result.Message = "Файл загружен. Id = " + newId;
                return result;
            }
            catch
            {
                SafeDelete(fullPath);
                result.IsSuccess = false;
                result.Message = "Ошибка записи в базу данных. Загрузка отменена.";
                return result;
            }
        }

        public IEnumerable<FileRecord> GetAllFiles()
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

            string fullPath = Path.Combine(_settingsProvider.GetUploadFolderPath(), fileRecord.SavedFileName);
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

            string fullPath = Path.Combine(_settingsProvider.GetUploadFolderPath(), fileRecord.SavedFileName);
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

        private static string FormatFileSizeLimit(int bytes)
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
                // Логирование можно добавить отдельным адаптером.
            }
        }
    }
}
