using System.ComponentModel.DataAnnotations;

namespace GP_MVC.Models
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
