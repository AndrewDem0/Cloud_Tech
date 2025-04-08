using WebApplication_lab.Models;

namespace WebApplication_lab.Services
{
    // Services/HealthService.cs
    public class HealthService : BaseAzureAnalysisService
    {
        public HealthService(HttpClient httpClient) : base(httpClient) { }

        public override async Task<object> AnalyzeAsync(string text)
        {
            return new HealthResultModel
            {
                Entities = new List<string> { "Ibuprofen", "Headache" },
                Categories = new List<string> { "Medication", "Symptom" }
            };
        }
    }

}
