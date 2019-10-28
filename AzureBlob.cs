using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace azureblob
{
	internal sealed class AzureBlob
	{
		private CloudBlobContainer _blobContainer;
		private BlobRequestOptions _requestOptions;

		public AzureBlob(string connectionString, string blobContainer)
		{
			var storageAccount = CloudStorageAccount.Parse(connectionString);
			var blobClient = storageAccount.CreateCloudBlobClient();
			this._blobContainer = blobClient.GetContainerReference(blobContainer);
			this._blobContainer.CreateIfNotExists();
			this._requestOptions = new BlobRequestOptions();
			this._blobContainer.CreateIfNotExists(this._requestOptions);
		}

		public async Task UploadFileAsync(string filePath, CancellationToken cancellationToken)
		{
			if (IsDirectory(filePath))
			{
				Console.WriteLine($"{filePath} is a directory");
				return;
			}

			Console.WriteLine($"Uploading file {filePath}");
			var newBlob = this._blobContainer.GetBlockBlobReference(filePath);
			await newBlob.UploadFromFileAsync(filePath, null, this._requestOptions, null, cancellationToken);
		}

		public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken)
		{
			if (IsDirectory(filePath))
			{
				Console.WriteLine($"{filePath} is a directory");
				return;
			}

			Console.WriteLine($"Deleting file {filePath}");
			var blob = await this._blobContainer.GetBlobReferenceFromServerAsync(filePath, cancellationToken);
			await blob.DeleteIfExistsAsync(cancellationToken);
		}

		private static bool IsDirectory(string path)
		{
			FileAttributes fa = File.GetAttributes(path);
			return (fa & FileAttributes.Directory) != 0;
		}
	}
}
