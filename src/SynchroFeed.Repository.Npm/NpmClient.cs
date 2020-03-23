using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SynchroFeed.Repository.Npm
{
    public class NpmClient
    {
        public NpmClient(NpmRepository repository, string feedUri, string apiKey, string feedName, ILogger logger)
        {
            this.Repository = repository;
            this.HttpClient = new HttpClient();
            this.Uri = feedUri.EndsWith("/") ? feedUri : feedUri + "/";
            this.ApiKey = apiKey;
            this.FeedName = feedName;
            this.Logger = logger;
        }

        public HttpClient HttpClient { get; }
        public NpmRepository Repository { get; }
        public string Uri { get; }
        public string ApiKey { get; }
        public string FeedName { get; }

        public ILogger Logger { get; set; }
    }
}
