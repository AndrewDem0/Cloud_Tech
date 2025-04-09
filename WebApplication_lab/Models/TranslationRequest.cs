namespace WebApplication_lab.Models
{
    public class TranslationRequest
    {
        public string InputText1 { get; set; }
        public string InputText2 { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string Result { get; set; }
        public int Confidence { get; set; }  // 0-100%
    }
}
