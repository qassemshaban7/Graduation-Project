using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Graduation_Project.DTO
{
    public class TeacherDto 
    {
        [Required]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National Number must be 14 digits")]
        public string NationalNum { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        [Required]
        public string SubjectName { get; set; }
        public List<Int32> AssignClassId { get; set; }   
    }
}
