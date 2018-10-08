using System;
using System.Collections.Generic;
using System.Linq;
using BaGet.Web.Extensions;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Carter.ModelBinding;
using Carter.Request;
using Carter.Response;


namespace BaGet.Web.Controllers
{
    /// <summary>
    /// The NuGet Service Index. This aids NuGet client to discover this server's services.
    /// </summary>
    public class IndexModule : CarterModule
    {
        private IEnumerable<ServiceResource> ServiceWithAliases(string name, string url, params string[] versions) 
        {
            foreach (var version in versions) 
            {
                string fullname = string.IsNullOrEmpty(version) ? name : name + "/" + version;
                yield return new ServiceResource(fullname, url);
            }
        }

        public IndexModule() {
            this.Get("/v3/index.json", async (req, res, routeData) =>
            {
                await res.AsJson(new
                {
                    Version = "3.0.0",
                    Resources = 
                        ServiceWithAliases("PackagePublish", req.PackagePublish(), "2.0.0") // api.nuget.org returns this too.
                        .Concat(ServiceWithAliases("SearchQueryService", req.PackageSearch(), "", "3.0.0-beta", "3.0.0-rc")) // each version is an alias of others
                        .Concat(ServiceWithAliases("RegistrationsBaseUrl", req.RegistrationsBase(), "", "3.0.0-rc", "3.0.0-beta"))
                        .Concat(ServiceWithAliases("PackageBaseAddress", req.PackageBase(), "3.0.0"))
                        .Concat(ServiceWithAliases("SearchAutocompleteService", req.PackageAutocomplete(), "", "3.0.0-rc", "3.0.0-beta"))
                        .ToList()
                });
            });
        }
        private class ServiceResource
        {
            public ServiceResource(string type, string id, string comment = null)
            {
                Id = id ?? throw new ArgumentNullException(nameof(id));
                Type = type ?? throw new ArgumentNullException(nameof(type));
                Comment = comment ?? string.Empty;
            }

            [JsonProperty(PropertyName = "@id")]
            public string Id { get; }

            [JsonProperty(PropertyName = "@type")]
            public string Type { get; }

            public string Comment { get; }
        }
    }
}