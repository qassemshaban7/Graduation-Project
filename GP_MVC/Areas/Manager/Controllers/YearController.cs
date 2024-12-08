using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GP_MVC.Areas.Manager.Controllers
{
    [Authorize(Roles = StaticDetails.Manager)]
    [Area(nameof(Manager))]
    [Route(nameof(Manager) + "/[controller]/[action]")]
    public class YearController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public YearController( ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
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

            var Course = await _context.Years.ToListAsync();

            return View(Course);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Years"] = new SelectList(await _context.Years.ToListAsync(), "Id", "Name", "Index");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Index")] Year year)
        {
            try
            {
                if (_context.Years.Any(t => t.Index == year.Index))
                {
                    TempData["ErrorMessage"] = "This Index value already exists.";
                    return RedirectToAction("Create");
                }

                if (_context.Years.Any(t => t.Name == year.Name))
                {
                    TempData["ErrorMessage"] = "This YearName already exists.";
                    return RedirectToAction("Create");
                }

                _context.Add(year);
                await _context.SaveChangesAsync();
                var thisYear = DateTime.Now.Year;
                var termOne = new Term
                {
                    TermName = $"First_Semester",
                    EndDate = new DateTime(thisYear, 2, 10),
                    Index = (year.Index * 2) - 1,
                    YearId = year.Id
                };

                var termTwo = new Term
                {
                    TermName = $"Second_Semester",
                    EndDate = new DateTime(thisYear, 8, 10),
                    Index = year.Index * 2,
                    YearId = year.Id
                };
                _context.Terms.Add(termOne);
                _context.Terms.Add(termTwo);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("created", "true");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the year.";
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

            var year = await _context.Years.FindAsync(id);
            if (year == null)
            {
                return NotFound();
            }

            return View(year);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Index")] Year year)
        {
            if (id != year.Id)
            {
                return NotFound();
            }

            try
            {
                try
                {
                    if (_context.Years.Any(t => t.Index == year.Index && t.Id != year.Id))
                    {
                        TempData["ErrorMessage"] = "This Index value already exists.";
                        return RedirectToAction("Edit", new { id = year.Id });
                    }

                    if (_context.Years.Any(t => t.Name == year.Name && t.Id != year.Id))
                    {
                        TempData["ErrorMessage"] = "This Year Name already exists.";
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
                    TempData["ErrorMessage"] = "An error occurred while updating the year.";
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
            return _context.Years.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var ToDelete = await _context.Years.FindAsync(id);
            if (ToDelete == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Year with ID '{id}' not found."
                });
            }

            _context.Years.Remove(ToDelete);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("deleted", "true");
            return RedirectToAction(nameof(Index));
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

        [HttpGet]
        public async Task<IActionResult> EditTerm(int? id)  
        {
            if (id == null)
            {
                return NotFound();
            }
           
            var term = await _context.Terms.FindAsync(id);
            ViewBag.YearId = term.YearId;
            if (term == null)
            {
                return NotFound();
            }

            ViewData["Years"] = new SelectList(_context.Years, "Id", "Name", term.YearId);
            return View(term);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTerm( Term model)
        {
            try
            {
                var question = await _context.Terms.FirstOrDefaultAsync(s => s.Id == model.Id);
                ViewBag.YearId = question.YearId;

                if (question == null)
                {
                    return NotFound();
                }

                if (_context.Terms.Any(t => t.Index == model.Index && t.Id != model.Id))
                {
                    TempData["ErrorMessage"] = "This Index value already exists.";
                    return RedirectToAction("EditTerm", new { id = model.Id });
                }

                if (_context.Terms.Any(t => t.TermName == model.TermName && t.YearId == model.YearId && t.Id != model.Id))
                {
                    TempData["ErrorMessage"] = "This Term Name already exists within the same Year.";
                    return RedirectToAction("EditTerm", new { id = model.Id });
                }

                question.TermName = model.TermName;
                question.EndDate = model.EndDate;
                question.Index = model.Index;

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("updated", "true");
                return RedirectToAction("TermIndex", new { id = question.YearId });
            }
            catch (Exception)
            {
                var question = await _context.Terms.FirstOrDefaultAsync(s => s.Id == model.Id);
                ViewBag.YearId = question.YearId;
                return RedirectToAction(nameof(TermIndex), "Year");
            }
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

            var Course = await _context.Materials.Include(y => y.Term).Where(x => x.TermId == id).ToListAsync();
            ViewBag.TermId = id;
            return View(Course);
        }

        [HttpGet]
        public IActionResult AddMaterial(int? id)
        {
            ViewBag.TermId = id;
            ViewData["Terms"] = new SelectList(_context.Terms, "Id", "TermName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial( int id,Material materialDto, string? returnUrl = null)
        {
            ViewBag.TermId = id;
            if (materialDto == null)
            {
                TempData["ErrorMessage"] = "Invalid material data.";
                return RedirectToAction("MaterialIndex");
            }

            var term = await _context.Terms.FindAsync(materialDto.TermId);
            if (term == null)
            {
                TempData["ErrorMessage"] = $"Term with ID '{materialDto.TermId}' not found.";
                return RedirectToAction("AddMaterial");
            }

            if (_context.Materials.Any(t => t.Name == materialDto.Name && t.TermId == id))
            {
                TempData["ErrorMessage"] = "This Material Name already exists In This Semester.";
                return RedirectToAction("AddMaterial");
            }

            if (materialDto.M_grade <= 0)
            {
                TempData["ErrorMessage"] = "Material Grade must be a positive number.";
                return RedirectToAction("AddMaterial");
            }

            var newMaterial = new Material
            {
                Name = materialDto.Name,
                M_grade = materialDto.M_grade,
                TermId = materialDto.TermId
            };

            _context.Materials.Add(newMaterial);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("created", "true");

            TempData["SuccessMessage"] = "Material added successfully.";
            return RedirectToAction("MaterialIndex", new { id = materialDto.TermId });
        }

        [HttpGet]
        public async Task<IActionResult> EditMaterial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var term = await _context.Materials.FindAsync(id);
            ViewBag.TermId = term.TermId;
            if (term == null)
            {
                return NotFound();
            }

            return View(term);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(Material model)
        {
            try
            {
                var question = await _context.Materials.FirstOrDefaultAsync(s => s.Id == model.Id);
                ViewBag.TermId = question.TermId;

                if (question == null)
                {
                    return NotFound();
                }

                if (_context.Materials.Any(t => t.Name == model.Name &&  t.TermId == model.TermId && t.Id != model.Id))
                {
                    TempData["ErrorMessage"] = "This Material Name already exists In This Semester.";
                    return RedirectToAction("EditMaterial", new { id = model.Id });
                }

                if (model.M_grade <= 0)
                {
                    TempData["ErrorMessage"] = "Material Grade must be a positive number.";
                    return RedirectToAction("EditMaterial", new { id = model.Id });
                }

                question.Name = model.Name;
                question.M_grade = model.M_grade;

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("updated", "true");
                return RedirectToAction("MaterialIndex", new { id = question.TermId });
            }
            catch (Exception)
            {
                var question = await _context.Materials.FirstOrDefaultAsync(s => s.Id == model.Id);
                ViewBag.YearId = question.TermId;
                return RedirectToAction("MaterialIndex", new { id = question.TermId });
            }
        }

        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var ToDelete = await _context.Materials.FindAsync(id);
            if (ToDelete == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Material with ID '{id}' not found."
                });
            }

            _context.Materials.Remove(ToDelete);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("deleted", "true");
            return RedirectToAction("MaterialIndex", new { id = ToDelete.TermId });
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
    }
}
