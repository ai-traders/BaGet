using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core.Services;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Protocol;
using NuGet.Common;
using System.Linq;
using NuGet.Packaging.Core;
using System.IO;
using BaGet.Core.Configuration;

namespace BaGet.Core.Mirror
{
    public class MirrorService : IMirrorService
    {
        private readonly IPackageCacheService _localPackages;
        private readonly IPackageDownloader _downloader;
        private readonly ILogger<MirrorService> _logger;
        private readonly SourceRepository _sourceRepository;
        private readonly RegistrationResourceV3 _regResource;
        private SourceCacheContext _cacheContext;
        NuGetLoggerAdapter<MirrorService> _loggerAdapter;
        private PackageMetadataResourceV3 _metadataSearch;
        private RemoteV3FindPackageByIdResource _versionSearch;

        public MirrorService(
            IPackageCacheService localPackages,
            IPackageDownloader downloader,
            ILogger<MirrorService> logger,
            MirrorOptions options)
        {
            _localPackages = localPackages ?? throw new ArgumentNullException(nameof(localPackages));
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._loggerAdapter = new NuGetLoggerAdapter<MirrorService>(_logger);
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());
            providers.Add(new Lazy<INuGetResourceProvider>(() => new PackageMetadataResourceV3Provider()));
            PackageSource packageSource = new PackageSource(options.UpstreamIndex.AbsoluteUri);
            _sourceRepository = new SourceRepository(packageSource, providers);
            _cacheContext = new SourceCacheContext();
            var httpSource = _sourceRepository.GetResource<HttpSourceResource>();
            _regResource = _sourceRepository.GetResource<RegistrationResourceV3>();
            ReportAbuseResourceV3 reportAbuseResource = _sourceRepository.GetResource<ReportAbuseResourceV3>();
            _metadataSearch = new PackageMetadataResourceV3(httpSource.HttpSource, _regResource, reportAbuseResource);
            _versionSearch = new RemoteV3FindPackageByIdResource(_sourceRepository, httpSource.HttpSource);
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> FindUpstreamMetadataAsync(string id, CancellationToken ct) {
            //TODO: possibly cache response
            return await _metadataSearch.GetMetadataAsync(id, true, false, _cacheContext, _loggerAdapter, ct);
        }

        public async Task<IReadOnlyList<string>> FindUpstreamAsync(string id, CancellationToken ct)
        {           
            var versions = await _versionSearch.GetAllVersionsAsync(id, _cacheContext, _loggerAdapter, ct);
            //TODO: possibly cache response
            return versions.Select(v => v.ToNormalizedString()).ToList();
        }

        public async Task MirrorAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            var pid = new PackageIdentity(id, version);
            if (await _localPackages.ExistsAsync(pid))
            {
                return;
            }

            var idString = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();

            await IndexFromSourceAsync(idString, versionString, cancellationToken);
        }

        private async Task IndexFromSourceAsync(string id, string version, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Attempting to mirror package {Id} {Version}...", id, version);

            try
            {
                var pid = new PackageIdentity(id, NuGetVersion.Parse(version));
                var serviceIndex = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>(cancellationToken);
                var packageBaseAddress = serviceIndex.GetServiceEntryUri(ServiceTypes.PackageBaseAddress);      
                Uri packageUri;          
                if (packageBaseAddress != null)
                {
                    packageUri = new Uri(packageBaseAddress, $"{id}/{version}/{id}.{version}.nupkg");
                }
                else
                {
                    _logger.LogDebug("Upstream repository does not support flat container, falling back to registration");
                    // If there is no flat container resource fall back to using the registration resource to find the download url.
                    using (var sourceCacheContext = new SourceCacheContext())
                    {
                        // Read the url from the registration information
                        var blob = await _regResource.GetPackageMetadata(pid, sourceCacheContext, _loggerAdapter, cancellationToken);
                        if (blob != null && blob["packageContent"] != null)
                        {
                            packageUri = new Uri(blob["packageContent"].ToString());
                        }
                        else
                            throw new InvalidOperationException("Could not determine upstream url for download");
                    }
                }

                using (var stream = await _downloader.DownloadOrNullAsync(packageUri, cancellationToken))
                {
                    if (stream == null)
                    {
                        _logger.LogWarning(
                            "Failed to download package {Id} {Version} at {PackageUri}",
                            id,
                            version,
                            packageUri);

                        return;
                    }

                    _logger.LogInformation("Downloaded package {Id} {Version}, adding to cache...", id, version);

                    await _localPackages.AddPackageAsync(stream);

                    _logger.LogInformation(
                        "Finished adding package {Id} {Version}",
                        id,
                        version);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mirror package {Id} {Version}", id, version);
            }
        }

        public Task<Stream> GetPackageStreamAsync(PackageIdentity identity)
        {
            //TODO: possibly stream from in-memory cache
            return _localPackages.GetPackageStreamAsync(identity);
        }

        public Task<bool> ExistsAsync(PackageIdentity identity)
        {
            return _localPackages.ExistsAsync(identity);
        }

        public Task<Stream> GetNuspecStreamAsync(PackageIdentity identity)
        {
            //TODO: possibly stream from in-memory cache
            return _localPackages.GetNuspecStreamAsync(identity);
        }

        public Task<Stream> GetReadmeStreamAsync(PackageIdentity identity)
        {
            //TODO: possibly stream from in-memory cache
            return _localPackages.GetReadmeStreamAsync(identity);
        }

        public Task<IPackageSearchMetadata> FindAsync(PackageIdentity identity)
        {
            //TODO: possibly cache and stream from in-memory cache
            return _metadataSearch.GetMetadataAsync(identity, _cacheContext, _loggerAdapter, CancellationToken.None);             
        }
    }
}
