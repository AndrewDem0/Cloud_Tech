using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    // Замінити на свій ключ та endpoint від Face API (не OCR!)
    static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("FACE_APIKEY") ?? "<apikey>";
    static readonly string Endpoint = Environment.GetEnvironmentVariable("FACE_ENDPOINT") ?? "<endpoint>";

    static async Task Main(string[] args)
    {
        IFaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
        {
            Endpoint = Endpoint
        };

        // Шляхи до фото
        string imagePath1 = @"C:\work prog\ASPCoreMVC\Cloud_Tech\WebApplication_lab\photo\Face.jpg";
        string imagePath2 = @"C:\work prog\ASPCoreMVC\Cloud_Tech\WebApplication_lab\photo\photo_2025-04-10_13-34-55.jpg";

        Console.WriteLine("=== Аналіз фото 1 ===");
        await CheckFaceAsync(faceClient, imagePath1);

        Console.WriteLine("\n=== Аналіз фото 2 ===");
        await CheckFaceAsync(faceClient, imagePath2);
    }

    static async Task CheckFaceAsync(IFaceClient client, string imagePath)
    {
        try
        {
            using Stream imageStream = File.OpenRead(imagePath);
            var detectedFaces = await client.Face.DetectWithStreamAsync(
                imageStream,
                returnFaceId: false,
                detectionModel: DetectionModel.Detection01
            );

            if (detectedFaces.Count == 0)
                Console.WriteLine($"❌ Обличчя не знайдено у: {Path.GetFileName(imagePath)}");
            else
                Console.WriteLine($"ТОЧНО Знайдено {detectedFaces.Count} облич у: {Path.GetFileName(imagePath)}");
        }
        catch (APIErrorException ex)
        {
            Console.WriteLine("🚫 Помилка доступу до Face API:");
            Console.WriteLine(ex.Body.Error.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠️ Сталася помилка:");
            Console.WriteLine(ex.Message);
        }
    }
}
