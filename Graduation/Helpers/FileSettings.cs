using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Graduation.Helpers
{
    public class FileSettings
    {
        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=imagegraduation;AccountKey=MhlQR9SQN8g5tIYILvaYDYFP7527actJVfOyY+NSEipssl39Kbq4s2r31goSEXzjyA0/LV2ORSUD+AStwEYDKw==;EndpointSuffix=core.windows.net";
        private static string containerName = "graduation";

        public static async Task<string> UploadFileAsync(IFormFile file)
        {
            // Initialize the BlobContainerClient
            var container = new BlobContainerClient(connectionString, containerName);
            await container.CreateIfNotExistsAsync();

            // Ensure the container is publicly accessible
            await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            // Generate a unique blob name
            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = container.GetBlobClient(blobName);

            // Determine the correct Content-Type based on the file extension
            string contentType = file.ContentType;

            // If Content-Type is not provided, infer it from the file extension
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = GetContentType(file.FileName);
            }

            // Upload the file with the correct Content-Type
            using (var stream = file.OpenReadStream())
            {
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType // Set the correct Content-Type
                };
                await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
            }

            // Return the URI of the uploaded blob
            return blobClient.Uri.AbsoluteUri;
        }

        public static async Task<string> DeleteFileAsync(string fileName)
        {
            var container = new BlobContainerClient(connectionString, containerName);
            var blobClient = container.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
                return "Deleted";
            }
            return "File not found";
        }

        // Helper method to infer Content-Type based on file extension
        private static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                default:
                    return "application/octet-stream"; 
            }
        }
    }
}