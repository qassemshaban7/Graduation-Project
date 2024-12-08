namespace GP_MVC.Models
{
    public class Year
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public ICollection<Term> Terms { get; set; }
        public ICollection<ApplicationUserYear> ApplicationUserYears { get; set; }
    }
}
