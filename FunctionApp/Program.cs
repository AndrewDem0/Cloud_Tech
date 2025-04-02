using Azure;
using Azure.AI.TextAnalytics;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(FunctionApp.Startup))]

namespace FunctionApp
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{

			builder.Services.AddAzureClients(b =>
			{
				b.AddBlobServiceClient(Environment.GetEnvironmentVariable("blobConn"));

				var endpoint = new Uri(Environment.GetEnvironmentVariable("textAnalyticsEndpoint"));
				var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("textAnalyticsKey"));

				b.AddTextAnalyticsClient(endpoint, credential);
			});
		}
	}
}
