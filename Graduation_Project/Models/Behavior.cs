using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class Behavior
    {
        public int Id { get; set; }
        public int Behavior_Value { get; set; }
        public DateTime Time { get; set; }

        //[ForeignKey("SudentId")]
        //public string SudentId { get; set; }
        //public ApplicationUser ApplicationUser { get; set; }
    }
}