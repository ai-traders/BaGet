using System;
using System.Linq;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Configuration;
using BaGet.Tests.Support;
using System.Net.Http;
using System.Net;
using System.Threading;
using FluentValidation;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace BaGet.Tests
{
    /// <summary>
    /// Uses official nuget client packages to talk to test host.
    /// </summary>
    public class NuGetClientIntegrationTest : IDisposable
    {
        protected readonly ITestOutputHelper Helper;
        private readonly TestServer server;
        readonly string IndexUrlString = "v3/index.json";
        SourceRepository _sourceRepository;
        private SourceCacheContext _cacheContext;
        HttpSourceResource _httpSource;
        private HttpClient _httpClient;
        string indexUrl;
        private NuGet.Common.ILogger logger = new NuGet.Common.NullLogger();

        public NuGetClientIntegrationTest(ITestOutputHelper helper)
        {
            Helper = helper ?? throw new ArgumentNullException(nameof(helper));
            server = TestServerBuilder.Create().TraceToTestOutputHelper(Helper,LogLevel.Error).Build();
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());
            providers.Add(new Lazy<INuGetResourceProvider>(() => new PackageMetadataResourceV3Provider()));
            _httpClient = server.CreateClient();
            providers.Insert(0, new Lazy<INuGetResourceProvider>(() => new HttpSourceResourceProviderTestHost(_httpClient)));

            indexUrl = new Uri(server.BaseAddress, IndexUrlString).AbsoluteUri;
            PackageSource packageSource = new PackageSource(indexUrl); 
            _sourceRepository = new SourceRepository(packageSource, providers);
            _cacheContext = new SourceCacheContext() { NoCache = true, MaxAge=new DateTimeOffset(), DirectDownload=true };
            _httpSource = _sourceRepository.GetResource<HttpSourceResource>();
            Assert.IsType<HttpSourceTestHost>(_httpSource.HttpSource);
        }

        private PackageMetadataResourceV3 GetPackageMetadataResource()
        {
            RegistrationResourceV3 regResource = _sourceRepository.GetResource<RegistrationResourceV3>();
            ReportAbuseResourceV3 reportAbuseResource = _sourceRepository.GetResource<ReportAbuseResourceV3>();
            var packageMetadataRes = new PackageMetadataResourceV3(_httpSource.HttpSource, regResource, reportAbuseResource);
            return packageMetadataRes;
        }

        private string GetApiKey(string arg)
        {
            return "";
        }
        public void Dispose()
        {
            if(server != null)
                server.Dispose();
        }

        [Fact]
        public async Task GetIndexShouldReturn200()
        {
            var index = await _httpClient.GetAsync(indexUrl);
            Assert.Equal(HttpStatusCode.OK, index.StatusCode);
            return;
        }

        [Fact]
        public async Task IndexResourceHasManyEntries()
        {
            var indexResource = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>();
            Assert.NotEmpty(indexResource.Entries);
        }

        [Fact]
        public async Task IndexIncludesAtLeastOneSearchQueryEntry()
        {
            var indexResource = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>();
            Assert.NotEmpty(indexResource.GetServiceEntries("SearchQueryService"));
        }

        [Fact]
        public async Task IndexIncludesAtLeastOneRegistrationsBaseEntry()
        {
            var indexResource = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>();
            Assert.NotEmpty(indexResource.GetServiceEntries("RegistrationsBaseUrl"));
        }

        [Fact]
        public async Task IndexIncludesAtLeastOnePackageBaseAddressEntry()
        {
            var indexResource = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>();
            Assert.NotEmpty(indexResource.GetServiceEntries("PackageBaseAddress/3.0.0"));
        }

        [Fact]
        public async Task IndexIncludesAtLeastOneSearchAutocompleteServiceEntry()
        {
            var indexResource = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>();
            Assert.NotEmpty(indexResource.GetServiceEntries("SearchAutocompleteService"));
        }

        // Push
        [Fact]
        [Trait("Category", "integration")] // because it uses external nupkg files
        public async Task PushValidPackage()
        {
            var packageResource = await _sourceRepository.GetResourceAsync<PackageUpdateResource>();
            await packageResource.Push(TestResources.GetNupkgBagetTest1(),
                null, 5, false, GetApiKey, GetApiKey, false, logger);
            PackageMetadataResourceV3 packageMetadataRes = GetPackageMetadataResource();
            var meta = await packageMetadataRes.GetMetadataAsync("baget-test1", true, true, _cacheContext, logger, CancellationToken.None);
            Assert.NotEmpty(meta);
            var one = meta.First();
            Assert.Equal(new PackageIdentity("baget-test1", NuGetVersion.Parse("1.0.0")), one.Identity);
        }

        [Fact]
        [Trait("Category", "integration")] // because it uses external nupkg files
        public async Task PushAndDeletePackage()
        {
            var packageResource = await _sourceRepository.GetResourceAsync<PackageUpdateResource>();
            await packageResource.Push(TestResources.GetNupkgBagetTest1(),
                null, 5, false, GetApiKey, GetApiKey, false, logger);
            await packageResource.Delete(
                "baget-test1", "1.0.0", GetApiKey, _ => true, false, logger);
            PackageMetadataResourceV3 packageMetadataRes = GetPackageMetadataResource();
            var meta = await packageMetadataRes.GetMetadataAsync("baget-test1", true, true, _cacheContext, logger, CancellationToken.None);
            Assert.Empty(meta);
        }

        // Search
        [Fact]
        [Trait("Category", "integration")] // because it uses external nupkg files
        public async Task PushOneThenSearchPackage()
        {
            var packageResource = await _sourceRepository.GetResourceAsync<PackageUpdateResource>();
            await packageResource.Push(TestResources.GetNupkgBagetTest1(),
                null, 5, false, GetApiKey, GetApiKey, false, logger);
            PackageSearchResourceV3 search = GetSearch();
            var found = await search.SearchAsync("baget-test1", new SearchFilter(true), 0, 10, logger, CancellationToken.None);
            Assert.NotEmpty(found);
            var one = found.First();
            Assert.Equal(new PackageIdentity("baget-test1", NuGetVersion.Parse("1.0.0")), one.Identity);
        }

        private PackageSearchResourceV3 GetSearch()
        {
            PackageMetadataResourceV3 packageMetadataRes = GetPackageMetadataResource();
            RawSearchResourceV3 rawSearchResource = _sourceRepository.GetResource<RawSearchResourceV3>();
            Assert.NotNull(rawSearchResource);
            var search = new PackageSearchResourceV3(rawSearchResource, packageMetadataRes);
            return search;
        }

        [Fact]
        [Trait("Category", "integration")] // because it uses external nupkg files
        public async Task Push2VersionsThenSearchPackage()
        {
            var packageResource = await _sourceRepository.GetResourceAsync<PackageUpdateResource>();
            await packageResource.Push(TestResources.GetNupkgBagetTwoV1(),
                null, 5, false, GetApiKey, GetApiKey, false, logger);
            await packageResource.Push(TestResources.GetNupkgBagetTwoV2(),
                null, 5, false, GetApiKey, GetApiKey, false, logger);
            PackageSearchResourceV3 search = GetSearch();
            var found = await search.SearchAsync("baget-two", new SearchFilter(true), 0, 10, logger, CancellationToken.None);
            Assert.NotEmpty(found);
            var ids = found.Select(p => p.Identity);            
            Assert.Contains(ids, p => p.Version.Equals(NuGetVersion.Parse("2.1.0")));
            var versions = await found.First().GetVersionsAsync();
            Assert.Contains(versions, p => p.Version.Equals(NuGetVersion.Parse("1.0.0")));
            Assert.Contains(versions, p => p.Version.Equals(NuGetVersion.Parse("2.1.0")));
        }
    }
}