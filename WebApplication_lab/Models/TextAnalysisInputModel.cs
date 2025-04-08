using System.ComponentModel.DataAnnotations;


namespace WebApplication_lab.Models
{
    // Models/TextAnalysisInputModel.cs

    public class TextAnalysisInputModel
    {
        [Required(ErrorMessage = "Поле не може бути порожнім")]
        public string Text { get; set; }
    }

}
