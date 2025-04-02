using Azure.AI.TextAnalytics;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LanguageBasedDocumentClassifier
{
	public class LangBasedDocumentClassifier
	{
		private readonly ILogger<LangBasedDocumentClassifier> _logger;
		private readonly TextAnalyticsClient _textAnalyticsClient;
		private readonly BlobServiceClient _blobServiceClient;
		private readonly BlobContainerClient _blobSourceContainerClient;
		private readonly BlobContainerClient _blobDestinationContainerClient;

		public LangBasedDocumentClassifier(ILogger<LangBasedDocumentClassifier> logger,
										   TextAnalyticsClient textAnalyticsClient,
										   BlobServiceClient blobServiceClient)
		{
			_logger = logger;
			_textAnalyticsClient = textAnalyticsClient;
			_blobServiceClient = blobServiceClient;
			_blobSourceContainerClient = _blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("sourceContainerName"));
			_blobDestinationContainerClient = _blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("targetContainerName"));
		}

		[FunctionName("LangBasedDocumentClassifier")]
		public async Task Run(
			[BlobTrigger("source/{name}", Connection = "blobConn")] Stream stream,
			string name)
		{
			try
			{
				using var blobStreamReader = new StreamReader(stream);
				var content = await blobStreamReader.ReadToEndAsync();
				_logger.LogInformation($"C# Blob Trigger processed blob\n Name: {name}\n Data: {content}");

				var detectedLanguage = await _textAnalyticsClient.DetectLanguageAsync(content);
				var languageName = detectedLanguage.Value.Name;
				_logger.LogInformation($"Detected language: {languageName}");

				string targetBlobName = $"{languageName}/{name}";
				BlobClient blobClient = _blobDestinationContainerClient.GetBlobClient(targetBlobName);

				// Ensure proper cleanup of the MemoryStream
				byte[] byteArray = Encoding.UTF8.GetBytes(content);
				using (var memoryStream = new MemoryStream(byteArray))
				{
					await blobClient.UploadAsync(memoryStream);
				}

				_logger.LogInformation($"Uploaded blob - {targetBlobName} in the {Environment.GetEnvironmentVariable("targetContainerName")} container.");

				// Deleting the blob from the source container after processing
				await _blobSourceContainerClient.DeleteBlobIfExistsAsync(name);
				_logger.LogInformation($"Deleted blob - {name} from {Environment.GetEnvironmentVariable("sourceContainerName")} container.");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error processing blob {name}: {ex.Message}");
			}
		}
	}
}
