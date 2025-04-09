using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApplication_lab.Controllers
{
    public class TranslationController : Controller
    {
        private readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";
        private readonly string region = "westeurope"; // напр. "westeurope"
        private readonly string key = "BOKrgN80ogGzsVOK0yWNHCNKSMfO6IQeofDeuC32BeMjGSqOege8JQQJ99BDAC5RqLJXJ3w3AAAbACOGHce6";

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var languages = await GetLanguagesAsync();
            ViewBag.Languages = languages; // Додано передачу списку мов
            return View(new TranslationRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Translate(TranslationRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.InputText1) || string.IsNullOrWhiteSpace(model.SourceLanguage) || string.IsNullOrWhiteSpace(model.TargetLanguage))
            {
                ModelState.AddModelError("", "Please enter the text and select source and target languages.");
                var languages = await GetLanguagesAsync();
                ViewBag.Languages = languages;
                return View("Index", model);
            }

            // Побудова маршруту для перекладу
            string route = $"/translate?api-version=3.0&from={model.SourceLanguage}&to={model.TargetLanguage}";

            // Тіло запиту
            object[] body = new object[] { new { Text = model.InputText1 } };
            string requestBody = JsonSerializer.Serialize(body);

            using var client = new HttpClient();
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(endpoint + route),
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            request.Headers.Add("Ocp-Apim-Subscription-Region", region);

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var translationResult = JsonSerializer.Deserialize<JsonElement>(result);
                    var translatedText = translationResult[0]
                        .GetProperty("translations")[0]
                        .GetProperty("text").GetString();

                    model.Result = translatedText;

                    // Оцінка довіри, тут ми використовуємо умовний приклад
                    model.Confidence = 100; // Це можна змінити на реальну оцінку довіри, якщо доступно з API

                    // Повертаємо результат на тій самій сторінці
                    var languages = await GetLanguagesAsync();
                    ViewBag.Languages = languages;
                    return View("Index", model);
                }
                else
                {
                    ModelState.AddModelError("", "Translation failed.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            var languagesFallback = await GetLanguagesAsync();
            ViewBag.Languages = languagesFallback;
            return View("Index", model);
        }

        private async Task<Dictionary<string, string>> GetLanguagesAsync()
        {
            var languages = new Dictionary<string, string>();
            string route = "/languages?api-version=3.0&scope=translation";

            using var client = new HttpClient();
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(endpoint + route)
            };

            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            request.Headers.Add("Ocp-Apim-Subscription-Region", region);

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(result);
                    var translation = json.GetProperty("translation");

                    foreach (var lang in translation.EnumerateObject())
                    {
                        string name = lang.Value.GetProperty("name").GetString();
                        languages.Add(lang.Name, name);
                    }
                }
            }
            catch (Exception ex)
            {
                // Логування помилки
                Console.WriteLine($"Error getting languages: {ex.Message}");
            }

            return languages;
        }
    }
}
