using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class AddBehaviorDto
    {
        [Required]
        public int Behavior_Value { get; set; }
    }
}
