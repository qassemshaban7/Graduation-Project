using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class AddStudentGradeDto
    {
        [Required]
        public double Student_Grade { get; set; }
    }
}
