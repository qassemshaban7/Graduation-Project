using System.ComponentModel.DataAnnotations.Schema;

namespace GP_MVC.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }

        public string? FirstAnswer { get; set; }
        public string? SecondAnswer { get; set; }
        public string? ThirdAnswer { get; set; }
        public string? FourthAnswer { get; set; }

        public int SurveyId { get; set; }
        [ForeignKey("SurveyId")]
        public Survey Survey { get; set; }
    }
}
