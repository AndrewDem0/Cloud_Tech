using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using WebApplication_lab.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace WebApplication_lab.Controllers
{
    public class ImmersiveReaderController : Controller
    {
        private readonly string TenantId;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string Subdomain;

        private IConfidentialClientApplication _confidentialClientApplication;

        private IConfidentialClientApplication ConfidentialClientApplication
        {
            get
            {
                if (_confidentialClientApplication == null)
                {
                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(ClientId)
                        .WithClientSecret(ClientSecret)
                        .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
                        .Build();
                }

                return _confidentialClientApplication;
            }
        }

        public ImmersiveReaderController(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            TenantId = configuration["TenantId"];
            ClientId = configuration["ClientId"];
            ClientSecret = configuration["ClientSecret"];
            Subdomain = configuration["Subdomain"];

            if (string.IsNullOrWhiteSpace(TenantId))
                throw new ArgumentNullException("TenantId is null! Did you add that info to secrets.json?");
            if (string.IsNullOrWhiteSpace(ClientId))
                throw new ArgumentNullException("ClientId is null! Did you add that info to secrets.json?");
            if (string.IsNullOrWhiteSpace(ClientSecret))
                throw new ArgumentNullException("ClientSecret is null! Did you add that info to secrets.json?");
            if (string.IsNullOrWhiteSpace(Subdomain))
                throw new ArgumentNullException("Subdomain is null! Did you add that info to secrets.json?");
        }

        public async Task<string> GetTokenAsync()
        {
            const string resource = "https://cognitiveservices.azure.com/";

            var authResult = await ConfidentialClientApplication.AcquireTokenForClient(
                new[] { $"{resource}/.default" })
                .ExecuteAsync()
                .ConfigureAwait(false);

            return authResult.AccessToken;
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenAndSubdomain()
        {
            try
            {
                string token = await GetTokenAsync();
                return Json(new { token = token, subdomain = Subdomain });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error acquiring token: ", e);
                return Json(new { error = "Unable to acquire token." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string extractedText = "";

            // Завантажуємо та витягуємо текст з PDF
            using (var stream = file.OpenReadStream())
            using (var pdf = PdfDocument.Open(stream))
            {
                foreach (Page page in pdf.GetPages())
                {
                    extractedText += page.Text + "\n";
                }
            }

            string token = await GetTokenAsync();

            // Повертаємо токен, субдомен і витягнутий текст
            return Json(new
            {
                token = token,
                subdomain = Subdomain,
                content = extractedText,
                title = file.FileName
            });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                string token = await GetTokenAsync();

                // Передаємо токен і субдомен у вигляд
                var model = new ImmersiveReaderModel
                {
                    Token = token,
                    Subdomain = Subdomain,
                    Title = "Immersive Reader C# Quickstart",
                    Content = "This is a sample text demonstrating how the Immersive Reader works with your application."
                };

                return View(model);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error acquiring token: ", e);
                return Content("Unable to acquire token.");
            }
        }
    }
}
