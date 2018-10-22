using System.Collections.Generic;
using System.Linq;
using BaGet.Web.Models;
using Xunit;

namespace BaGet.Tests.Models
{
    public class CatalogEntryTest
    {
        List<Core.Entities.PackageDependency> frameworkDeps = new List<Core.Entities.PackageDependency>() {
            new Core.Entities.PackageDependency() { TargetFramework="netstandard2.0" },
            new Core.Entities.PackageDependency() { TargetFramework="net35" }
        };

        List<Core.Entities.PackageDependency> packagePerFrameworkDeps = new List<Core.Entities.PackageDependency>() {
            new Core.Entities.PackageDependency() { TargetFramework="netstandard2.0", Id="dep1"  },
            new Core.Entities.PackageDependency() { TargetFramework="netstandard2.0", Id="depX"  },
            new Core.Entities.PackageDependency() { TargetFramework="net35", Id="dep2" }
        };

        List<Core.Entities.PackageDependency> anyFrameworkPackageDeps = new List<Core.Entities.PackageDependency>() {
            new Core.Entities.PackageDependency() { Id="dep1"  },
            new Core.Entities.PackageDependency() { Id="dep2"  },
        };

        [Fact]
        public void ToDependencyGroups_ShouldIncludeFrameworkDependencies()
        {
            var result = CatalogEntry.ToDependencyGroups(frameworkDeps, "http://catalog/package.json");
            Assert.All(result, r => Assert.Null(r.Dependencies));
        }

        [Fact]
        public void ToDependencyGroups_ShouldGroupPerFrameworkDepsTogether()
        {
            var result = CatalogEntry.ToDependencyGroups(packagePerFrameworkDeps, "http://catalog/package.json");
            var netstd2 = result.First(n => n.TargetFramework == "netstandard2.0");
            Assert.Equal(new string[] { "dep1", "depX" }, netstd2.Dependencies.Select(d => d.Id));
            var net35 = result.First(n => n.TargetFramework == "net35");
            Assert.Equal(new string[] { "dep2" }, net35.Dependencies.Select(d => d.Id));
        }

        [Fact]
        public void ToDependencyGroups_ShouldGroupDependenciesWithoutFrameworkTogether()
        {
            var result = CatalogEntry.ToDependencyGroups(anyFrameworkPackageDeps, "http://catalog/package.json");
            var deps = result.First();
            Assert.Equal(new string[] { "dep1", "dep2" }, deps.Dependencies.Select(d => d.Id));
            Assert.All(result, r => Assert.Null(r.TargetFramework));
        }
    }
}