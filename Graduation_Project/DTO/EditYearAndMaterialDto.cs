using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
    public class EditYearAndMaterialDto
    {
        [Required]
        public int Index { get; set; }
        [Required]
        public string YearName { get; set; }
        public List<string>? AddFirstSemesterMaterial { get; set; }
        public List<string>? AddSecondSemesterMaterial { get; set; }
    }
}
