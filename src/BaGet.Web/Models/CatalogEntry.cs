using System;
using BaGet.Core.Entities;
using Newtonsoft.Json;
using NuGet.Protocol.Core.Types;

namespace BaGet.Web.Models
{
    public class CatalogEntry
    {
        public CatalogEntry(Package package, string catalogUri, string packageContent)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            CatalogUri = catalogUri ?? throw new ArgumentNullException(nameof(catalogUri));

            PackageId = package.Id;
            Version = package.VersionString;
            Authors = string.Join(", ", package.Authors);
            Description = package.Description;
            Downloads = package.Downloads;
            HasReadme = package.HasReadme;
            IconUrl = package.IconUrlString;
            Language = package.Language;
            LicenseUrl = package.LicenseUrlString;
            Listed = package.Listed;
            MinClientVersion = package.MinClientVersion;
            PackageContent = packageContent;
            ProjectUrl = package.ProjectUrlString;
            RepositoryUrl = package.RepositoryUrlString;
            RepositoryType = package.RepositoryType;
            Published = package.Published;
            RequireLicenseAcceptance = package.RequireLicenseAcceptance;
            Summary = package.Summary;
            Tags = package.Tags;
            Title = package.Title;
        }

        public CatalogEntry(IPackageSearchMetadata package, string catalogUri, string packageContent)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            CatalogUri = catalogUri ?? throw new ArgumentNullException(nameof(catalogUri));

            PackageId = package.Identity.Id;
            Version = package.Identity.Version.ToFullString();
            Authors = string.Join(", ", package.Authors);
            Description = package.Description;
            Downloads = package.DownloadCount.GetValueOrDefault(0);
            HasReadme = false; // 
            IconUrl = NullSafeToString(package.IconUrl);
            Language = null; //
            LicenseUrl = NullSafeToString(package.LicenseUrl);
            Listed = package.IsListed;
            //MinClientVersion =
            PackageContent = packageContent;
            ProjectUrl = NullSafeToString(package.ProjectUrl);
            //RepositoryUrl = package.RepositoryUrlString;
            //RepositoryType = package.RepositoryType;
            //Published = package.Published.GetValueOrDefault(DateTimeOffset.MinValue);
            RequireLicenseAcceptance = package.RequireLicenseAcceptance;
            Summary = package.Summary;
            Tags = package.Tags == null ? null : package.Tags.Split(",");
            Title = package.Title;
        }

        private string NullSafeToString(object prop)
        {
            if(prop == null)
                return null;
            return prop.ToString();
        }

        [JsonProperty(PropertyName = "@id")]
        public string CatalogUri { get; }

        [JsonProperty(PropertyName = "id")]
        public string PackageId { get; }

        public string Version { get; }
        public string Authors { get; }
        public string Description { get; }
        public long Downloads { get; }
        public bool HasReadme { get; }
        public string IconUrl { get; }
        public string Language { get; }
        public string LicenseUrl { get; }
        public bool Listed { get; }
        public string MinClientVersion { get; }
        public string PackageContent { get; }
        public string ProjectUrl { get; }
        public string RepositoryUrl { get; }
        public string RepositoryType { get; }
        public DateTime Published { get; }
        public bool RequireLicenseAcceptance { get; }
        public string Summary { get; }
        public string[] Tags { get; }
        public string Title { get; }
    }
}