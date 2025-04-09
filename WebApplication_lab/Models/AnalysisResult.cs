namespace WebApplication_lab.Models
{
    public class AnalysisResult
    {
        public string OriginalText { get; set; }
        public string ProcessedText { get; set; } // PII: ***, медичні дані: підкреслені
        public List<EntityInfo> MedicalEntities { get; set; } = new List<EntityInfo>();
        public List<PiiEntityInfo> HiddenPii { get; set; } = new List<PiiEntityInfo>();
    }

    public class EntityInfo
    {
        public string Text { get; set; }
        public string Category { get; set; } // "Diagnosis", "Medication" тощо
        public int Confidence { get; set; }  // 0-100%
    }

    public class PiiEntityInfo
    {
        public string Text { get; set; }     // Оригінальний текст (наприклад, "Іван Петренко")
        public string Category { get; set; } // "Person", "Email" тощо
        public int Confidence { get; set; }
    }
}