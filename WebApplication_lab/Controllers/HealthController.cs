using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication_lab.Controllers
{
    public class HealthController : Controller
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
        public async Task<IActionResult> Index(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                ModelState.AddModelError("", "Будь ласка, введіть текст.");
                return View();
            }

            try
            {
                var healthResult = await AnalyzeHealthText(inputText);
                return View("Result", healthResult);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Помилка аналізу: {ex.Message}");
                return View();
            }
        }

        private async Task<HealthResultModel> AnalyzeHealthText(string text)
        {
            var result = new HealthResultModel
            {
                Entities = new List<string>(),
                Categories = new List<string>(),
                ConfidenceScores = new List<int>()
            };

            List<string> batchInput = new List<string>() { text };
            AnalyzeHealthcareEntitiesOperation healthOperation = await client.StartAnalyzeHealthcareEntitiesAsync(batchInput);
            await healthOperation.WaitForCompletionAsync();

            await foreach (var documentsInPage in healthOperation.Value)
            {
                foreach (var entitiesInDoc in documentsInPage)
                {
                    if (!entitiesInDoc.HasError)
                    {
                        foreach (var entity in entitiesInDoc.Entities)
                        {
                            result.Entities.Add(entity.Text);
                            result.Categories.Add(entity.Category.ToString());
                            result.ConfidenceScores.Add((int)(entity.ConfidenceScore * 100));
                        }
                    }
                }
            }

            return result;
        }
    }
}