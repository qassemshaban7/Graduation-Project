namespace GP_MVC.ViewModels
{
    public class ExamVM
    {
        public string Name { get; set; }
        public double Exam_Grade { get; set; }
        public int MaterialId { get; set; }
        public IFormFile Image { get; set; }
    }
}
