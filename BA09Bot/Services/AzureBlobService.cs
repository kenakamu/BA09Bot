using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace BA09Bot.Services
{
    public class AzureBlobService : IAzureBlobService
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

        public AzureBlobService()
        {
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("imagecontainer");
            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
        }

        public string Upload(string base64Image, string name)
        {
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("imagecontainer");

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            CloudBlockBlob blob = container.GetBlockBlobReference(name);
            blob.Properties.ContentType = "image/png";
            blob.UploadFromByteArray(imageBytes, 0, imageBytes.Length);
            return blob.StorageUri.PrimaryUri.AbsoluteUri;
        }
    }
}