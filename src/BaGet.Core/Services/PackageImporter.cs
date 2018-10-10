using System;
using System.IO;
using System.Threading.Tasks;

namespace BaGet.Core.Services
{
    public class PackageImporter
    {
        private readonly IIndexingService _indexingService;

        public PackageImporter(IIndexingService indexingService) {
            this._indexingService = indexingService;
        }

        public async Task ImportAsync(string pkgDirectory, TextWriter output)
        {
            string[] files = Directory.GetFiles(pkgDirectory, "*.nupkg", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                output.Write("Importing package {0} ", file);
                using(var uploadStream = File.OpenRead(file)) {
                    var result = await _indexingService.IndexAsync(uploadStream);
                    output.WriteLine(result);
                }
            }            
        }
    }
}