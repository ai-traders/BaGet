using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaGet.Core.Services;
using BaGet.Web.Extensions;
using BaGet.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BaGet.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        }

        public async Task<object> Get([FromQuery(Name = "q")] string query = null)
        {
            query = query ?? string.Empty;

            var results = await _searchService.SearchAsync(query);

            return new
            {
                TotalHits = results.Count,
                Data = results.Select(p => new SearchResultModel(p, Url.ActionContext.HttpContext.Request))
            };
        }

        public async Task<IActionResult> Autocomplete([FromQuery(Name = "q")] string query = null)
        {
            var results = await _searchService.AutocompleteAsync(query);

            return Json(new
            {
                TotalHits = results.Count,
                Data = results,
            });
        }
    }
}