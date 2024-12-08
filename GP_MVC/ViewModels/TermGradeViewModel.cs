namespace GP_MVC.ViewModels
{
    public class TermGradeViewModel
    {
        public int TermId { get; set; }
        public string TermName { get; set; }
        public List<MaterialGradeViewModel> MaterialGrades { get; set; }
    }
}
