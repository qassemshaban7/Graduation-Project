using System.ComponentModel.DataAnnotations;

namespace GP_MVC.ViewModels
{
    public class EditTeacherVM
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National Number must be 14 digits")]
        public string NationalNum { get; set; }
        public IFormFile? Image { get; set; }
        public string SubjectName { get; set; }
        public List<Int32> AssignClassId { get; set; }
    }
}
