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
            return View();  // Відображаємо вигляд "Index.cshtml"
        }

        [HttpPost]
        public IActionResult Index(string inputText)
        {
            // Перевірка на порожній текст
            if (string.IsNullOrEmpty(inputText))
            {
                ModelState.AddModelError("", "Будь ласка, введіть текст.");
                return View();
            }

            // Розпізнавання Pii в тексті
            var response = client.RecognizePiiEntities(inputText);

            // Модель результату
            var piiResult = new PiiResultModel
            {
                RedactedText = response.Value.RedactedText,  // Текст після заміни PII
                Entities = new List<string>()
            };

            // Додаємо всі знайдені сутності до списку
            foreach (var entity in response.Value)
            {
                piiResult.Entities.Add(entity.Text);
            }

            // Повертаємо результат на вигляд "Result"
            return View("Result", piiResult);
        }
    }
}
