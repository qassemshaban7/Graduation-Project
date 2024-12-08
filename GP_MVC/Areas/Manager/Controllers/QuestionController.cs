using GP_MVC.Models;
using GP_MVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GP_MVC.Areas.Manager.Controllers
{
    [Authorize(Roles = StaticDetails.Manager)]
    [Area(nameof(Manager))]
    [Route(nameof(Manager) + "/[controller]/[action]")]
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public QuestionController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment
            , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index(int? id)
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

            var questions = await _context.Questions.Where(x => x.SurveyId == id).ToListAsync();
            ViewBag.SurveyId = id;
            return View(questions);
        }
        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            var survey = await _context.Surveys.FirstOrDefaultAsync(x => x.Id == id);
            if (survey == null)
                return NotFound();
            ViewBag.surveyId = id;
            ViewBag.surveyName = survey.SurveyName;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Question model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            try
            {
                var question = new Question
                {
                    QuestionText = model.QuestionText,
                    FirstAnswer = model.FirstAnswer,
                    SecondAnswer = model.SecondAnswer,
                    ThirdAnswer = model.ThirdAnswer,
                    FourthAnswer = model.FourthAnswer,
                    SurveyId = model.SurveyId,

                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("created", "true");
                return RedirectToAction("Index", new { id = question.SurveyId });
            }
            catch
            {
                return RedirectToAction(nameof(Index), "Surveys");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _context.Questions.Include(x => x.Survey).FirstOrDefaultAsync(s => s.Id == id);
            if (question == null)
            {
                return NotFound();
            }
            return View(question);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Question model)
        {
            try
            {
                var question = await _context.Questions.FirstOrDefaultAsync(s => s.Id == model.Id);

                if (question == null)
                {
                    return NotFound();
                }

                question.QuestionText = model.QuestionText;
                question.FirstAnswer = model.FirstAnswer;
                question.SecondAnswer = model.SecondAnswer;
                question.ThirdAnswer = model.ThirdAnswer;
                question.FourthAnswer = model.FourthAnswer;

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("updated", "true");
                return RedirectToAction("Index", new { id = question.SurveyId });
            }
            catch
            {
                return RedirectToAction(nameof(Index), "Surveys");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(m => m.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("deleted", "true");
            return RedirectToAction("Index", new { id = question.SurveyId });
        }
    }
}
