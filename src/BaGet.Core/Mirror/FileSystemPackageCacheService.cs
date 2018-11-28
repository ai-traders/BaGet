using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core.Extensions;
using BaGet.Core.Services;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Repositories;
using NuGet.Versioning;

namespace BaGet.Core.Mirror
{
    public class FileSystemPackageCacheService : IPackageCacheService
    {
        private FilePackageStorageService _fsStorageProvider;

        public FileSystemPackageCacheService(string storagePath) {
            if (storagePath == null)
            {
                throw new ArgumentNullException(nameof(storagePath));
            }

            _fsStorageProvider = new FilePackageStorageService(storagePath);
        }

        public async Task AddPackageAsync(Stream stream)
        {
            using (var archiveReader = new PackageArchiveReader(stream))
            {
                await _fsStorageProvider.SavePackageStreamAsync(archiveReader, stream, CancellationToken.None);
            }
        }

        public Task<bool> ExistsAsync(PackageIdentity package)
        {
            return Task.FromResult(File.Exists(Path.Combine(_fsStorageProvider.RootPath, package.PackagePath())));
        }

        public Task<Stream> GetNuspecStreamAsync(PackageIdentity identity)
        {
            return _fsStorageProvider.GetNuspecStreamAsync(identity);
        }

        public Task<Stream> GetPackageStreamAsync(PackageIdentity identity)
        {
            return _fsStorageProvider.GetPackageStreamAsync(identity);
        }

        public Task<Stream> GetReadmeStreamAsync(PackageIdentity identity)
        {
            return _fsStorageProvider.GetReadmeStreamAsync(identity);
        }
    }
}