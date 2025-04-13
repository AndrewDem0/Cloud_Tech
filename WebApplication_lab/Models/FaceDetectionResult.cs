using System.Collections.Generic;

namespace WebApplication_lab.Models
{
    public class FaceDetectionResult
    {
        public string ImageUrl { get; set; }
        public int FaceCount { get; set; }
        public List<FaceInfo> Faces { get; set; } = new();
    }

    public class FaceInfo
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Confidence { get; set; } // optional, use 1.0 for detection
    }
}
