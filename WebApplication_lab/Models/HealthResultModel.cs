namespace WebApplication_lab.Models
{
    public class HealthResultModel
    {
        public List<string> Entities { get; set; }  // Список знайдених сутностей
        public List<string> Categories { get; set; }  // Список категорій для кожної сутності
    }
}
