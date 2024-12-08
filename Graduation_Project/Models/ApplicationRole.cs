using Microsoft.AspNetCore.Identity;

namespace Graduation_Project.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string ArabicRoleName { get; set; }
    }
}
