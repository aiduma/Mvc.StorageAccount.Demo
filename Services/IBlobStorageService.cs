
namespace Mvc.StorageAccount.Demo.Services
{
    public interface IBlobStorageService
    {
        string GetBlobUrl(string imageName);
        Task RemoveBlob(string imageName);
        Task<string> UploadBlob(IFormFile formFile, string imageName);
    }
}