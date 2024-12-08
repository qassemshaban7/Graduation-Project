using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Exam_Grade { get; set; }
        public string Image { get; set; }
        [Required]
        public int MaterialId { get; set; } 
        public Material Material { get; set; }
        public ICollection<StudentGrade> StudentGrades { get; set; }
    }
}
