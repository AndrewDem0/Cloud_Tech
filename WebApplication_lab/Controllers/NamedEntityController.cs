using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System.Collections.Generic;
using System;

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
            return View();  // Відображаємо вигляд "Index.cshtml"
        }

        [HttpPost]
        public IActionResult Index(string inputText, List<string> selectedCategories, List<string> showOptions)
        {
            // Перевірка на порожній текст
            if (string.IsNullOrEmpty(inputText))
            {
                ModelState.AddModelError("", "Будь ласка, введіть текст.");
                return View();
            }

            // Список для результатів
            var result = new List<NamedEntityResult>();

            // Розпізнавання сутностей в тексті
            var response = client.RecognizeEntities(inputText);

            // Групуємо сутності за категорією
            var groupedResults = response.Value
                .Where(entity => selectedCategories.Contains(entity.Category.ToString()))
                .GroupBy(entity => entity.Category.ToString())
                .ToList();

            // Проходимо по кожній сутності
            foreach (var entity in response.Value)
            {
                // Перевіряємо чи вибрана категорія для виведення
                if (selectedCategories.Contains(entity.Category.ToString()))
                {
                    result.Add(new NamedEntityResult
                    {
                        Text = entity.Text,
                        Category = entity.Category.ToString(),
                        // Якщо вибрано "Показати підкатегорії", додаємо SubCategory
                        SubCategory = showOptions.Contains("SubCategory") ? entity.SubCategory : null,
                        ConfidenceScore = showOptions.Contains("Confidence") ? (int?)(entity.ConfidenceScore * 100) : null
                    });
                }
            }

            // Передаємо параметр для показу точності в View
            ViewBag.ShowConfidence = showOptions.Contains("Confidence");
            ViewBag.ShowSubCategory = showOptions.Contains("SubCategory");

            // Повертаємо результати на вигляд "Result"
            return View("Result", result);
        }
    }
}
