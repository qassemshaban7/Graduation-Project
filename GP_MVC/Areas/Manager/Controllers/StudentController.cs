using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System;

namespace GP_MVC.Areas.Manager.Controllers
{
    [Authorize(Roles = StaticDetails.Manager)]
    [Area(nameof(Manager))]
    [Route(nameof(Manager) + "/[controller]/[action]")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> usermanager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public StudentController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> usermanager, SignInManager<IdentityUser> signInManager)
        {
            this.usermanager = usermanager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

            var Students = await (from x in _context.Users
                                 join userRole in _context.UserRoles
                                 on x.Id equals userRole.UserId
                                 join role in _context.Roles
                                 on userRole.RoleId equals role.Id
                                 where role.Name == StaticDetails.Student
                                 select x)
                                 .OrderBy(h => h.Name)
                              .Include(x => x.PClass)
                              .Include(x => x.ApplicationUserYears)
                              .ThenInclude(y => y.Year)
                              .ToListAsync();

            return View(Students);
        }


        [HttpGet]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Years"] = new SelectList(await _context.Years.ToListAsync(), "Id", "Name", "Index");
            ViewData["PClasses"] = new SelectList(await _context.PClasses.ToListAsync(), "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddStudentVM studentdto)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                user.Name = studentdto.Name;
                user.Email = studentdto.Email;
                user.UserName = studentdto.Email.Split("@")[0];
                user.NationalNum = studentdto.NationalNum;
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;

                if (_context.Users.Any(t => t.UserName == user.UserName))
                {
                    TempData["ErrorMessage"] = $"Username '{studentdto.Email.Split("@")[0]}' is already taken.";
                    return RedirectToAction("Create");
                }
                if (_context.Users.Any(t => t.NationalNum == studentdto.NationalNum))
                {
                    TempData["ErrorMessage"] = "this National Number already exists.";
                    return RedirectToAction("Create");
                }
                if (studentdto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    if (!allowedExtensions.Contains(Path.GetExtension(studentdto.Image.FileName).ToLower()))
                    {
                        TempData["ErrorMessage"] = "Only .png and .jpg and .jpeg images are allowed!";
                        return RedirectToAction("Create");
                    }

                    // Send image to API
                    using (var httpClient = new HttpClient())
                    {
                        using (var form = new MultipartFormDataContent())
                        {
                            using (var fileStream = studentdto.Image.OpenReadStream())
                            {
                                form.Add(new StreamContent(fileStream), "Image", studentdto.Image.FileName);

                                var response = await httpClient.PostAsync("http://ablexav1.runasp.net/api/User/AddUserImage", form);

                                if (response.IsSuccessStatusCode)
                                {
                                    var uniqueFileName = await response.Content.ReadAsStringAsync();
                                    user.Image = uniqueFileName;
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Failed to upload image.";
                                    return RedirectToAction("Create");
                                }
                            }
                        }
                    }
                }

                if (studentdto.PClassId != null)
                {
                    var pClass = await _context.PClasses.FindAsync(studentdto.PClassId);

                    if (pClass == null)
                    {
                        TempData["ErrorMessage"] = $"Invalid Class ID: {studentdto.PClassId}";
                        return RedirectToAction("Create");
                    }

                    user.PClassId = pClass.Id;
                }
                var t = await _context.Years.FindAsync(studentdto.YearId);
                if (studentdto.YearId != null)
                {
                    var term = await _context.Years.FindAsync(studentdto.YearId);

                    if (term == null)
                    {
                        TempData["ErrorMessage"] = $"Invalid Year_ID: {studentdto.YearId}";
                        return RedirectToAction("Create");
                    }
                }

                string AsPassword = "P@ssw0rd";
                IdentityResult result = await usermanager.CreateAsync(user, AsPassword);
                if (result.Succeeded)
                {
                    var role = "student";
                    await usermanager.AddToRoleAsync(user, role);
                    _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = user.Id, YearId = t.Id });

                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction(nameof(Index));
                }

                return BadRequest(result.Errors.FirstOrDefault());
            }
            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditStudentVM
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                NationalNum = user.NationalNum,
                PClassId = user.PClassId ?? 0,
                YearId = _context.ApplicationUserYear.FirstOrDefault(t => t.UserId == user.Id)?.YearId ?? 0
            };

            ViewData["Years"] = new SelectList(await _context.Years.ToListAsync(), "Id", "Name", viewModel.YearId);
            ViewData["PClasses"] = new SelectList(await _context.PClasses.ToListAsync(), "Id", "Name", viewModel.PClassId);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( EditStudentVM userDto)
        {
            if (ModelState.IsValid)
            {
                if (userDto.YearId == null || userDto.YearId == 0)
                {
                    TempData["ErrorMessage"] = "Year is required.";
                    return RedirectToAction("Edit", new { id = userDto.Id });
                }

                if (userDto.PClassId == null || userDto.PClassId == 0)
                {
                    TempData["ErrorMessage"] = "Class is required.";
                    return RedirectToAction("Edit", new { id = userDto.Id });
                }

                ApplicationUser user = await _context.Users.FindAsync(userDto.Id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = $"Studnt with ID {userDto.Id} not found.";
                    return RedirectToAction("Edit", new { id = userDto.Id });
                }
                string oldImageFileName = user.Image;
                string Oldd = user.Image;


                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.UserName = userDto.Email.Split("@")[0];
                user.NationalNum = userDto.NationalNum;
                if (_context.Users.Any(t => t.NationalNum == userDto.NationalNum && t.Id != userDto.Id))
                {
                    TempData["ErrorMessage"] = "this National Number already exists.";
                    return RedirectToAction("Edit", new { id = userDto.Id });
                }
                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        TempData["ErrorMessage"] = "Only .png and .jpg and .jpeg images are allowed!";
                        return RedirectToAction("Edit", new { id = userDto.Id });
                    }

                    // Send image to API
                    using (var httpClient = new HttpClient())
                    {
                        using (var form = new MultipartFormDataContent())
                        {
                            using (var fileStream = userDto.Image.OpenReadStream())
                            {
                                form.Add(new StreamContent(fileStream), "Image", userDto.Image.FileName);

                                var response = await httpClient.PostAsync("http://ablexav1.runasp.net/api/User/AddUserImage", form);

                                if (response.IsSuccessStatusCode)
                                {
                                    var uniqueFileName = await response.Content.ReadAsStringAsync();
                                    user.Image = uniqueFileName;
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Failed to upload image.";
                                    return RedirectToAction("Edit", new { id = userDto.Id });
                                }
                            }
                        }
                    }
                }
                else
                {
                    user.Image = Oldd;
                }

                if (userDto.PClassId != null)
                {
                    var pClass = await _context.PClasses.FindAsync(userDto.PClassId);

                    if (pClass == null)
                    {
                        TempData["ErrorMessage"] = $"Invalid PClass ID: {userDto.PClassId}";
                        return RedirectToAction("Edit", new { id = userDto.Id });
                    }

                    user.PClassId = pClass.Id;
                }

                if (userDto.YearId != null)
                {
                    var term = await _context.Years.FindAsync(userDto.YearId);

                    if (term == null)
                    {
                        TempData["ErrorMessage"] = $"Invalid Year ID: {userDto.YearId}";
                        return RedirectToAction("Edit", new { id = userDto.Id });
                    }

                    var userCurrentTerm = _context.ApplicationUserYear.FirstOrDefault(t => t.UserId == user.Id);
                    if (userCurrentTerm != null)
                    {
                        _context.ApplicationUserYear.Remove(userCurrentTerm);
                    }

                }

                var t = await _context.Years.FindAsync(userDto.YearId);

                IdentityResult result = await usermanager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = user.Id, YearId = t.Id });
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["ErrorMessage"] = $"Invalid ModelState";
            return RedirectToAction("Edit", new { id = userDto.Id });
        }

        public async Task<IActionResult> Delete(string Id)
        {
            var user = await _context.Users.FindAsync(Id);
            if (user == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"User with ID {Id} not found."
                });
            }

            string oldImageFileName = user.Image;

            var results = _context.Results.Where(rs => rs.UserId == Id).ToList();
            _context.Results.RemoveRange(results);
            await _context.SaveChangesAsync();

            var result = await usermanager.DeleteAsync(user);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(oldImageFileName))
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction(nameof(Index));

            }
            return BadRequest(new
            {
                status = 400,
                errorMessage = "Invalid model state."
            });
        }

    }
}
