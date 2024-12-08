using Graduation_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Services
{
    public class GradeCalculationService
    {
        private readonly ApplicationDbContext _context;

        public GradeCalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public (double totalMaterialGrade, bool passedHalfGrade) CalculateTotalMaterialGrade(Material material, ApplicationUser student)
        {
            double totalMaterialGrade = _context.Exams
                .Join(
                    _context.StudentGrades,
                    exam => exam.Id,
                    grade => grade.Exam.Id,
                    (exam, grade) => new { Exam = exam, Grade = grade }
                )
                .Where(j => j.Exam.Material.Id == material.Id && j.Grade.ApplicationUser.Id == student.Id)
                .Sum(j => j.Grade.Student_Grade);

            double halfMaterialGrade = material.M_grade / 2;
            bool passedHalfGrade = totalMaterialGrade >= halfMaterialGrade;

            return (totalMaterialGrade, passedHalfGrade);
        }

        public async Task<bool> PassedAllMaterialsInTermAsync(ApplicationUser student, int termId)
        {
            var term = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Id == termId);

            if (term == null)
            {
                return false;
            }

            foreach (var material in term.Materials)
            {
                var (totalMaterialGrade, passedHalfGrade) = CalculateTotalMaterialGrade(material, student);

                if (!passedHalfGrade)
                {
                    return false;
                }
            }
            return true;
        }

    }
}