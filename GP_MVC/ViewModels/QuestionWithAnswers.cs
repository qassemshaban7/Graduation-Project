namespace GP_MVC.ViewModels
{
    public class QuestionWithAnswers
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<Answer> Answers { get; set; }
    }
}
