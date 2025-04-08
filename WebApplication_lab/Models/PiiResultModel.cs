namespace WebApplication_lab.Models
{
    public class PiiResultModel
    {
        public string RedactedText { get; set; }
        public List<string> Entities { get; set; }
        public string Category { get; set; }
        public int? ConfidenceScore { get; set; }
    }
}
