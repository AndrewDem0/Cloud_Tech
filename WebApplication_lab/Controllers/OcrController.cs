using Microsoft.AspNetCore.Mvc;
using WebApplication_lab.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebApplication_lab.Controllers
{
    public class OcrController : Controller
    {
        private readonly string endpoint = "https://ocr2tasks.cognitiveservices.azure.com/";
        private readonly string key = "FXrb922zuFRmqXZSEnBqaUtueDdiqIdb7SKQIkiJrY2DbBwV60BpJQQJ99BDAC5RqLJXJ3w3AAAFACOGP8hN";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new OcrResultModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(OcrResultModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload an image file.");
                return View(model);
            }

            using (var ms = new MemoryStream())
            {
                await model.ImageFile.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                model.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(imageBytes);

                var uri = $"{endpoint}vision/v3.2/read/analyze";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                using var content = new ByteArrayContent(imageBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await client.PostAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "OCR request failed.");
                    return View(model);
                }

                string operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();

                // Polling
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);

                    var resultResponse = await client.GetAsync(operationLocation);
                    var jsonResult = await resultResponse.Content.ReadAsStringAsync();
                    var result = JsonDocument.Parse(jsonResult).RootElement;

                    if (result.GetProperty("status").GetString() == "succeeded")
                    {
                        var lines = result.GetProperty("analyzeResult")
                                          .GetProperty("readResults")[0]
                                          .GetProperty("lines");

                        double avgConfidence = 0;
                        int wordsCount = 0;
                        string resultText = "";

                        foreach (var line in lines.EnumerateArray())
                        {
                            resultText += line.GetProperty("text").GetString() + "\n";

                            foreach (var word in line.GetProperty("words").EnumerateArray())
                            {
                                if (word.TryGetProperty("confidence", out var conf))
                                {
                                    avgConfidence += conf.GetDouble();
                                    wordsCount++;
                                }
                            }
                        }

                        model.ExtractedText = resultText.Trim();
                        model.Confidence = wordsCount > 0 ? Math.Round((avgConfidence / wordsCount) * 100, 2) : 0;
                        return View(model);
                    }
                }

                ModelState.AddModelError("", "OCR processing timed out.");
                return View(model);
            }
        }
    }
}
