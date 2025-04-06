using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.TextAnalytics;
using WebApplication_lab.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication_lab.Controllers
{
    public class EntityController : Controller
    {
        // Ініціалізація клієнта Azure для аналізу сутностей
        private static readonly AzureKeyCredential credentials =
            new AzureKeyCredential(Environment.GetEnvironmentVariable("LANGUAGE_KEY"));

        private static readonly Uri endpoint =
            new Uri(Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT"));

        private static readonly TextAnalyticsClient client =
            new TextAnalyticsClient(endpoint, credentials);

        // GET-метод для завантаження головної сторінки
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST-метод для обробки введеного тексту
        [HttpPost]
        public IActionResult Index(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                ModelState.AddModelError("", "Будь ласка, введіть текст.");
                return View();
            }

            // Викликаємо API Azure для розпізнавання зв'язаних сутностей
            var response = client.RecognizeLinkedEntities(inputText);
            var result = new List<EntityLinkingResult>();

            // Використовуємо HashSet для унікальних посилань (по URL та Name)
            var seenEntities = new HashSet<string>();

            // Обробляємо отриманий список сутностей
            foreach (var entity in response.Value)
            {
                var entityUrl = entity.Url?.ToString();
                var entityName = entity.Name;

                // Якщо ми ще не додавали таку сутність, додаємо її
                if (!string.IsNullOrEmpty(entityUrl) && !seenEntities.Contains(entityName + entityUrl))
                {
                    seenEntities.Add(entityName + entityUrl); // додавання URL та Name як унікального ідентифікатора

                    // Перевірка наявності хоча б одного збігу
                    var matchedText = entity.Matches.Any() ? entity.Matches.First().Text : string.Empty;

                    // Створюємо модель результату лише для першого збігу сутності
                    var model = new EntityLinkingResult
                    {
                        Name = entity.Name,
                        Url = entityUrl,  // Переводимо Url в строку
                        DataSource = entity.DataSource,
                        MatchedText = matchedText,  // Вибираємо перший збіг або порожній рядок
                    };

                    result.Add(model);
                }
            }

            // Повертаємо результат на сторінку "Result"
            return View("Result", result);
        }
    }
}
