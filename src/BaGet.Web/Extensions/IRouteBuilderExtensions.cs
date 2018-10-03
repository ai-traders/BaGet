using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;

namespace BaGet.Web.Extensions
{
    public static class IRouteBuilderExtensions
    {
        public static IRouteBuilder MapServiceIndexRoutes(this IRouteBuilder routes)
        {
            return routes.MapRoute(
                name: Routes.IndexRouteName,
                template: "v3/index.json",
                defaults: new { controller = "Index", action = "Get" });
        }

        public static IRouteBuilder MapPackagePublishRoutes(this IRouteBuilder routes)
        {
            routes.MapRoute(
                name: Routes.UploadRouteName,
                template: "v2/package",
                defaults: new { controller = "PackagePublish", action = "Upload" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("PUT") });

            routes.MapRoute(
                name: Routes.DeleteRouteName,
                template: "v2/package/{id}/{version}",
                defaults: new { controller = "PackagePublish", action = "Delete" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("DELETE") });

            routes.MapRoute(
                name: Routes.RelistRouteName,
                template: "v2/package/{id}/{version}",
                defaults: new { controller = "PackagePublish", action = "Relist" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("POST") });

            return routes;
        }
    }
}
