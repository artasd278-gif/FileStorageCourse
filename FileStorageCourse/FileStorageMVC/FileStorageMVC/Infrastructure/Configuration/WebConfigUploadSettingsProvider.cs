using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using FileStorageMVC.Core.Abstractions;

namespace FileStorageMVC.Infrastructure.Configuration
{
    public class WebConfigUploadSettingsProvider : IUploadSettingsProvider
    {
        private const int DefaultMaxFileSizeBytes = 10 * 1024 * 1024;

        public int GetMaxFileSizeBytes()
        {
            string maxFileSizeRaw = ConfigurationManager.AppSettings["MaxFileSizeBytes"];
            int parsed;
            return int.TryParse(maxFileSizeRaw, out parsed) && parsed > 0
                ? parsed
                : DefaultMaxFileSizeBytes;
        }

        public string GetUploadFolderPath()
        {
            string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];

            string FallbackPath()
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Uploads");
            }

            if (string.IsNullOrWhiteSpace(uploadFolder))
            {
                return HostingEnvironment.MapPath("~/App_Data/Uploads") ?? FallbackPath();
            }

            if (Path.IsPathRooted(uploadFolder))
            {
                return uploadFolder;
            }

            if (uploadFolder.StartsWith("~"))
            {
                return HostingEnvironment.MapPath(uploadFolder) ?? FallbackPath();
            }

            string normalized = uploadFolder.TrimStart('/', '\\').Replace("\\", "/");
            return HostingEnvironment.MapPath("~/" + normalized) ?? FallbackPath();
        }
    }
}
