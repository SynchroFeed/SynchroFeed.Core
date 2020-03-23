using System;
using System.Net;

namespace SynchroFeed.Repository.Npm.Model
{
    public class Error
    {
        public Error(HttpStatusCode statusCode, string errorMessage = null)
        {
            this.StatusCode = statusCode;
            this.ErrorMessage = errorMessage;
        }

        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}