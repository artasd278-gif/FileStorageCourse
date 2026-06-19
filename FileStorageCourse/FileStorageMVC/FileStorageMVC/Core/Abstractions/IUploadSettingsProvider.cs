namespace FileStorageMVC.Core.Abstractions
{
    public interface IUploadSettingsProvider
    {
        int GetMaxFileSizeBytes();
        string GetUploadFolderPath();
    }
}
