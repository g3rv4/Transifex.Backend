using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Transifex.Backend.Services;

namespace Transifex.Backend.Controllers
{
    public class HomeController : Controller
    {
        private ITransifexService _transifexService { get; }

        public HomeController(ITransifexService transifexService)
        {
            _transifexService = transifexService;
        }

        public async Task<IActionResult> GetData()
        {
            var allTheStrings = await _transifexService.GetAllStringsAsync();
            var first = allTheStrings.First();

            await _transifexService.UpdateStringsDatabaseAsync();

            //q = q.Where(e=>e.String.Trim().ToLower().StartsWith("clos"));
            return Content($"<pre>{String.Join("\n", allTheStrings.Select(t=>$"|{t.String}|{t.Id}").ToArray())}<pre>");
        }
    }
}