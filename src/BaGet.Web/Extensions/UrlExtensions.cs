using System;
using Microsoft.AspNetCore.Http;
using NuGet.Versioning;

namespace BaGet.Web.Extensions
{
    public static class CarterUrlExtensions
    {
        public static string PackageBase(this HttpRequest request) => request.AbsoluteUrl("v3/package/");
        public static string RegistrationsBase(this HttpRequest request) => request.AbsoluteUrl("v3/registration/");

        public static string PackagePublish(this HttpRequest request) => request.AbsoluteUrl("v2/package");
        public static string PackageSearch(this HttpRequest request) => request.AbsoluteUrl("v3/search");
        public static string PackageAutocomplete(this HttpRequest request) => request.AbsoluteUrl("v3/autocomplete");

        public static string PackageDownload(this HttpRequest request, string id, NuGetVersion version)
        {
            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();
            var relativePath = string.Format("v3/package/{0}/{1}/{0}.{1}.nupkg", id, versionString);
            return request.AbsoluteUrl(relativePath);
        }

        public static string PackageRegistration(this HttpRequest request, string id) {
            id = id.ToLowerInvariant();
            var relativePath = string.Format("v3/registration/{0}/index.json", id);
            return request.AbsoluteUrl(relativePath);
        }

        public static string PackageRegistration(this HttpRequest request, string id, NuGetVersion version) {
            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();
            var relativePath = string.Format("v3/registration/{0}/{1}.json", id, versionString);
            return request.AbsoluteUrl(relativePath);
        }

        public static string AbsoluteUrl(this HttpRequest request, string relativePath)
        {
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), relativePath).ToString();
        }
    }
}
