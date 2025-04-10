namespace WebApplication_lab.Models
{
    public class OcrResultModel
    {
        public IFormFile ImageFile { get; set; }
        public string ImageUrl { get; set; }
        public string ExtractedText { get; set; }
        public double Confidence { get; set; } // Від 0 до 100
    }
}
