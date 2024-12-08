using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class LoginUserDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
} 
