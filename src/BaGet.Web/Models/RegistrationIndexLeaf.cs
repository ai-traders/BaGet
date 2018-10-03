using System;
using BaGet.Core.Entities;
using Newtonsoft.Json;
using NuGet.Protocol.Core.Types;
using BaGet.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BaGet.Web.Models
{
    public class RegistrationIndexLeaf
    {
        public static RegistrationIndexLeaf ToRegistrationIndexLeaf(ControllerBase c, IPackageSearchMetadata package) =>
            new RegistrationIndexLeaf(
                packageId: package.Identity.Id,
                catalogEntry: new CatalogEntry(
                    package: package,
                    catalogUri: $"https://api.nuget.org/v3/catalog0/data/2015.02.01.06.24.15/{package.Identity.Id}.{package.Identity.Version}.json",
                    packageContent: c.Url.PackageDownload(package.Identity.Id, package.Identity.Version)),
                packageContent: c.Url.PackageDownload(package.Identity.Id, package.Identity.Version));

        public static RegistrationIndexLeaf ToRegistrationIndexLeaf(ControllerBase c, Package package) =>
            new RegistrationIndexLeaf(
                packageId: package.Id,
                catalogEntry: new CatalogEntry(
                    package: package,
                    catalogUri: $"https://api.nuget.org/v3/catalog0/data/2015.02.01.06.24.15/{package.Id}.{package.Version}.json",
                    packageContent: c.Url.PackageDownload(package.Id, package.Version)),
                packageContent: c.Url.PackageDownload(package.Id, package.Version));

        public RegistrationIndexLeaf(string packageId, CatalogEntry catalogEntry, string packageContent)
        {
            if (string.IsNullOrEmpty(packageId)) throw new ArgumentNullException(nameof(packageId));

            PackageId = packageId;
            CatalogEntry = catalogEntry ?? throw new ArgumentNullException(nameof(catalogEntry));
            PackageContent = packageContent ?? throw new ArgumentNullException(nameof(packageContent));
        }

        [JsonProperty(PropertyName = "id")]
        public string PackageId { get; }

        public CatalogEntry CatalogEntry { get; }

        public string PackageContent { get; }
    }
}