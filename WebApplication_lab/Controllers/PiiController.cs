using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System;
using System.Collections.Generic;

namespace WebApplication_lab.Controllers
{
    public class PiiController : Controller
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

            try
            {
                var response = client.RecognizePiiEntities(inputText);
                var piiResult = new PiiResultModel
                {
                    RedactedText = response.Value.RedactedText,
                    Entities = new List<string>(),
                    Category = "PII",
                    ConfidenceScore = null // Не використовуємо загальний ConfidenceScore
                };

                foreach (var entity in response.Value)
                {
                    piiResult.Entities.Add($"{entity.Text}|{entity.Category}|{(int)(entity.ConfidenceScore * 100)}");
                }

                return View("Result", piiResult);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Помилка аналізу: {ex.Message}");
                return View();
            }
        }
    }
}