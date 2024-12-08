using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class SendPINDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
