using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaGet.Core.Entities;
using BaGet.Core.Services;
using BaGet.Web.Extensions;
using BaGet.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BaGet.Controllers.Web.Registration
{
    /// <summary>
    /// The API to retrieve the metadata of a specific package.
    /// </summary>
    public class RegistrationIndexController : Controller
    {
        private readonly IPackageService _packages;

        public RegistrationIndexController(IPackageService packages)
        {
            _packages = packages ?? throw new ArgumentNullException(nameof(packages));
        }

        // GET v3/registration/{id}.json
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            // Documentation: https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource
            var packages = await _packages.FindAsync(id);
            var versions = packages.Select(p => p.Version).ToList();

            if (!packages.Any())
            {
                return NotFound();
            }

            // TODO: Paging of registration items.
            // "Un-paged" example: https://api.nuget.org/v3/registration3/newtonsoft.json/index.json
            // Paged example: https://api.nuget.org/v3/registration3/fake/index.json
            return Json(new
            {
                Count = packages.Count,
                TotalDownloads = packages.Sum(p => p.Downloads),
                Items = new[]
                {
                    new RegistrationIndexItem(
                        packageId: id,
                        items: packages.Select(ToRegistrationIndexLeaf).ToList(),
                        lower: versions.Min().ToNormalizedString(),
                        upper: versions.Max().ToNormalizedString()
                    ),
                }
            });
        }

        private RegistrationIndexLeaf ToRegistrationIndexLeaf(Package package) =>
            new RegistrationIndexLeaf(
                packageId: package.Id,
                catalogEntry: new CatalogEntry(
                    package: package,
                    catalogUri: $"https://api.nuget.org/v3/catalog0/data/2015.02.01.06.24.15/{package.Id}.{package.Version}.json",
                    packageContent: Url.PackageDownload(package.Id, package.Version)),
                packageContent: Url.PackageDownload(package.Id, package.Version));
    }
}