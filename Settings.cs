using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace azureblob
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class Settings
	{
		public string BlobConnectionString { get; set; }
		public string BlobContainer { get; set; }
		public string MonitoredDirectory { get; set; }
	}
}

