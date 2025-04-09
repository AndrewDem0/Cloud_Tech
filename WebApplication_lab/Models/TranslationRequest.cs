namespace WebApplication_lab.Models
{
    public class TranslationRequest
    {
        public string InputText { get; set; }
        public string TargetLanguage { get; set; }
        public string Result { get; set; }
        public int Confidence { get; set; }  // 0-100%
    }
}
