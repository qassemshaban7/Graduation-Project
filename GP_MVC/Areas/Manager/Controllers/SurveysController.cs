using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GP_MVC.Areas.Manager.Controllers
{
    [Authorize(Roles = StaticDetails.Manager)]
    [Area(nameof(Manager))]
    [Route(nameof(Manager) + "/[controller]/[action]")]
    public class SurveysController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public SurveysController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment
            , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
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

            var surveys = await _context.Surveys.ToListAsync();

            return View(surveys);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Survey model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            try
            {
                var survey = new Survey
                {
                    SurveyName = model.SurveyName,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                };

                _context.Surveys.Add(survey);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("created", "true");
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == id);
            if (survey == null)
            {
                return NotFound();
            }
            return View(survey);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Survey model)
        {
            try
            {
                var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == model.Id);

                if (survey == null)
                {
                    return NotFound();
                }

                survey.SurveyName = model.SurveyName;
                survey.StartDate = model.StartDate;
                survey.EndDate = model.EndDate;

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("updated", "true");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (survey == null)
            {
                return NotFound();
            }

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("deleted", "true");
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> GetCourseQuestionAnswers(int id)
        {
            var questionsWithAnswers = await _context.Results
                .Where(r => r.SurveyId == id)
                .Include(r => r.Question)
                .GroupBy(r => new { r.QuestionId, r.Question.QuestionText })
                .Select(g => new QuestionWithAnswers
                {
                    QuestionId = g.Key.QuestionId,
                    QuestionText = g.Key.QuestionText,
                    Answers = g.GroupBy(r => r.Answer)
                               .Select(ag => new Answer
                               {
                                   AnswerText = ag.Key,
                                   Count = ag.Count()
                               }).ToList()
                })
                .ToListAsync();

            return View("GetCourseQuestionAnswers", questionsWithAnswers);
        }

    }
}
