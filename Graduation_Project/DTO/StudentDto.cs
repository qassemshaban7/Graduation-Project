using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class StudentDto
    {
        [Required]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National Number must be 14 digits")]
        public string NationalNum { get; set; }
        public IFormFile? Image { get; set; }
        [Required]
        public int PClassId { get; set; }
        [Required]
        public int YearId { get; set; } 
    }
}