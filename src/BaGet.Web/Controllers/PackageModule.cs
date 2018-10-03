using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core.Mirror;
using BaGet.Core.Services;
using Carter;
using Carter.ModelBinding;
using Carter.Request;
using Carter.Response;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace BaGet.Controllers
{
    public class PackageModule : CarterModule
    {
        private readonly IMirrorService _mirror;
        private readonly IPackageService _packages;
        private readonly IPackageStorageService _storage;

        public PackageModule(IMirrorService mirror, IPackageService packageService, IPackageStorageService storage)
        {
            _mirror = mirror ?? throw new ArgumentNullException(nameof(mirror));
            _packages = packageService ?? throw new ArgumentNullException(nameof(packageService));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            this.Get("v3/package/{id}/index.json", async (req, res, routeData) => {
                string id = routeData.As<string>("id");
                var packages = await _packages.FindAsync(id);

                if (!packages.Any())
                {
                    res.StatusCode = 404;
                    return;
                }

                await res.AsJson(new
                {
                    Versions = packages.Select(p => p.VersionString).ToList()
                });
            });

            this.Get("v3/package/{id}/{version}/{idVersion}.nupkg", async (req, res, routeData) => {
                string id = routeData.As<string>("id");
                string version = routeData.As<string>("version");

                if (!NuGetVersion.TryParse(version, out var nugetVersion))
                {
                    res.StatusCode = 400;
                    return;
                }

                // Allow read-through caching if it is configured.
                await _mirror.MirrorAsync(id, nugetVersion, CancellationToken.None);

                if (!await _packages.AddDownloadAsync(id, nugetVersion))
                {
                     res.StatusCode = 404;
                     return;
                }

                var identity = new PackageIdentity(id, nugetVersion);
                var packageStream = await _storage.GetPackageStreamAsync(identity);

                await res.FromStream(packageStream, "application/octet-stream");
            });

            this.Get("v3/package/{id}/{version}/{id2}.nuspec", async (req, res, routeData) => {
                string id = routeData.As<string>("id");
                string version = routeData.As<string>("version");

                if (!NuGetVersion.TryParse(version, out var nugetVersion))
                {
                    res.StatusCode = 400;
                    return;
                }

                // Allow read-through caching if it is configured.
                await _mirror.MirrorAsync(id, nugetVersion, CancellationToken.None);

                if (!await _packages.ExistsAsync(id, nugetVersion))
                {
                    res.StatusCode = 404;
                    return;
                }

                var identity = new PackageIdentity(id, nugetVersion);

                await res.FromStream(await _storage.GetNuspecStreamAsync(identity), "text/xml");
            });

            this.Get("v3/package/{id}/{version}/readme", async (req, res, routeData) => {
                string id = routeData.As<string>("id");
                string version = routeData.As<string>("version");

                if (!NuGetVersion.TryParse(version, out var nugetVersion))
                {
                    res.StatusCode = 400;
                    return;
                }

                // Allow read-through caching if it is configured.
                await _mirror.MirrorAsync(id, nugetVersion, CancellationToken.None);

                if (!await _packages.ExistsAsync(id, nugetVersion))
                {
                    res.StatusCode = 404;
                    return;
                }

                var identity = new PackageIdentity(id, nugetVersion);

                await res.FromStream(await _storage.GetReadmeStreamAsync(identity), "text/markdown");
            });
        }
    }
}