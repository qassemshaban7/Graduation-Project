using Microsoft.AspNetCore.Identity;

namespace GP_MVC.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string ArabicRoleName { get; set; }
    }
}
