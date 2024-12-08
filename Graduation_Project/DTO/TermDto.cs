using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class TermDto
    {
        [Required]
        public int Index { get; set; }
        [Required]
        public string TermName { get; set; }  
        public DateTime EndDate { get; set; }
    }
}
