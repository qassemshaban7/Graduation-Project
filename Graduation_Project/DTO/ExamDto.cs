using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class ExamDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public double Exam_Grade { get; set; }
        [Required]
        public int MaterialId { get; set; }
        [Required]
        public IFormFile Image { get; set; }
    }
}
