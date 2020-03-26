using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using ProGet.Net;
using ProGet.Net.Native.Models;
using SynchroFeed.Library.Model;
using SynchroFeed.Repository.Npm.Model;
using NpmPackage = SynchroFeed.Repository.Npm.Model.NpmPackage;

namespace SynchroFeed.Repository.Npm
{
    public static class NpmExtensions
    {
        private const string ApiKeyHeaderName = "X-ApiKey";
        private const string prereleaseRegEx = @"(?<PackageId>.*\d+\.\d+\.\d+)(?<Prerelease>.*)";

        public static async Task<(IEnumerable<Package> Packages, Error Error)> NpmFetchPackagesAsync(this NpmClient client, Expression<Func<Package, bool>> expression, bool downloadPackageContent)
        {
            var uri = new Uri(client.Uri);

            var hostUri = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            var feedName = uri.Segments.Last().Replace("/", "");

            var sourceClient = new ProGetClient(hostUri, client.ApiKey);
            Feed sourceFeed;
            try
            {
                sourceFeed = (await sourceClient.Feeds_GetFeedsAsync(false))
                    .SingleOrDefault(f => f.Feed_Name.Equals(feedName, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Message.Contains("403"))
                    return (null, new Error(HttpStatusCode.Forbidden, ex.Message));
                return (null, new Error(HttpStatusCode.NotFound, ex.Message));
            }
            if (sourceFeed == null)
                return (null, new Error(HttpStatusCode.NotFound, $"Feed not found ({client.FeedName}) on {hostUri}"));

            var packageVersions = await sourceClient.NpmFeeds_GetAllPackageVersionsAsync(sourceFeed.Feed_Id);
            var expressionFunc = expression.Compile();
            var npmPackageVersionsArray = packageVersions as NpmPackageAllVersions[] ?? packageVersions.ToArray();
            var packages = new List<Package>(npmPackageVersionsArray.Length);
            packages.AddRange(npmPackageVersionsArray.Select(async p => 
                                await ConvertNpmPackageVersionToPackageAsync(client, p))
                                  .Select(t => t.Result));
            var matchedPackages = packages.Where(package => expressionFunc(package)).ToList();
            if (downloadPackageContent)
            {
                foreach (var matchedPackage in matchedPackages)
                {
                    var (content, error) = await client.NpmDownloadPackageAsync(matchedPackage);
                    if (error != null)
                        throw new WebException(error.ErrorMessage);
                    matchedPackage.Content = content;
                }
            }

            return (matchedPackages, null);
        }

        public static async Task<(NpmPackage Package, Error Error)> NpmAddPackageAsync(this NpmClient client, NpmPackage package, byte[] packageContent)
		{
            var publishPackage = new NpmPublish
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                Readme = "",
                Versions = new Dictionary<string, NpmPackage>
                {
                    {package.Version, package}
                },
            };

            var attachment = new NpmAttachment()
			{
				ContentType = "application/octet-stream",
				Data = Convert.ToBase64String(packageContent),
				Length = packageContent.LongLength
			};

			var archiveUri = new Uri(package.Dist[NpmPackage.ArchiveKey]);
            publishPackage.Attachments = new Dictionary<string, NpmAttachment>
            {
                {archiveUri.Segments.Last(), attachment}
            };

            var content = new StringContent(JsonConvert.SerializeObject(publishPackage, new JsonSerializerSettings{ MissingMemberHandling = MissingMemberHandling.Ignore }), Encoding.UTF8, "application/json");
			content.Headers.Add(ApiKeyHeaderName, client.ApiKey);
			// TODO: Scope is included in the package.Name - need to do some testing to see if scope if actually needed
            using (var response = await client.HttpClient.PutAsync(new Uri(new Uri(client.Uri), GetNpmPath(package.Name, package.Version)), content))
            {
                if (response.IsSuccessStatusCode)
                {
                    return (package, null);
                }

                return (null, new Error(response.StatusCode, response.ReasonPhrase));
            }
        }

		public static async Task<(NpmPackage Package, Error Error)> NpmGetPackageAsync(this NpmClient client, string packageName, string version)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(client.Uri), GetNpmPath(packageName, version))))
            {
                requestMessage.Headers.Add(ApiKeyHeaderName, client.ApiKey);
                requestMessage.Headers.Add("Accept", "application/json");
                using (var response = await client.HttpClient.SendAsync(requestMessage))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var npmPackage = JsonConvert.DeserializeObject<NpmPackage>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                        return (npmPackage, null);
                    }

                    return (null, new Error(response.StatusCode, response.ReasonPhrase));
                }
            }
        }

        public static async Task<Error> NpmDeletePackageAsync(this NpmClient client, Package package)
        {
            var uri = new Uri(client.Uri);
            var hostUri = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            var feedName = uri.Segments.Last().Replace("/", "");

            var sourceClient = new ProGetClient(hostUri, client.ApiKey);
            var sourceFeeds = await sourceClient.Feeds_GetFeedsAsync(false);
            var sourceFeed = sourceFeeds.SingleOrDefault(f => f.Feed_Name.Equals(feedName, StringComparison.CurrentCultureIgnoreCase));
            if (sourceFeed == null)
            {
                return new Error(HttpStatusCode.NotFound, $"NPM Feed not found {client.FeedName} on {client.Uri}");
            }
            var (name, scope) = ParseNpmId(package.Id);
            var result = sourceClient.NpmPackages_DeletePackageAsync(sourceFeed.Feed_Id, name, scope, package.Version).Result;

            if (result)
            {
                return null;
            }

            return (new Error(HttpStatusCode.ServiceUnavailable, "Error"));
        }

        public static async Task<(byte[] Content, Error Error)> NpmDownloadPackageAsync(this NpmClient client, Package package, bool validateHash = true)
		{
            using (var packageDownloadResponse = await client.HttpClient.GetAsync(package.PackageDownloadUrl))
            {
                if (packageDownloadResponse.IsSuccessStatusCode)
                {
                    var content = await packageDownloadResponse.Content.ReadAsByteArrayAsync();
                    if (validateHash && !HashValid(package, content))
                    {
                        Console.WriteLine($"SHA1 hash of NPM package downloaded doesn't match expected SHA {package.PackageHash}");
                        return (content,
                            new Error(HttpStatusCode.InternalServerError, $"SHA1 hash of NPM package downloaded doesn't match expected SHA {package.PackageHash}"));
                    }

                    return (content, null);
                }

                return (null, new Error(packageDownloadResponse.StatusCode, packageDownloadResponse.ReasonPhrase));
            }
        }

        public static (string Name, string Scope) ParseNpmId(string npmId)
        {
            // NPM ID has the following format: @alkami/albus@0.1.0
            // Need to break that down into its components
            string name;
            string scope;

            var npmIdNoVersion = npmId.LastIndexOf('@', 1) == -1 ? npmId : npmId.Substring(0, npmId.LastIndexOf('@'));

            var parts = npmIdNoVersion.Split('/');
            if (parts.Length == 2)
            {
                name = parts[1].Substring(0, parts[1].IndexOf("@", StringComparison.Ordinal));
                // Remove @ from scope
                scope = parts[0].Substring(1);
            }
            else if (parts.Length == 1)
            {
                name = parts[0];
                scope = "";
            }
            else
            {
                throw new InvalidOperationException($"NPM ID not in expected format {npmId}");
            }

            return (name, scope);
        }

        private static string ConvertNpmPackageNameToId(NpmPackageAllVersions npmPackageVersion)
        {
            return string.IsNullOrEmpty(npmPackageVersion.Scope_Name)
                ? npmPackageVersion.Package_Name
                : $"@{npmPackageVersion.Scope_Name}/{npmPackageVersion.Package_Name}";
        }

        public static NpmPackage ToNpmPackage(this Package package)
        {
            var npmPackage = new NpmPackage()
            {
                Id = package.Id,
                Version = package.Version,
                Name = package.Title,
                Dist = new Dictionary<string, string>()
                {
                    { NpmPackage.ArchiveKey, package.PackageDownloadUrl },
                    { NpmPackage.HashKey, package.PackageHash }
                }
            };

            return npmPackage;
        }

        private static async Task<Package> ConvertNpmPackageVersionToPackageAsync(NpmClient client, NpmPackageAllVersions npmPackageAllVersions)
        {
            var (npmPackage, error) = await client.NpmGetPackageAsync(GetNpmName(npmPackageAllVersions.Package_Name, npmPackageAllVersions.Scope_Name), npmPackageAllVersions.Version_Text);
            if (error != null)
                throw new WebException(error.ErrorMessage);

            var match = Regex.Match(npmPackage.Id, prereleaseRegEx);
            var prerelease = match.Groups["Prerelease"].Value.Any();
            var package = new Package
            {
                Id = ConvertNpmPackageNameToId(npmPackageAllVersions),
                Version = npmPackageAllVersions.Version_Text,
                PackageDownloadUrl = npmPackage.Dist[NpmPackage.ArchiveKey],
                PackageHash = npmPackage.Dist[NpmPackage.HashKey],
                IsPrerelease = prerelease
            };

            return package;
        }

        public static Package ToPackage(this NpmPackage npmPackage)
        {
            var match = Regex.Match(npmPackage.Id, prereleaseRegEx);
            var prerelease = match.Groups["Prerelease"].Value.Any();
            var package = new Package()
            {
                // Remove the version from the NPM Package ID. SynchroFeed uses Id and Version together to uniquely identify a package.
                Id = npmPackage.Id.Substring(1).LastIndexOf('@') == -1 ? npmPackage.Id : npmPackage.Id.Substring(0, npmPackage.Id.LastIndexOf('@')),
                Version = npmPackage.Version,
                Title = npmPackage.Name,
                PackageDownloadUrl = npmPackage.Dist[NpmPackage.ArchiveKey],
                PackageHash = npmPackage.Dist[NpmPackage.HashKey],
                IsPrerelease = prerelease
            };

            return package;
        }

        private static bool HashValid(Package package, byte[] content)
		{
            using (var sha = SHA1.Create())
            {
                var hashBytes = sha.ComputeHash(content);
                var shasum = ConvertHashToString(hashBytes);

                return shasum.Equals(package.PackageHash, StringComparison.OrdinalIgnoreCase);
            }
        }

		private static string ConvertHashToString(byte[] hashBytes)
		{
			var sb = new StringBuilder(hashBytes.Length * 2);

			foreach (byte b in hashBytes)
			{
				sb.Append(b.ToString("x2"));
			}

			return sb.ToString();
		}

        private static string GetNpmPath(string packageName, string version)
        {
            var result = $"{packageName}/{version}";

            return result;
        }


        private static string GetNpmName(string packageName, string scope)
        {
            var result = string.IsNullOrEmpty(scope) ?
                $"{packageName}" :
                $"@{scope}/{packageName}";

            return result;
        }
    }
}
