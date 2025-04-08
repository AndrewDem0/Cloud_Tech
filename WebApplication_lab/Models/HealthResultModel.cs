namespace WebApplication_lab.Models
{
    public class HealthResultModel
    {
        public List<string> Entities { get; set; }      // Список знайдених сутностей
        public List<string> Categories { get; set; }    // Категорії сутностей
        public List<int> ConfidenceScores { get; set; } // Відсоткова ймовірність для кожної сутності
    }
}