using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GP_MVC.Areas.Student.Controllers
{
    [Authorize(Roles = StaticDetails.Student)]
    [Area(nameof(Student))]
    [Route(nameof(Student) + "/[controller]/[action]")]
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GradesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult GetStudentMaterialGrade(string id)
        {
            ViewBag.studentId = id;

            var years = _context.Years.Include(p => p.ApplicationUserYears)
                        .Where(p => p.ApplicationUserYears.Any(aup => aup.UserId == id))
                        .Where(x => x.Index != 900000000)
                        .ToList();

            var yearGrades = years.Select(year => new YearGradeViewModel
            {
                Year = year.Name,
                TermGrades = _context.Terms
                    .Where(term => term.YearId == year.Id)
                    .Select(term => new TermGradeViewModel
                    {
                        TermId = term.Id,
                        TermName = term.TermName,
                        MaterialGrades = _context.Exams
                            .Join(
                                _context.StudentGrades,
                                exam => exam.Id,
                                grade => grade.Exam.Id,
                                (exam, grade) => new { Exam = exam, Grade = grade }
                            )
                            .Where(j => j.Grade.ApplicationUser.Id == id && j.Exam.Material.TermId == term.Id)
                            .GroupBy(j => j.Exam.Material.Id)
                            .Select(group => new MaterialGradeViewModel
                            {
                                MaterialId = group.Key,
                                MaterialName = group.First().Exam.Material.Name,
                                MaterialGrade = group.First().Exam.Material.M_grade,
                                StudentTotalGrade = group.Sum(g => g.Grade.Student_Grade)
                            })
                            .ToList()
                    }).ToList()
            }).ToList();

            return View(yearGrades);
        }

        [HttpGet]
        public IActionResult GetStudentGradesDetails(string studentId, int materialId)
        {
            var examGradesDetails = _context.Exams
                .Join(
                    _context.StudentGrades,
                    exam => exam.Id,
                    grade => grade.Exam.Id,
                    (exam, grade) => new { Exam = exam, Grade = grade }
                )
                .Where(j => j.Grade.ApplicationUser.Id == studentId && j.Exam.Material.Id == materialId)
                .GroupBy(j => new { j.Exam.Material.Name, j.Exam.Material.M_grade })
                .Select(group => new StudentGradesDetailsViewModel
                {
                    MaterialName = group.Key.Name,
                    MaterialGrade = group.Key.M_grade,
                    StudentTotalGrade = group.Sum(g => g.Grade.Student_Grade),
                    Exams = group.Select(j => new ExamViewModel
                    {
                        ExamId = j.Exam.Id,
                        ExamName = j.Exam.Name,
                        ExamGrade = j.Exam.Exam_Grade,
                        GeneralExamImage = $"http://ablexav1.runasp.net/GeneralExams/{j.Exam.Image}",
                        StudentExamGrade = j.Grade.Student_Grade
                    }).ToList()
                })
                .ToList();

            return View(examGradesDetails);
        }


        [HttpGet]
        public IActionResult Graph(string id)
        {
            var years = _context.Years.Include(p => p.ApplicationUserYears)
                        .Where(p => p.ApplicationUserYears.Any(aup => aup.UserId == id))
                        .Where(x => x.Index != 900000000)
                        .ToList();

            ViewBag.id = id;

            var yearGrades = years.Select(year => new YearGradeViewModel
            {
                Year = year.Name,
                TermGrades = _context.Terms
                    .Where(term => term.YearId == year.Id)
                    .Select(term => new TermGradeViewModel
                    {
                        TermId = term.Id,
                        TermName = term.TermName,
                        MaterialGrades = _context.Exams
                            .Join(
                                _context.StudentGrades,
                                exam => exam.Id,
                                grade => grade.Exam.Id,
                                (exam, grade) => new { Exam = exam, Grade = grade }
                            )
                            .Where(j => j.Grade.ApplicationUser.Id == id && j.Exam.Material.TermId == term.Id)
                            .GroupBy(j => j.Exam.Material.Id)
                            .Select(group => new MaterialGradeViewModel
                            {
                                MaterialId = group.Key,
                                MaterialName = group.First().Exam.Material.Name,
                                MaterialGrade = group.First().Exam.Material.M_grade,
                                StudentTotalGrade = group.Sum(g => g.Grade.Student_Grade)
                            })
                            .ToList()
                    }).ToList()
            }).ToList();

            return View(yearGrades);
        }

    }
}
