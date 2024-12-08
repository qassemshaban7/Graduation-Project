using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Graduation_Project.Models
{
    public class PClass
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [JsonIgnore]
        public ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}
