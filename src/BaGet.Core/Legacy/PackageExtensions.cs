using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BaGet.Core.Entities;
using BaGet.Core.Legacy.OData;
using NuGet;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Versioning;

namespace BaGet.Core.Legacy
{
    public static class PackageExtensions
    {
        public static string ToDependenciesString(this IEnumerable<Entities.PackageDependency> dependencies) 
        {
            if(dependencies == null)
                return null;
            var groups = dependencies.GroupBy(d => ToFrameworkSpec(d.TargetFramework))
                .Select(g => new PackageDependencyGroup(g.Key, g.Select(d => d.ToPackageDependency())));
            return DependencySetsAsString(groups);
        }

        private static NuGetFramework ToFrameworkSpec(string targetFramework)
        {
            if(targetFramework == null)
                return NuGetFramework.AnyFramework;
            else
                return NuGetFramework.Parse(targetFramework);
        }

        public static NuGet.Packaging.Core.PackageDependency ToPackageDependency(this Entities.PackageDependency dependency) {
            return new NuGet.Packaging.Core.PackageDependency(dependency.Id, VersionRange.Parse(dependency.VersionRange));  
        }

        public static string DependencySetsAsString(this IEnumerable<PackageDependencyGroup> dependencySets)
        {
            if (dependencySets == null)
            {
                return null;
            }

            var dependencies = new List<string>();
            foreach (var dependencySet in dependencySets)
            {
                if (!dependencySet.Packages.Any())
                {
                    dependencies.Add(string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", null, null, dependencySet.TargetFramework.GetFrameworkString()));
                }
                else
                {
                    foreach (var dependency in dependencySet.Packages.Select(d => new { d.Id, d.VersionRange, dependencySet.TargetFramework }))
                    {
                        dependencies.Add(string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}",
                            dependency.Id, dependency.VersionRange == null ? null : dependency.VersionRange.ToNormalizedString(), dependencySet.TargetFramework.GetFrameworkString()));
                    }
                }
            }

            return string.Join("|", dependencies);
        }
    }
}
