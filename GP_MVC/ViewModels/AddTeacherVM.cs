using System.ComponentModel.DataAnnotations;

namespace GP_MVC.ViewModels
{
    public class AddTeacherVM
    {
        [Required]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National Number must be 14 digits")]
        public string NationalNum { get; set; }
        public IFormFile Image { get; set; }
        public string SubjectName { get; set; }
        public List<int> AssignClassId { get; set; } = new List<int>();
    }
}
