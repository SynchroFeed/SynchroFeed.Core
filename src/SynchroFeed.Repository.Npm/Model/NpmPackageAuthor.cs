using System;
using Newtonsoft.Json;

namespace SynchroFeed.Repository.Npm.Model
{
    public class NpmPackageAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}