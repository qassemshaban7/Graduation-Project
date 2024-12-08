using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Graduation_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "Invalid National Number.")]
        public string NationalNum { get; set; }
        public string Image { get; set; }
        public int? PasswordResetPin { get; set; } = null;
        public DateTime? ResetExpires { get; set; } = null;
        public ICollection<Apperance> Apperances { get; set; }
        //public ICollection<Behavior> Behaviors { get; set; }
        public ICollection<Attendence> Attendences { get; set; }
        public ICollection<StudentGrade> StudentGrades { get; set; }
        public int? PClassId { get; set; }
        public String? Subject { get; set; }    
        [JsonIgnore]
        public PClass? PClass { get; set; }
        public ICollection<ApplicationUserYear> ApplicationUserYears { get; set; }
        public ICollection<ApplicationUserTerm> ApplicationUserTerms { get; set; }  
        public ICollection<AppLicationUserPClass> AppLicationUserPClasses { get; set; } 
    }
}