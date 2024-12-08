using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GP_MVC.Areas.Teacher.Controllers
{
    [Authorize(Roles = StaticDetails.Teacher)]
    [Area(nameof(Teacher))]
    [Route(nameof(Teacher) + "/[controller]/[action]")]
    public class YearController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public YearController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
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

            var Course = await _context.Years.Where(x => x.Index != 900000000).ToListAsync();

            return View(Course);
        }

        public async Task<IActionResult> TermIndex(int id)
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

            var Course = await _context.Terms.Include(y => y.Year).Where(x => x.YearId == id).ToListAsync();
            ViewBag.YearId = id;
            return View(Course);
        }

        public async Task<IActionResult> MaterialIndex(int id)
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

            var Course = await _context.Materials.Include(y => y.Term).Where(x => x.TermId == id && x.Name == teacher.Subject).ToListAsync();
            ViewBag.TermId = id;
            return View(Course);
        }

        public async Task<IActionResult> ExamIndex(int id)
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

            var Course = await _context.Exams.Include(y => y.Material).Where(x => x.MaterialId == id).ToListAsync();

            var users = await (from x in _context.Users
                               join userRole in _context.UserRoles
                               on x.Id equals userRole.UserId
                               join role in _context.Roles
                               on userRole.RoleId equals role.Id
                               where role.Name == StaticDetails.Teacher
                               select x)
                               .ToListAsync();

            ViewBag.Teachers = users;
            ViewBag.MaterialId = id;
            return View(Course);
        }

        [HttpGet]
        public async Task<IActionResult> AddExam(int id)
        {
            ViewBag.MaterialId = id;

            var users = await (from x in _context.Users
                               join userRole in _context.UserRoles
                               on x.Id equals userRole.UserId
                               join role in _context.Roles
                               on userRole.RoleId equals role.Id
                               where role.Name == StaticDetails.Teacher
                               select x)
                               .ToListAsync();

            ViewBag.Teachers = users;

            ViewBag.Materials = _context.Materials.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddExam(int id, ExamVM examDtos, string? returnUrl = null)
        {
            ViewBag.MaterialId = id;
            if (examDtos == null)
            {
                TempData["ErrorMessage"] = "Invalid exam data.";
                return RedirectToAction("AddExam");
            }

            var material = await _context.Materials.FindAsync(examDtos.MaterialId);

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (teacher == null || teacher.Subject != material.Name)
            {
                TempData["ErrorMessage"] = "You are not authorized to add grades for this material.";
                return RedirectToAction("AddExam");
            }

            if (_context.Exams.Any(e => e.Name == examDtos.Name && e.Material.Id == material.Id))
            {
                TempData["ErrorMessage"] = $"An exam with name '{examDtos.Name}' already exists for this material.";
                return RedirectToAction("AddExam");
            }

            var totalExamGradesInMaterial = _context.Exams.Where(e => e.Material.Id == material.Id).Sum(e => e.Exam_Grade);
            if (totalExamGradesInMaterial + examDtos.Exam_Grade > material.M_grade)
            {
                TempData["ErrorMessage"] = "Total exam grades exceed material grade.";
                return RedirectToAction("AddExam");
            }

            var exam = new Exam
            {
                Name = examDtos.Name,
                Exam_Grade = examDtos.Exam_Grade,
                Material = material
            };


             if (examDtos.Image != null)
            {
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                if (!allowedExtensions.Contains(Path.GetExtension(examDtos.Image.FileName).ToLower()))
                {
                    TempData["ErrorMessage"] = "Only .png and .jpg and .jpeg images are allowed!";
                    return RedirectToAction("AddExam");
                }

                // Send image to API
                using (var httpClient = new HttpClient())
                {
                    using (var form = new MultipartFormDataContent())
                    {
                        using (var fileStream = examDtos.Image.OpenReadStream())
                        {
                            form.Add(new StreamContent(fileStream), "Image", examDtos.Image.FileName);

                            var response = await httpClient.PostAsync("http://ablexav1.runasp.net/api/Exam/AddExamImage", form);

                            if (response.IsSuccessStatusCode)
                            {
                                var uniqueFileName = await response.Content.ReadAsStringAsync();
                                exam.Image = uniqueFileName;
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Failed to upload image.";
                                return RedirectToAction("AddExam");
                            }
                        }
                    }
                }
            }

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("created", "true");

            return RedirectToAction("ExamIndex", new { id = examDtos.MaterialId });
        }


        public async Task<IActionResult> EditExam(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            ViewBag.MaterialId = exam.MaterialId;

            var editExamVM = new EditExamVM
            {
                Id = exam.Id,
                Name = exam.Name,
                Exam_Grade = exam.Exam_Grade,
                MaterialId = exam.MaterialId,
            };

            return View(editExamVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExam(EditExamVM model)
        {
            try
            {
                var question = await _context.Exams.FirstOrDefaultAsync(s => s.Id == model.Id);
                ViewBag.MaterialId = question.MaterialId;

                if (question == null)
                {
                    return NotFound();
                }

                var material = await _context.Materials.FindAsync(model.MaterialId);

                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var teacher = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

                if (teacher == null || teacher.Subject != material.Name)
                {
                    TempData["ErrorMessage"] = "You are not authorized to add grades for this material.";
                    return RedirectToAction("ExamIndex", new { id = question.MaterialId });
                }

                if (_context.Exams.Any(e => e.Name == model.Name && e.Material.Id == material.Id && e.Id != model.Id))
                {
                    TempData["ErrorMessage"] = $"An exam with name '{model.Name}' already exists for this material.";
                    return RedirectToAction("EditExam", new { id = model.Id});
                }

                var totalExamGradesInMaterial = _context.Exams.Where(e => e.Material.Id == material.Id && e.Id != model.Id).Sum(e => e.Exam_Grade);
                if (totalExamGradesInMaterial + model.Exam_Grade > material.M_grade)
                {
                    TempData["ErrorMessage"] = "Total exam grades exceed material grade.";
                    return RedirectToAction("EditExam", new { id = model.Id });
                }

                question.Name = model.Name;
                question.Exam_Grade = model.Exam_Grade;


                string oldImageFileName = question.Image;
                string Oldd = question.Image;

                if (model.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    if (!allowedExtensions.Contains(Path.GetExtension(model.Image.FileName).ToLower()))
                    {
                        TempData["ErrorMessage"] = "Only .png and .jpg and .jpeg images are allowed!";
                        return RedirectToAction("EditExam", new { id = model.Id });
                    }

                    // Send image to API
                    using (var httpClient = new HttpClient())
                    {
                        using (var form = new MultipartFormDataContent())
                        {
                            using (var fileStream = model.Image.OpenReadStream())
                            {
                                form.Add(new StreamContent(fileStream), "Image", model.Image.FileName);

                                var response = await httpClient.PostAsync("http://ablexav1.runasp.net/api/Exam/AddExamImage", form);

                                if (response.IsSuccessStatusCode)
                                {
                                    var uniqueFileName = await response.Content.ReadAsStringAsync();
                                    question.Image = uniqueFileName;
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Failed to upload image.";
                                    return RedirectToAction("EditExam", new { id = model.Id });
                                }
                            }
                        }
                    }
                }
                else
                {
                    question.Image = Oldd;
                }


                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("updated", "true");
                return RedirectToAction("ExamIndex", new { id = question.MaterialId });
            }
            catch (Exception)
            {
                var question = await _context.Exams.FirstOrDefaultAsync(s => s.Id == model.Id);
                ViewBag.MaterialId = question.MaterialId;
                return RedirectToAction("ExamIndex", new { id = question.MaterialId });
            }
        }


        public async Task<IActionResult> DeleteExam(int id)
        {
            var ToDelete = await _context.Exams.FindAsync(id);
            if (ToDelete == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Exam with ID '{id}' not found."
                });
            }

            _context.Exams.Remove(ToDelete);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("deleted", "true");
            return RedirectToAction("ExamIndex", new { id = ToDelete.MaterialId });
        }

    }
}
