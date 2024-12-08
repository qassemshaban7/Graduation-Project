using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Models
{
    public class ApplicationUserYear
    {
        [Key]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Key]
        public int YearId { get; set; }
        public Year Year { get; set; }
    }
}
