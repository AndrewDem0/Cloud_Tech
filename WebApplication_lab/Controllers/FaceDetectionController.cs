using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.IO;
using System.Threading.Tasks;
using WebApplication_lab.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebApplication_lab.Controllers
{
    public class FaceDetectionController : Controller
    {
        private readonly IFaceClient _faceClient;
        private readonly string _subscriptionKey = Environment.GetEnvironmentVariable("FACE_APIKEY") ?? "<apikey>";
        private readonly string _endpoint = Environment.GetEnvironmentVariable("FACE_ENDPOINT") ?? "<endpoint>";

        public FaceDetectionController()
        {
            _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_subscriptionKey))
            {
                Endpoint = _endpoint
            };
        }

        [HttpGet]
        public IActionResult Index() => View(new FaceDetectionResult());

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
                return View(new FaceDetectionResult());

            var result = new FaceDetectionResult();

            using (var stream = ImageFile.OpenReadStream())
            {
                var faces = await _faceClient.Face.DetectWithStreamAsync(
                    stream,
                    returnFaceId: false,
                    returnFaceLandmarks: false,
                    returnFaceAttributes: null,
                    detectionModel: DetectionModel.Detection01
                );

                result.FaceCount = faces.Count;

                foreach (var face in faces)
                {
                    var rect = face.FaceRectangle;
                    result.Faces.Add(new FaceInfo
                    {
                        Left = rect.Left,
                        Top = rect.Top,
                        Width = rect.Width,
                        Height = rect.Height,
                        Confidence = 1.0 // API не повертає точність для Detect
                    });
                }
            }

            // Зберігаємо тимчасово файл для відображення
            var fileName = Path.GetRandomFileName() + Path.GetExtension(ImageFile.FileName);
            var filePath = Path.Combine("wwwroot", "uploads", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
                await ImageFile.CopyToAsync(fileStream);

            result.ImageUrl = $"/uploads/{fileName}";
            return View(result);
        }
    }
}
