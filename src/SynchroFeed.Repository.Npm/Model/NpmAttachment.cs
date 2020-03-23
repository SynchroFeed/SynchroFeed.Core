using System;
using Newtonsoft.Json;

namespace SynchroFeed.Repository.Npm.Model
{
    public class NpmAttachment
    {
        [JsonProperty("content_type")] public string ContentType { get; set; }
        [JsonProperty("data")] public string Data { get; set; }
        [JsonProperty("length")] public long Length { get; set; }
    }
}