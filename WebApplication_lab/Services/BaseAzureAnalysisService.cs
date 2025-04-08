namespace WebApplication_lab.Services
{
    // Services/BaseAzureAnalysisService.cs
    public abstract class BaseAzureAnalysisService
    {
        protected readonly HttpClient _httpClient;

        protected BaseAzureAnalysisService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public abstract Task<object> AnalyzeAsync(string text);
    }

}
