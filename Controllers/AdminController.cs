using Microsoft.AspNetCore.Mvc;
using Transifex.Backend.Services;

namespace Transifex.Backend.Controllers
{
    public class AdminController : Controller
    {
        private ITransifexService _transifexService { get; }

        public AdminController(ITransifexService transifexService)
        {
            _transifexService = transifexService;
        }
    }
}