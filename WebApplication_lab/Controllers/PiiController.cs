using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using WebApplication_lab.Services;

namespace WebApplication_lab.Controllers
{
    public class PiiController : Controller
    {
        private readonly PiiService _service;

        public PiiController(PiiService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Analyze(TextAnalysisInputModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var result = await _service.AnalyzeAsync(model.Text);
            return View("Result", result);
        }
    }

}
