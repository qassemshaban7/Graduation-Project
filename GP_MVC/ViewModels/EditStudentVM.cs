using System.ComponentModel.DataAnnotations;

namespace GP_MVC.ViewModels
{
    public class EditStudentVM
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

        [Required(ErrorMessage = "Year is required")]
        public int YearId { get; set; }

        [Required(ErrorMessage = "Class is required")]
        public int PClassId { get; set; }
    }
}
