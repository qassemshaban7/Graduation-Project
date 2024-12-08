using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP_MVC.Models
{
    public class Term
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string TermName { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        [ForeignKey("YearId")]
        public int YearId { get; set; }
        public Year Year { get; set; }
        public ICollection<Material> Materials { get; set; }
        public ICollection<ApplicationUserTerm> ApplicationUserTerms { get; set; }
    }
}

