using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace azureblob
{
	internal sealed class FileWatcher : IDisposable
	{
		private FileSystemWatcher _watcher;
		private BufferBlock<FileChange> _fileChanges;
		private readonly string _baseFullPath;

		public FileWatcher(string monitoredDirectory)
		{
			this._watcher = new FileSystemWatcher(monitoredDirectory);
			this._fileChanges = new BufferBlock<FileChange>();
			this._baseFullPath = Path.GetFullPath(monitoredDirectory);

			this._watcher.NotifyFilter = NotifyFilters.LastWrite |
				NotifyFilters.Size |
				 NotifyFilters.FileName |
				NotifyFilters.DirectoryName;
			this._watcher.IncludeSubdirectories = true;
			this._watcher.Changed += this.OnFileChanged;
			this._watcher.Created += this.OnFileChanged;
			this._watcher.Renamed += this.OnFileRenamed;
			this._watcher.Deleted += this.OnFileChanged;
			this._watcher.Error += this.OnFileWatchError;
			this._watcher.EnableRaisingEvents = true;
		}

		public void Dispose()
		{
			this._fileChanges.LinkTo(DataflowBlock.NullTarget<FileChange>());
			this._fileChanges.Complete();
			this._watcher.EnableRaisingEvents = false;
			this._watcher.Changed -= this.OnFileChanged;
			this._watcher.Created -= this.OnFileChanged;
			this._watcher.Renamed -= this.OnFileRenamed;
			this._watcher.Deleted -= this.OnFileChanged;
			this._watcher.Error -= this.OnFileWatchError;
		}

		public async Task<FileChange> GetFileChangeAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("Try to get an item.");
			return await this._fileChanges.ReceiveAsync(cancellationToken);
		}

		// Last access
		private void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			 WatcherChangeTypes wct = e.ChangeType;
			 Console.WriteLine($"File {e.FullPath} {wct.ToString()}");

			 if ((wct == WatcherChangeTypes.Created) || (wct == WatcherChangeTypes.Changed))
			 {
				 var relativePath = e.FullPath.Substring(this._baseFullPath.Length + 1);
				 Console.WriteLine($"Relative path: {relativePath}");

				 this._fileChanges.Post(new FileChange(FileChange.ChangeType.Upload, relativePath, e.FullPath));
			 }
			 else if (wct == WatcherChangeTypes.Deleted)
			 {
				 var relativePath = e.FullPath.Substring(this._baseFullPath.Length + 1);
				 Console.WriteLine($"Relative path: {relativePath}");

				 this._fileChanges.Post(new FileChange(FileChange.ChangeType.Delete, relativePath, e.FullPath));
			 }
		}

		private void OnFileRenamed(object sender, RenamedEventArgs e)
		{
			Console.WriteLine($"File {e.OldFullPath} Renamed to {e.FullPath}");

			var oldRelativePath = e.OldFullPath.Substring(this._baseFullPath.Length + 1);
			Console.WriteLine($"Old relative path: {oldRelativePath}");

			var newRelativePath = e.FullPath.Substring(this._baseFullPath.Length + 1);
			Console.WriteLine($"New relative path: {newRelativePath}");

			this._fileChanges.Post(new FileChange(FileChange.ChangeType.Delete, oldRelativePath, e.OldFullPath));
			this._fileChanges.Post(new FileChange(FileChange.ChangeType.Upload, newRelativePath, e.FullPath));
		}

		private void OnFileWatchError(object sender, ErrorEventArgs e)
		{
			Console.Error.WriteLine($"file watch exception: {e.GetException().ToString()}");
		}
	}
}
