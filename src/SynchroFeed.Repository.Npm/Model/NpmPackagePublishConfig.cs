using System;
using Newtonsoft.Json;

namespace SynchroFeed.Repository.Npm.Model
{
    public class NpmPackagePublishConfig
    {
        [JsonProperty("registry")]
        public string Registry { get; set; }
    }
}