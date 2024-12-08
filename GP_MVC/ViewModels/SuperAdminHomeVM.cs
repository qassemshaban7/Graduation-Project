using GP_MVC.Models;

namespace GP_MVC.ViewModels
{
    public class SuperAdminHomeVM
    {
        public  IEnumerable<ApplicationUser>? Teachers { get; set; }
        public  IEnumerable<ApplicationUser>? Students { get; set; }
        public IEnumerable<Year>? Years { get; set; }
        public IEnumerable<PClass>? Classes { get; set; }
        public IEnumerable<Survey>? Surveys { get; set; }
        public IEnumerable<Behavior>? Behaviors { get; set; }
    }
}
