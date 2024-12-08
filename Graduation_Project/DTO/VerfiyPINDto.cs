using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class VerfiyPINDto
    {
        //[Required]
        //[EmailAddress]
        //public string Email { get; set; }
        [Required]
        public int pin { get; set; }
    }
}
