using Graduation_Project.DTO;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class YearMaterialDto
    {
        [Required]
        public int Index { get; set; }
        [Required]
        public string YearName { get; set; }
        public List<string>? FirstSemesterMaterial { get; set; }
        public List<string>? SecondSemesterMaterial { get; set; } 
    }
}