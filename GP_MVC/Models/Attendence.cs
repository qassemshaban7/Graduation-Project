using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP_MVC.Models
{
    public class Attendence
    {
        public int Id { get; set; }

        public string PartOne { get; set; }
        public string PartTwo { get; set; }
        public int Total { get; set; } // 0  / 1  / 2
        //public string? Excuse { get; set; }
        public DateOnly Date_Day { get; set; }
   
        public ApplicationUser ApplicationUser { get; set; }
    }
}
