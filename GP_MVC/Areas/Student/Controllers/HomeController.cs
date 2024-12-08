using GP_MVC.Models;
using GP_MVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GP_MVC.ViewModels;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace GP_MVC.Areas.Student.Controllers
{
    [Authorize(Roles = StaticDetails.Student)]
    [Area(nameof(Student))]
    [Route(nameof(Student) + "/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            string teacherId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var stu = await _context.Users.FindAsync(teacherId);
            ViewBag.Attendance = await _context.Attendences.Where(x => x.ApplicationUser.Id == teacherId).CountAsync();
            ViewBag.studentId = stu.Id;
            ViewBag.Surveys = await _context.Surveys.CountAsync();


            var termsCount = await _context.ApplicationUserTerms
                .Where(a => a.UserId == teacherId)
                .Select(a => a.Term)
                .Where(t => t.Index != 900000000)
                .CountAsync();

            ViewBag.TermCount = termsCount;


            return View();
        }

        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(string oldPassword, string newPassword)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            int x = 0;
            if (oldPassword == null && newPassword == null)
            {
                {
                    x = 2;
                    return View("ChangePassword", new ChangePasswordViewModel { X = x });
                }
            }

            var passwordVerificationResult = await _userManager.CheckPasswordAsync(user, oldPassword);
            if (!passwordVerificationResult)
            {
                x = 1;
                return View("ChangePassword", new ChangePasswordViewModel { X = x });
            }

            // P@ssw0rd
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            else
            {
                return View("ChangePassword", new ChangePasswordViewModel());
            }
        }


        public async Task<IActionResult> Service()
        {
            if (HttpContext.Session.GetString("created") != null)
            {
                ViewBag.created = true;
                HttpContext.Session.Remove("created");
            }

            var survey = await _context.Surveys.Include(y => y.Questions).ToListAsync();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewBag.userId = userId;

            var userCompletedSurveys = await _context.Results
                .Where(rs => rs.UserId == userId)
                .Select(rs => rs.SurveyId)
                .ToListAsync();

            ViewBag.UserCompletedSurveys = userCompletedSurveys;


            return View(survey);
        }


        [HttpGet]
        public async Task<IActionResult> TakeCourseSurveyServices(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var survey = await _context.Surveys.FindAsync(Id);
            if (survey == null)
            {
                return RedirectToAction(nameof(Service));
            }

            var surveyQuestions = await _context.Questions.Where(x => x.SurveyId == survey.Id).ToListAsync();
            ViewBag.SurveyId = Id;
            return View(surveyQuestions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeCourseSurveyServices(List<SurveyResultServiesVM> surveyResults, int surveyId)
        {
            try
            {
                var survey = await _context.Surveys.FindAsync(surveyId);
                if (survey == null)
                    return NotFound();

                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound();

                foreach (var surveyResult in surveyResults)
                {
                    var result = new Result
                    {
                        QuestionId = surveyResult.QuestionId,
                        Answer = surveyResult.SelectedAnswer,
                        UserId = userId,
                        ApplicationUser = user,
                        SurveyId = surveyId,
                        Survey = survey
                    };

                    _context.Results.Add(result);
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("created", "true");
                return RedirectToAction("Service");
            }
            catch
            {

                var surveyQuestions = await _context.Questions.Where(x => x.SurveyId == surveyId).ToListAsync();
                ViewBag.SurveyId = surveyId;
                return View(surveyQuestions);
            }
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
        public async Task<IActionResult>  Charts(string id)
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
        public ActionResult ExcelForOneStudent()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            
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

    }
}
