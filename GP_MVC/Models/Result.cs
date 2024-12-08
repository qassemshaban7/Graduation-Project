namespace GP_MVC.Models
{
    public class Result
    {
        public int Id { get; set; }
        public string Answer { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int SurveyId { get; set; }
        public Survey Survey { get; set; }
    }
}
