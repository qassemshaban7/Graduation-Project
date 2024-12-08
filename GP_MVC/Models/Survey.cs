using System.ComponentModel.DataAnnotations;

namespace GP_MVC.Models
{
    public class Survey
    {
        public int Id { get; set; }
        [Required]
        public string SurveyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<Result> Result { get; set; }
    }
}
