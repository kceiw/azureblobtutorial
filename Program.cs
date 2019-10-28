using Microsoft.Azure.Storage;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace azureblob
{
	class Program
	{
		static async Task Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("usage: Program [setting file]");
				return;
			}

			var settingFile = args[0];
			Settings settings = null;

			using (var stream = new StreamReader(settingFile))
			{
				var fileContents = await stream.ReadToEndAsync();
				settings = JsonConvert.DeserializeObject<Settings>(fileContents);
			}

			Console.WriteLine("Initializing");
			var blobContainer = settings.BlobContainer;
			var connectionString = settings.BlobConnectionString;
			var azureBlob = new AzureBlob(connectionString, blobContainer);

			if (!Directory.Exists(settings.MonitoredDirectory))
			{
				Console.WriteLine($"The Directory {settings.MonitoredDirectory} doesn't exist.");
				return;
			}

			var fileWatcher = new FileWatcher(settings.MonitoredDirectory);
			var cancellationToken = new CancellationToken();

			while (true)
			{
				var fileChange = await fileWatcher.GetFileChangeAsync(cancellationToken);

				try
				{
					switch (fileChange.Type)
					{
						case FileChange.ChangeType.Upload:
							await azureBlob.UploadFileAsync(fileChange.FilePath, cancellationToken);
							break;
						case FileChange.ChangeType.Delete:
							await azureBlob.DeleteFileAsync(fileChange.FilePath, cancellationToken);
							break;
					}
				}
				catch (StorageException e)
				{
					Console.Error.WriteLine($"Failed to operate on Azure Blob {e}");
				}
				catch (IOException e)
				{
					Console.Error.WriteLine($"Failed to operate on Azure Blob {e}");
				}
				catch (UnauthorizedAccessException e)
				{
					Console.Error.WriteLine($"Failed to operate on Azure Blob {e}");
				}
			}
		}
	}
}
