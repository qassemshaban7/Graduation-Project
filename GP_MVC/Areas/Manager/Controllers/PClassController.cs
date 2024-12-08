using ClosedXML.Excel;
using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GP_MVC.Areas.Manager.Controllers
{
    [Authorize(Roles = StaticDetails.Manager)]
    [Area(nameof(Manager))]
    [Route(nameof(Manager) + "/[controller]/[action]")]
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

            var Course = await _context.PClasses.OrderBy(x => x.Name).ToListAsync();

            return View(Course);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( PClass year)
        {
            try
            {
                if (_context.PClasses.Any(c => c.Name == year.Name))
                {
                    TempData["ErrorMessage"] = $"Class with name {year.Name} already exists";
                    return RedirectToAction("Create");
                }

                _context.Add(year);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("created", "true");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the Class.";
            }

            return View(year);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var year = await _context.PClasses.FindAsync(id);
            if (year == null)
            {
                return NotFound();
            }

            return View(year);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PClass year)
        {
            if (id != year.Id)
            {
                return NotFound();
            }

            try
            {
                try
                {

                    if (_context.PClasses.Any(c => c.Name == year.Name && c.Id != year.Id))
                    {
                        TempData["ErrorMessage"] = $"Class with name {year.Name} already exists";
                        return RedirectToAction("Edit", new { id = year.Id });
                    }

                    _context.Update(year);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!YearExists(year.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the Class.";
                    return RedirectToAction("Edit", new { id = year.Id });
                }
            }
            catch
            {
                return View(year);
            }
        }

        private bool YearExists(int id)
        {
            return _context.PClasses.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var ToDelete = await _context.PClasses.FindAsync(id);
            if (ToDelete == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Class with ID '{id}' not found."
                });
            }

            _context.PClasses.Remove(ToDelete);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("deleted", "true");
            return RedirectToAction(nameof(Index));
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
        public IActionResult GetStudentGradesDetails(string studentId, int materialId)
        {
            var examGradesDetails = _context.Exams
                .Join(
                    _context.StudentGrades,
                    exam => exam.Id,
                    grade => grade.Exam.Id,
                    (exam, grade) => new { Exam = exam, Grade = grade }
                )
                .Where(j => j.Grade.ApplicationUser.Id == studentId && j.Exam.MaterialId == materialId)
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
