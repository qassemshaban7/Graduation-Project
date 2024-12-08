using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class EditTeacherDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        //[EmailAddress]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National Number must be 14 digits")]
        public string NationalNum { get; set; }
        public IFormFile? Image { get; set; }
        [Required]
        public string SubjectName { get; set; }
        public List<Int32>? AssignClassId { get; set; }
    }
}