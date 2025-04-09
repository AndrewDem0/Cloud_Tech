using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApplication_lab.Controllers
{
    public class TranslationController : Controller
    {
        private readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";
        private readonly string region = "westeurope"; // напр. "westeurope"
        private readonly string key = "BOKrgN80ogGzsVOK0yWNHCNKSMfO6IQeofDeuC32BeMjGSqOege8JQQJ99BDAC5RqLJXJ3w3AAAbACOGHce6";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Translate(TranslationRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.InputText) || string.IsNullOrWhiteSpace(model.TargetLanguage))
            {
                ModelState.AddModelError("", "Please enter the text and target language.");
                return View("Index", model);
            }

            // Побудова маршруту
            string route = $"/translate?api-version=3.0&to={model.TargetLanguage}";

            // Тіло запиту
            object[] body = new object[] { new { Text = model.InputText } };
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

                    // Оцінка достовірності
                    var confidence = 100; // Це приклад, зазвичай для перекладу не використовують confidence score, але можна додати за потреби

                    model.Result = translatedText;
                    model.Confidence = confidence;

                    return View("Result", model);
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

            return View("Index", model);
        }
    }
}
