namespace GP_MVC.ViewModels
{
    public class StudentGradesDetailsViewModel
    {
        public string MaterialName { get; set; }
        public int MaterialGrade { get; set; }
        public double StudentTotalGrade { get; set; }
        public List<ExamViewModel> Exams { get; set; }
    }
}
