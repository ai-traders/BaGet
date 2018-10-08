using System;
using System.Collections.Generic;
using System.Linq;
using BaGet.Core.Services;
using BaGet.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaGet.Web.Models
{
    public class SearchResultModel
    {
        private readonly SearchResult _result;
        private readonly HttpRequest _url;

        public SearchResultModel(SearchResult result, HttpRequest url)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
            _url = url ?? throw new ArgumentNullException(nameof(url));

            var versions = result.Versions.Select(
                v => new SearchResultVersionModel(
                    url.PackageRegistration(result.Id, v.Version),
                    v.Version.ToNormalizedString(),
                    v.Downloads));

            Versions = versions.ToList().AsReadOnly();
        }

        public string Id => _result.Id;
        public string Version => _result.Version.ToNormalizedString();
        public string Description => _result.Description;
        public string Authors => _result.Authors;
        public string IconUrl => _result.IconUrl;
        public string LicenseUrl => _result.LicenseUrl;
        public string ProjectUrl => _result.ProjectUrl;
        public string Registration => _url.PackageRegistration(_result.Id);
        public string Summary => _result.Summary;
        public string[] Tags => _result.Tags;
        public string Title => _result.Title;
        public long TotalDownloads => _result.TotalDownloads;

        public IReadOnlyList<SearchResultVersionModel> Versions { get; }
    }
}