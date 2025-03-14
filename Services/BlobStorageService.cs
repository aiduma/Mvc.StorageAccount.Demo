using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mvc.StorageAccount.Demo.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private string containerName = "attendeeimages";

        public BlobStorageService(IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadBlob(IFormFile formFile, string imageName)
        {
            var blobName = $"{imageName}";
            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            using var memoryStream = new MemoryStream();

            formFile.CopyTo(memoryStream);
            memoryStream.Position = 0;

            if (formFile != null)
            {
                container.DeleteBlobIfExists(imageName);
            }

            var client = await container.UploadBlobAsync(blobName, memoryStream, default);
            return blobName;
        }

        public string GetBlobUrl(string imageName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(imageName);

            BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blob.BlobContainerName,
                BlobName = blob.Name,
                ExpiresOn = DateTime.UtcNow.AddMinutes(2),
                Protocol = SasProtocol.Https,
                Resource = "b"
            };
            blobSasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            return blob.GenerateSasUri(blobSasBuilder).ToString();
        }

        public async Task RemoveBlob(string imageName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(imageName);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        //private async Task<BlobContainerClient> GetBlobContainerClient()
        //{
        //    try
        //    {
        //        BlobContainerClient container = new BlobContainerClient(_configuration["StorageConnectionString"], containerName);
        //        await container.CreateIfNotExistsAsync();

        //        return container;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
    }
}
