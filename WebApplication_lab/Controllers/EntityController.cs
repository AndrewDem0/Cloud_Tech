using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using System.IO;
using System.Text;
using WebApplication_lab.Models;
using System.Linq;
using Azure.Storage.Blobs.Models;
using Azure.AI.TextAnalytics;
using Azure;

namespace WebApplication_lab.Controllers
{
    public class EntityController : Controller
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

            var response = client.RecognizeLinkedEntities(inputText);
            var result = new List<EntityLinkingResult>();
            var seenEntities = new HashSet<string>();

            foreach (var entity in response.Value)
            {
                var entityUrl = entity.Url?.ToString();
                var entityName = entity.Name;

                if (!string.IsNullOrEmpty(entityUrl) && !seenEntities.Contains(entityName + entityUrl))
                {
                    seenEntities.Add(entityName + entityUrl);

                    var matchedText = entity.Matches.Any() ? entity.Matches.First().Text : string.Empty;

                    var model = new EntityLinkingResult
                    {
                        Name = entityName,
                        Url = entityUrl,
                        DataSource = entity.DataSource,
                        MatchedText = matchedText,
                        ConfidenceScore = entity.Matches.Any() ? (int)(entity.Matches.First().ConfidenceScore * 100) : 0

                    };

                    result.Add(model);
                }
            }

            return View("Result", result);
        }

        [HttpPost]
        public IActionResult DownloadResults(List<EntityLinkingResult> model)
        {
            try
            {
                if (model == null || !model.Any())
                {
                    ModelState.AddModelError("", "Немає результатів для завантаження.");
                    return RedirectToAction("Index");
                }

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Назва\tПосилання\tДжерело\tЗбіги\tТочність");

                foreach (var item in model)
                {
                    stringBuilder.AppendLine($"{item.Name}\t{item.Url}\t{item.DataSource}\t{item.MatchedText}\t{item.ConfidenceScore}");
                }

                var fileContent = stringBuilder.ToString();
                var fileName = "entity_linking_results.txt";

                var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                if (string.IsNullOrEmpty(connectionString))
                    throw new Exception("AZURE_STORAGE_CONNECTION_STRING is empty or not set");

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("resource");

                if (!containerClient.Exists())
                    throw new Exception("Container 'resource' does not exist.");

                var blobClient = containerClient.GetBlobClient(fileName);

                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
                {
                    blobClient.Upload(memoryStream, overwrite: true);
                }

                return RedirectToAction("DownloadFile", new { fileName });
            }
            catch (Exception ex)
            {
                return Content($"Exeption Blob: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"));
            var containerClient = blobServiceClient.GetBlobContainerClient("resource");
            var blobClient = containerClient.GetBlobClient(fileName);

            BlobDownloadInfo download = blobClient.Download();

            using (var memoryStream = new MemoryStream())
            {
                download.Content.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();

                return File(fileBytes, "text/plain", fileName);
            }
        }
    }
}
