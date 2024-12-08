using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class StudentGradeDto
    {
        [Required]
        public double Student_Grade { get; set; }
        //public int ExamId { get; set; }
        //public string ApplicationUserId { get; set; }
    }
}
