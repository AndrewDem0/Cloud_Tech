using WebApplication_lab.Models;

namespace WebApplication_lab.Services
{
    // Services/PiiService.cs
    public class PiiService : BaseAzureAnalysisService
    {
        public PiiService(HttpClient httpClient) : base(httpClient) { }

        public override async Task<object> AnalyzeAsync(string text)
        {
            // ТУТ ПІЗНІШЕ — виклик до Azure API
            return new PiiResultModel
            {
                RedactedText = "[MASKED TEXT]",
                Entities = new List<string> { "Phone", "Email" }
            };
        }
    }

}
