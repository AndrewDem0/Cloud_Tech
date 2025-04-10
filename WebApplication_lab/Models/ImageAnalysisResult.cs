
namespace WebApplication_lab.Models
{
    public class ImageAnalysisResult
    {
        public IFormFile ImageFile { get; set; }
        public string Description { get; set; }
        public float Confidence { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string ImageUrl { get; set; }


    }
}
