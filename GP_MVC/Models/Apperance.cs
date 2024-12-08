namespace GP_MVC.Models
{
    public class Apperance
    {
        public int Id { get; set; }
        public string Apperance_Value { get; set; }
        public DateTime Time { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
