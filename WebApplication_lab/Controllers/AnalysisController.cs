using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebApplication_lab.Models;


namespace WebApplication_lab.Controllers
{
    public class AnalysisController : Controller
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
                var result = new AnalysisResult { OriginalText = inputText };

                // Крок 1: Приховуємо PII
                var piiResponse = await client.RecognizePiiEntitiesAsync(inputText);
                result.ProcessedText = ReplacePiiWithStars(inputText, piiResponse.Value, result.HiddenPii);

                // Крок 2: Аналізуємо медичні сутності
                var healthOperation = await client.StartAnalyzeHealthcareEntitiesAsync(new List<string> { result.ProcessedText });
                await healthOperation.WaitForCompletionAsync();

                // Отримуємо результати
                await foreach (var documentResults in healthOperation.Value)
                {
                    foreach (var entityInDoc in documentResults)
                    {
                        if (!entityInDoc.HasError)
                        {
                            // Додаємо медичні сутності
                            foreach (var entity in entityInDoc.Entities)
                            {
                                result.MedicalEntities.Add(new EntityInfo
                                {
                                    Text = entity.Text,
                                    Category = entity.Category.ToString(),
                                    Confidence = (int)(entity.ConfidenceScore * 100)
                                });
                            }

                            // Виділяємо медичні терміни в тексті
                            result.ProcessedText = HighlightMedicalTerms(result.ProcessedText, entityInDoc.Entities);
                        }
                    }
                }

                return View("Result", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Помилка: {ex.Message}");
                return View();
            }
        }

        private string ReplacePiiWithStars(string text, PiiEntityCollection piiEntities, List<PiiEntityInfo> hiddenPii)
        {
            var sb = new StringBuilder(text);
            var piiCounters = new Dictionary<string, int>();

            foreach (var entity in piiEntities.OrderByDescending(e => e.Offset))
            {
                string category = entity.Category.ToString();
                if (!piiCounters.ContainsKey(category))
                    piiCounters[category] = 1;
                else
                    piiCounters[category]++;

                // Заміна на категорію
                string replacement = $"<b>{category}_{piiCounters[category]}</b>"; // Жирний шрифт для категорії

                sb.Remove(entity.Offset, entity.Length).Insert(entity.Offset, replacement);

                hiddenPii.Add(new PiiEntityInfo
                {
                    Text = entity.Text,
                    Category = category,
                    Confidence = (int)(entity.ConfidenceScore * 100)
                });
            }

            return sb.ToString();
        }

        private string HighlightMedicalTerms(string text, IReadOnlyCollection<HealthcareEntity> medicalEntities)
        {
            var sb = new StringBuilder(text);
            foreach (var entity in medicalEntities.OrderByDescending(e => e.Offset))
            {
                // Виділення категорії медичного терміну жирним шрифтом
                string prefix = $"<b>[{entity.Category}]</b> ";
                sb.Insert(entity.Offset, prefix); // Вставляємо перед медичним терміном
            }
            return sb.ToString();
        }
    }
}