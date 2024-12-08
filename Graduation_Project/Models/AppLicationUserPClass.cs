using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Models
{
    public class AppLicationUserPClass
    {
        [Key]
        public string UserId { get; set; }
        [Key]
        public int PClassId { get; set; }   

        public ApplicationUser User { get; set; }
        public PClass PClass { get; set; }
    }
}
