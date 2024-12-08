using ClosedXML.Excel;
using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace GP_MVC.Areas.Teacher.Controllers
{
    [Authorize(Roles = StaticDetails.Teacher)]
    [Area(nameof(Teacher))]
    [Route(nameof(Teacher) + "/[controller]/[action]")]
    public class PClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PClassController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {

            if (HttpContext.Session.GetString("created") != null)
            {
                ViewBag.created = true;
                HttpContext.Session.Remove("created");
            }
            if (HttpContext.Session.GetString("updated") != null)
            {
                ViewBag.updated = true;
                HttpContext.Session.Remove("updated");
            }
            if (HttpContext.Session.GetString("deleted") != null)
            {
                ViewBag.deleted = true;
                HttpContext.Session.Remove("deleted");
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            var Course = await _context.PClasses
                                    .Include(p => p.AppLicationUserPClasses)
                                    .Where(p => p.AppLicationUserPClasses.Any(aup => aup.UserId == userId))
                                    .OrderBy(p => p.Name)
                                    .ToListAsync();

            return View(Course);
        }



        public async Task<IActionResult> Student(int Id)
        {

            if (HttpContext.Session.GetString("created") != null)
            {
                ViewBag.created = true;
                HttpContext.Session.Remove("created");
            }
            if (HttpContext.Session.GetString("updated") != null)
            {
                ViewBag.updated = true;
                HttpContext.Session.Remove("updated");
            }
            if (HttpContext.Session.GetString("deleted") != null)
            {
                ViewBag.deleted = true;
                HttpContext.Session.Remove("deleted");
            }


            var Students = await (from x in _context.Users
                                  join userRole in _context.UserRoles
                                  on x.Id equals userRole.UserId
                                  join role in _context.Roles
                                  on userRole.RoleId equals role.Id
                                  where role.Name == StaticDetails.Student
                                  select x)
                                 .OrderBy(h => h.Name)
                                 .Where(s => s.PClassId == Id)
                              .Include(x => x.PClass)
                              .Include(x => x.ApplicationUserYears)
                              .ThenInclude(y => y.Year)
                              .ToListAsync();

            return View(Students);
        }


        [HttpGet]
        public IActionResult GetStudentMaterialGrade(string id)
        {
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

            ViewBag.id = id;
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

        [HttpGet]
        public async Task<IActionResult> AddGrades(string id)
        {
            var studentYears = await _context.Years
                        .Include(p => p.ApplicationUserYears)
                        .Where(p => p.ApplicationUserYears.Any(aup => aup.UserId == id))
                        .OrderBy(p => p.Name)
                        .ToListAsync();

            var user = await _context.Users.FindAsync(id);

            if (!studentYears.Any())
            {
                TempData["ErrorMessage"] = "No years found for this student.";
                return RedirectToAction("Student", new { studentId = user.Id });
            }

            var studentTerms = await _context.Terms
                        .Include(p => p.ApplicationUserTerms)
                        .Where(p => p.ApplicationUserTerms.Any(aup => aup.UserId == id))
                        .OrderBy(p => p.TermName)
                        .ToListAsync();

            var maxYearId = studentYears.Max(uy => uy.Index);
            var maxTermId = studentTerms.Max(uy => uy.Index);

            var maxYear = await _context.Years
                .Where(t => t.Index == maxYearId)
                .FirstOrDefaultAsync();

            var maxTerm = await _context.Terms
                .Where(t => t.Index == maxTermId)
                .FirstOrDefaultAsync();

            string teacherId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var teacher = await _context.Users.FindAsync(teacherId);

            var material = await _context.Materials
                .Where(m => m.TermId == maxTerm.Id)
                .Where(m => m.Name == teacher.Subject)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name
                }).FirstOrDefaultAsync();

            if (material == null)
            {
                TempData["ErrorMessage"] = "No material found for the teacher's subject.";
                return RedirectToAction("Student", new { id = user.Id });
            }

            var model = new AddStudentGradeVM
            {
                YearId = maxYear.Id,
                TermId = maxTerm.Id,
                StudentId = id,
                MaterialId = int.Parse(material.Value),
                Material = material
            };

            ViewBag.exams = await _context.Exams
                .Where(e => e.MaterialId == model.MaterialId)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Name
                }).ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentGradeForExam(string studentId, int examId)
        {
            var studentGrade = await _context.StudentGrades
                .Where(sg => sg.ApplicationUser.Id == studentId && sg.Exam.Id == examId)
                .Select(sg => new
                {
                    sg.Student_Grade,
                    ExamGrade = sg.Exam.Exam_Grade
                })
                .FirstOrDefaultAsync();

            if (studentGrade == null)
            {
                return Json(new { success = false, message = "No grade found for the selected exam." });
            }

            return Json(new { success = true, studentGrade = studentGrade.Student_Grade, examGrade = studentGrade.ExamGrade });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGrades(string id, AddStudentGradeVM model)
        {
            var existingStudentGrade = await _context.StudentGrades
                .Include(x => x.ApplicationUser)
                .Include(x => x.Exam)
                .Where(x => x.Exam.Id == model.ExamId && x.ApplicationUser.Id == model.StudentId)
                .FirstOrDefaultAsync();

            string teacherId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (existingStudentGrade == null)
            {
                TempData["ErrorMessage"] = $"Student grade with ID {model.studentGradeId} not found.";
                return RedirectToAction("AddGrades", new { id = model.StudentId });
            }

            var exam = await _context.Exams.FindAsync(model.ExamId);
            if (exam == null)
            {
                TempData["ErrorMessage"] = $"Invalid exam ID: {model.ExamId}.";
                return RedirectToAction("AddGrades", new { id = model.StudentId });
            }

            var user = await _context.Users.FindAsync(model.StudentId);
            if (user == null)
            {
                TempData["ErrorMessage"] = $"Invalid Student ID: {model.StudentId}.";
                return RedirectToAction("AddGrades", new { id = model.StudentId });
            }

            var materialId = await _context.Exams
                .Where(e => e.Id == model.ExamId)
                .Select(e => e.Material.Id)
                .FirstOrDefaultAsync();

            var teacher = await _context.Users.FindAsync(teacherId);
            var material = await _context.Materials.FindAsync(materialId);

            if (teacher.Subject != material.Name)
            {
                TempData["ErrorMessage"] = "You are not authorized to add grades for this material.";
                return RedirectToAction("AddGrades", new { id = model.StudentId });
            }

            var pClassId = await _context.Users
               .Where(i => i.Id == model.StudentId)
               .Select(e => e.PClass.Id)
               .FirstOrDefaultAsync();

            var isTeacherForClass = await _context.AppLicationUserPClasses
                .AnyAsync(um => um.UserId == teacherId && um.PClassId == pClassId);

            if (!isTeacherForClass)
            {
                TempData["ErrorMessage"] = "You are not authorized to add grades for this student.";
                return RedirectToAction("AddGrades", new { id = model.StudentId });
            }

            if (model.Student_Grade < 0)
            {
                TempData["ErrorMessage"] = "Student grade must be positive.";
                return RedirectToAction("AddGrades", new { id = model.StudentId });
            }

            if (model.Student_Grade > exam.Exam_Grade)
            {
                TempData["ErrorMessage"] = "Student grade must be not bigger than exam grade.";
                return RedirectToAction("AddGrades", new { id = model.StudentId });
            }

            existingStudentGrade.Student_Grade = model.Student_Grade;
            existingStudentGrade.Exam = exam;
            existingStudentGrade.ApplicationUser = user;


            var stu = await _context.Users.FindAsync(model.StudentId);

            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("updated", "true");
            return RedirectToAction("Student", new { id = stu .PClassId});
        }



        [HttpGet]
        public async Task<IActionResult> Attendance(string id)
        {
            ViewBag.Stu = await _context.Users.FindAsync(id);

            var attendances = await _context.Attendences
                .Include(c => c.ApplicationUser)
                .Where(s => s.ApplicationUser.Id == id)
                .OrderBy(x => x.Date_Day)
                .ToListAsync();

            return View(attendances);
        }


        [HttpGet]
        public async Task<IActionResult> Charts(string id)
        {
            ViewBag.Stu = await _context.Users.FindAsync(id);

            var attendances = await _context.Attendences
                .Include(c => c.ApplicationUser)
                .Where(s => s.ApplicationUser.Id == id)
                .OrderBy(x => x.Date_Day)
                .ToListAsync();

            return View(attendances);
        }

        [HttpGet]
        public ActionResult ExcelForOneStudent(string id)
        {
            var user = _context.Users.Find(id);

            var _empdata = ForOneStudent(id);
            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet1 = wb.AddWorksheet(_empdata, $"{user.Name} Attendance");

                sheet1.Column(1).Style.Font.FontColor = XLColor.Black;

                sheet1.Columns(2, 4).Style.Font.FontColor = XLColor.Blue;

                sheet1.Row(1).CellsUsed().Style.Fill.BackgroundColor = XLColor.Black;
                sheet1.Row(1).Style.Font.FontColor = XLColor.White;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{user.Name} Attendance.xlsx");
                }
            }
        }


        [NonAction]
        private DataTable ForOneStudent(string id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                throw new Exception($"Student With ID {id} Not Found.");
            }

            DataTable dt = new DataTable();
            dt.TableName = "Attendences";
            dt.Columns.Add("Student Name", typeof(string));
            dt.Columns.Add("date Of Day", typeof(DateOnly));
            dt.Columns.Add("Part One", typeof(string));
            dt.Columns.Add("Part Two", typeof(string));
            dt.Columns.Add("Total", typeof(int));

            var _list = _context.Attendences.Where(x => x.ApplicationUser == user).OrderBy(a => a.Date_Day).ToList();
            if (_list.Count > 0)
            {
                _list.ForEach(item =>
                {
                    dt.Rows.Add(item.ApplicationUser.Name, item.Date_Day, item.PartOne, item.PartTwo, item.Total);
                });
            }

            return dt;
        }



        [HttpGet]
        public async Task<IActionResult> AttendanceForClass(int id, DateOnly? selectedDate)
        {
            var attendancesQuery = _context.Attendences
                .Include(a => a.ApplicationUser)
                .Where(a => a.ApplicationUser.PClassId == id)
                .OrderBy(a => a.Date_Day)
                .AsQueryable();

            var dates = await _context.Attendences
                .Select(a => a.Date_Day)
                .Distinct()
                .ToListAsync();

            if (selectedDate.HasValue)
            {
                attendancesQuery = attendancesQuery.Where(a => a.Date_Day == selectedDate.Value);
            }

            var attendances = await attendancesQuery.ToListAsync();

            ViewBag.Dates = dates;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.PClassId = id;

            return View(attendances);
        }


        [HttpGet]
        public ActionResult ExcelForOneClass(int ClassId, DateOnly? selectedDate)
        {
            if (!selectedDate.HasValue)
            {
                TempData["AlertMessage"] = "Please select a date of Day.";
                return RedirectToAction("AttendanceForClass", new { id = ClassId });
            }

            var pClass = _context.PClasses.Find(ClassId);
            var _empdata = ForOneClass(ClassId, selectedDate);
            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet1 = wb.AddWorksheet(_empdata, $"{pClass.Name} Attendance");

                sheet1.Columns(1, 2).Style.Font.FontColor = XLColor.Black;
                sheet1.Column(3).Style.Font.FontColor = XLColor.Blue;
                sheet1.Columns(4, 5).Style.Font.FontColor = XLColor.Blue;
                sheet1.Row(1).CellsUsed().Style.Fill.BackgroundColor = XLColor.Black;
                sheet1.Row(1).Style.Font.FontColor = XLColor.White;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{pClass.Name} Attendance.xlsx");
                }
            }
        }



        [NonAction]
        private DataTable ForOneClass(int ClassId1, DateOnly? selectedDate)
        {
            var pClass = _context.PClasses.Find(ClassId1);
            if (pClass == null)
            {
                throw new Exception($"Class With ID {ClassId1} Not Found.");
            }

            DataTable dt = new DataTable();
            dt.TableName = "Attendences";
            dt.Columns.Add("Student Name", typeof(string));
            dt.Columns.Add("date Of Day", typeof(DateOnly));
            dt.Columns.Add("Part One", typeof(string));
            dt.Columns.Add("Part Two", typeof(string));
            dt.Columns.Add("Total", typeof(int));

            var query = _context.Attendences.Include(x => x.ApplicationUser).OrderBy(a => a.ApplicationUser.Name).AsQueryable();

            if (selectedDate.HasValue)
            {
                query = query.Where(a => a.Date_Day == selectedDate.Value && a.ApplicationUser.PClassId == ClassId1);
            }
            else
            {
                query = query.Where(a => a.ApplicationUser.PClassId == ClassId1);
            }

            var _list = query.ToList();

            if (_list.Count > 0)
            {
                _list.ForEach(item =>
                {
                    dt.Rows.Add(item.ApplicationUser.Name, item.Date_Day, item.PartOne, item.PartTwo, item.Total);
                });
            }

            return dt;
        }

    }
}
