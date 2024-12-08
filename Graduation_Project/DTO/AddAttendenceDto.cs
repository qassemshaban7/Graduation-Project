using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class AddAttendenceDto
    {
        [Required]
        public string ImageName { get; set; }
        //[Required]
        //public int Value { get; set; }
    }
}
