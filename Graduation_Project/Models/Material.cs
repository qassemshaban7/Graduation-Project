using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int M_grade { get; set; }
        public ICollection<Exam> Exams { get; set; }
        [Required]
        public int TermId { get; set; }
        public Term Term { get; set; }
    }
}
