using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaGet.Core.Entities;
using BaGet.Core.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace BaGet.Tests.Services
{
    public class PackageServiceTest
    {
        public ITestOutputHelper Helper { get; private set; }

        private TestServer server;

        public PackageServiceTest(ITestOutputHelper helper)
        {
            Helper = helper ?? throw new ArgumentNullException(nameof(helper));
            server = TestServerBuilder.Create().TraceToTestOutputHelper(Helper, LogLevel.Error).Build();
        }

        [Fact]
        public async Task GetPackageWithDependencies() {
            var packageService = server.Host.Services.GetRequiredService<IPackageService>();

            var result = await packageService.AddAsync(new Package() {
                Id = "Dummy",
                Title = "Dummy",
                Listed = true,
                Version = NuGetVersion.Parse("1.0.0"),
                Authors = new [] { "Anton Setiawan" },
                LicenseUrl = new Uri("https://github.com/antonmaju/dummy/blob/master/LICENSE"),
                MinClientVersion = null,
                Published = DateTime.Parse("1900-01-01T00:00:00"),
                Dependencies = new List<Core.Entities.PackageDependency>() {
                    new Core.Entities.PackageDependency() { Id="Consul", VersionRange="[0.7.2.6, )", TargetFramework=".NETStandard2.0" }
                }
            });
            Assert.Equal(PackageAddResult.Success, result);

            var found = await packageService.FindAsync("dummy", NuGetVersion.Parse("1.0.0"), false, true);
            Assert.NotNull(found.Dependencies);
            Assert.NotEmpty(found.Dependencies);
            var one = found.Dependencies.Single();
            Assert.Equal("Consul", one.Id);
            Assert.Equal("[0.7.2.6, )", one.VersionRange);
        }
    }
}