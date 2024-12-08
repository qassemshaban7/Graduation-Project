using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Attendence> Attendences { get; set; }
        public DbSet<Behavior> Behaviors { get; set; }
        public DbSet<Apperance> Apperances { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<StudentGrade> StudentGrades { get; set; }
        public DbSet<ApplicationUserTerm> ApplicationUserTerms { get; set; }
        //public DbSet<ApplicationUserMaterial> ApplicationUserMaterial { get; set; }
        public DbSet<ApplicationUserYear> ApplicationUserYear { get; set; }
        public DbSet<AppLicationUserPClass> AppLicationUserPClasses { get; set; }
        public DbSet<PClass> PClasses { get; set; }
        public DbSet<Year> Years { get; set; }
        public DbSet<DefaultMaterial> DefaultMateriales { get; set; }   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUserTerm>()
                .HasKey(ut => new { ut.UserId, ut.TermId });

            //modelBuilder.Entity<ApplicationUserMaterial>()
            //    .HasKey(ut => new { ut.UserId, ut.MaterialId });

            modelBuilder.Entity<AppLicationUserPClass>()
                .HasKey(ut => new { ut.UserId, ut.PClassId });

            modelBuilder.Entity<ApplicationUserYear>()
                .HasKey(ut => new { ut.UserId, ut.YearId });


            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole()
                {
                    Id = "jg57hyr5-0765-464d-a42b-185922feb101", //Guid.NewGuid().ToString(),
                    Name = "Student",
                    NormalizedName = "STUDENT",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    ArabicRoleName = "طالب"
                },
                new ApplicationRole()
                {
                    Id = "kh51dg85-0765-464d-a42b-185922f14522", //Guid.NewGuid().ToString(),
                    Name = "Teacher",
                    NormalizedName = "TEACHER",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    ArabicRoleName = "معلم"
                },
                new ApplicationRole()
                {
                    Id = "lsg55434-0765-464d-a42b-18524ffeb108", //Guid.NewGuid().ToString(),
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    ArabicRoleName = "المدير العام"
                });

            var appUser3 = new ApplicationUser
            {
                Id = "zs08h2c5-0765-464d-a42b-185922jfdjk5",
                UserName = "manager",
                NormalizedUserName = "MANAGER",
                Email = "manager@gmail.com",
                NormalizedEmail = "MANAGER@GMAIL.COM",
                EmailConfirmed = true,
                Image ="manangerphoto",
                Name = "MANAGER",
                NationalNum = "12345678901234"
            };


            PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();
            appUser3.PasswordHash = ph.HashPassword(appUser3, "P@ssw0rd");

            //seed user
            modelBuilder.Entity<ApplicationUser>().HasData(appUser3);


            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "lsg55434-0765-464d-a42b-18524ffeb108",
                    UserId = "zs08h2c5-0765-464d-a42b-185922jfdjk5"
                }
             );

            modelBuilder.Entity<Term>().HasData(new Term
            {
                Id = 1,
                Index = 900000000,
                TermName = "Graduated",
                EndDate = DateTime.Now,
                YearId = 1
            });

            modelBuilder.Entity<Year>().HasData(new Year
            {
                Id = 1,
                Index = 900000000,
                Name = "Graduated",
            });
            modelBuilder.Entity<PClass>().HasData(new PClass
            {
                Id = 500,
                Name = "Graduated_Students",
            });

            //modelBuilder.Entity<ApplicationUser>()
            //    .Property(s => s.Password)
            //    .HasDefaultValue("P@ssw0rd");
            //modelBuilder.Entity<ApplicationUser>()
            //    .Property(s => s.ConfirmPassword)
            //    .HasDefaultValue("P@ssw0rd");

            base.OnModelCreating(modelBuilder);
        }
    }
}