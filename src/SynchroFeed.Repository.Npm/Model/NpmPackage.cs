using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SynchroFeed.Repository.Npm.Model
{
    // Define other methods, classes and namespaces here
	public class NpmPackage
	{
		public const string HashKey = "shasum";
		public const string ArchiveKey = "tarball";

		[JsonProperty("_id")] public string Id { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("main")] public string Main { get; set; }
		[JsonProperty("author")] public NpmPackageAuthor Author { get; set; }
		[JsonProperty("license")] public string License { get; set; }
		[JsonProperty("dependencies")] public Dictionary<string, string> Dependencies { get; set; }
		[JsonProperty("publishConfig")] public NpmPackagePublishConfig PublishConfig { get; set; }
		//[JsonProperty("repository")] public string Repository { get; set; }
		[JsonProperty("githead")] public string Githead { get; set; }
		[JsonProperty("dist")] public Dictionary<string, string> Dist { get; set; }
		[JsonProperty("version")] public string Version { get; set; }
	}
}
