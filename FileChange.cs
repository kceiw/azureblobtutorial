using System;
using System.IO;

namespace azureblob
{
	internal sealed class FileChange
	{
		public enum ChangeType
		{
			Upload,
			Delete,
		}

		public ChangeType Type { get; }
		public string Name { get; }
		public string FilePath { get; }

		public FileChange(ChangeType type, string name, string path)
		{
			this.Type = type;
			this.Name = name;
			this.FilePath = path;
		}
	}
}

