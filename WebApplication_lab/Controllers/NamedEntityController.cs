using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System.Text;

namespace WebApplication_lab.Controllers
{
    public class NamedEntityController : Controller
    {
        private static readonly AzureKeyCredential credentials =
            new AzureKeyCredential(Environment.GetEnvironmentVariable("LANGUAGE_KEY"));

        private static readonly Uri endpoint =
            new Uri(Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT"));

        private static readonly TextAnalyticsClient client =
            new TextAnalyticsClient(endpoint, credentials);

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                ModelState.AddModelError("", "Будь ласка, введіть текст.");
                return View();
            }

            var result = new List<NamedEntityResult>();
            var response = client.RecognizeEntities(inputText);

            foreach (var entity in response.Value)
            {
                result.Add(new NamedEntityResult
                {
                    Text = entity.Text,
                    Category = (string)entity.Category,
                    SubCategory = entity.SubCategory,
                    ConfidenceScore = (int?)(entity.ConfidenceScore * 100)
                });
            }

            return View("Result", result);
        }
    }
}
