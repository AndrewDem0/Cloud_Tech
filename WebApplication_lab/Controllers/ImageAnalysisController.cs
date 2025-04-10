using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebApplication_lab.Controllers
{
    public class ImageAnalysisController : Controller
    {
        private readonly string endpoint = "https://ocr2tasks.cognitiveservices.azure.com/";
        private readonly string key = "FXrb922zuFRmqXZSEnBqaUtueDdiqIdb7SKQIkiJrY2DbBwV60BpJQQJ99BDAC5RqLJXJ3w3AAAFACOGP8hN";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImageAnalysisResult());
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(ImageAnalysisResult model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload an image.");
                return View("Index", model);
            }

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await model.ImageFile.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            // Зберегти для перегляду (Base64)
            model.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(imageBytes);

            // Запит до Azure AI Vision
            string uri = $"{endpoint}vision/v3.2/analyze?visualFeatures=Description,Tags";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            using var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await client.PostAsync(uri, content);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Image analysis failed.");
                return View("Index", model);
            }

            var json = JsonDocument.Parse(jsonString).RootElement;

            var captions = json.GetProperty("description").GetProperty("captions");
            if (captions.GetArrayLength() > 0)
            {
                model.Description = captions[0].GetProperty("text").GetString();
                model.Confidence = captions[0].GetProperty("confidence").GetSingle();
            }

            model.Tags = json.GetProperty("tags").EnumerateArray()
                            .Select(t => t.GetProperty("name").GetString()).ToList();

            return View("Index", model);
        }
    }
}
