using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using WebApplication_lab.Services;
using System.Threading.Tasks;
using WebApplication_lab.Models;
using WebApplication_lab.Services;

namespace WebApplication_lab.Controllers
{
    public class HealthController : Controller
    {
        private readonly HealthService _service;

        public HealthController(HealthService service)
        {
            _service = service;
        }

        // GET: /Health/Input
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Показуємо форму для введення тексту
        }

        // POST: /Health/Analyze
        [HttpPost]
        public async Task<IActionResult> Analyze(TextAnalysisInputModel model)
        {
            // Перевірка чи валідна модель (чи є текст)
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Аналіз тексту через сервіс
            var result = await _service.AnalyzeAsync(model.Text);

            // Повертаємо View з результатом (Output.cshtml)
            return View("Result", result);
        }
    }
}
