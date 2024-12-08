using System.ComponentModel.DataAnnotations;

namespace GP_MVC.Models
{
    public class ApplicationUserTerm
    { 

        [Key]
        public string UserId { get; set; }
        [Key]
        public int TermId { get; set; }

        public ApplicationUser User { get; set; }
        public Term Term { get; set; }
    }
}
