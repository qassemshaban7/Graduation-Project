using System.ComponentModel.DataAnnotations.Schema;

namespace GP_MVC.Models
{
    public class StudentGrade
    {
        public int Id { get; set; }
        public double Student_Grade { get; set; }
        public Exam Exam { get; set; }
        //public string Image { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
