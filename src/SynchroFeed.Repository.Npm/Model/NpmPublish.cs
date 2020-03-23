using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SynchroFeed.Repository.Npm.Model
{
    public class NpmPublish
    {
        [JsonProperty("_id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("dist-tags")] public Dictionary<string, string> DistTags { get; set; }
        [JsonProperty("versions")] public Dictionary<string, NpmPackage> Versions { get; set; }
        [JsonProperty("readme")] public string Readme { get; set; }
        [JsonProperty("_attachments")] public Dictionary<string, NpmAttachment> Attachments { get; set; }
    }
}