using System.Web.Mvc;
using FileStorageMVC.Application.Abstractions;
using FileStorageMVC.Application.Services;
using FileStorageMVC.Core.Abstractions;
using FileStorageMVC.Infrastructure.Configuration;
using FileStorageMVC.Infrastructure.Repositories;

namespace FileStorageMVC.App_Start
{
    public static class DependencyConfig
    {
        public static void RegisterDependencies()
        {
            var resolver = new SimpleDependencyResolver();

            resolver.Register<IFileRepository>(() => new FileRepository());
            resolver.Register<IUploadSettingsProvider>(() => new WebConfigUploadSettingsProvider());
            resolver.Register<IFileStorageService>(() =>
                new FileStorageService(
                    (IFileRepository)resolver.GetService(typeof(IFileRepository)),
                    (IUploadSettingsProvider)resolver.GetService(typeof(IUploadSettingsProvider))));

            DependencyResolver.SetResolver(resolver);
        }
    }
}
