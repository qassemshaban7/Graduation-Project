using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class EditExamDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public double Exam_Grade { get; set; }
        [Required]
        public int MaterialId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
