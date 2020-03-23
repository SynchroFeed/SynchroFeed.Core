using System;
using Newtonsoft.Json;

namespace SynchroFeed.Repository.Npm.Model
{
    public class NpmPackageRepository
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}