using GP_MVC.Models;
using GP_MVC.Utility;
using GP_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace GP_MVC.Areas.Manager.Controllers
{
    [Authorize(Roles = StaticDetails.Manager)]
    [Area(nameof(Manager))]
    [Route(nameof(Manager) + "/[controller]/[action]")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> usermanager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public TeacherController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> usermanager, SignInManager<IdentityUser> signInManager)
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
                                  where role.Name == StaticDetails.Teacher
                                  select x)
                                 .OrderBy(h => h.Name)
                              .Include(x => x.AppLicationUserPClasses)
                              .ThenInclude(y => y.PClass)
                              .ToListAsync();

            return View(Students);
        }


        [HttpGet]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["DefaultMateriales"] = new SelectList(await _context.DefaultMateriales.ToListAsync(), "Id", "Name");
            ViewData["PClasses"] = new SelectList(await _context.PClasses.ToListAsync(), "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddTeacherVM userDto)
        {
            var errorMessages = new List<string>();

            if (ModelState.IsValid)
            {
                if (userDto.AssignClassId == null)
                {
                    errorMessages.Add("Class is required.");
                }

                if (userDto.SubjectName == null)
                {
                    errorMessages.Add("SubjectName is required.");
                }

                if (userDto.Image == null)
                {
                    errorMessages.Add("Image is required.");
                }

                if (errorMessages.Any())
                {
                    ViewBag.ErrorMessages = errorMessages;
                    return View(userDto);
                }

                ApplicationUser user = new ApplicationUser
                {
                    Name = userDto.Name,
                    Email = userDto.Email,
                    UserName = userDto.Email.Split("@")[0],
                    NationalNum = userDto.NationalNum,
                    Subject = userDto.SubjectName,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                if (_context.Users.Any(t => t.UserName == user.UserName))
                {
                    errorMessages.Add($"Username '{userDto.Email.Split("@")[0]}' is already taken.");
                }
                if (_context.Users.Any(t => t.NationalNum == userDto.NationalNum))
                {
                    errorMessages.Add("This National Number already exists.");
                }

                if (errorMessages.Any())
                {
                    ViewBag.ErrorMessages = errorMessages;
                    return View(userDto);
                }

                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        errorMessages.Add("Only .png, .jpg, and .jpeg images are allowed!");
                        ViewBag.ErrorMessages = errorMessages;
                        return View(userDto);
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
                                    errorMessages.Add("Failed to upload image.");
                                    ViewBag.ErrorMessages = errorMessages;
                                    return View(userDto);
                                }
                            }
                        }
                    }
                }

                if (userDto.AssignClassId == null || !userDto.AssignClassId.Any())
                {
                    errorMessages.Add("At least one Class ID is required.");
                    ViewBag.ErrorMessages = errorMessages;
                    return View(userDto);
                }

                foreach (int pClassId in userDto.AssignClassId)
                {
                    var pclass = await _context.PClasses.FindAsync(pClassId);

                    if (pclass == null)
                    {
                        errorMessages.Add($"Invalid Class ID: {pClassId}");
                        ViewBag.ErrorMessages = errorMessages;
                        return View(userDto);
                    }

                    _context.AppLicationUserPClasses.Add(new AppLicationUserPClass { UserId = user.Id, PClassId = pclass.Id });
                }

                string AsPassword = "P@ssw0rd";
                IdentityResult result = await usermanager.CreateAsync(user, AsPassword);
                if (result.Succeeded)
                {
                    var role = "teacher";
                    await usermanager.AddToRoleAsync(user, role);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction(nameof(Index));
                }
                errorMessages.Add(result.Errors.FirstOrDefault()?.Description ?? "An error occurred.");
                ViewBag.ErrorMessages = errorMessages;
                return View(userDto);
            }
            errorMessages.AddRange(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            ViewData["DefaultMateriales"] = new SelectList(await _context.DefaultMateriales.ToListAsync(), "Id", "Name");
            ViewData["PClasses"] = new SelectList(await _context.PClasses.ToListAsync(), "Id", "Name");
            ViewBag.ErrorMessages = errorMessages;
            return View(userDto);
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

            var assignedClasses = await _context.AppLicationUserPClasses
                                                .Where(apc => apc.UserId == user.Id)
                                                .Select(apc => apc.PClassId)
                                                .ToListAsync();

            var viewModel = new EditTeacherVM
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                NationalNum = user.NationalNum,
                SubjectName = user.Subject,
                AssignClassId = assignedClasses
            };

            ViewData["DefaultMateriales"] = new SelectList(await _context.DefaultMateriales.ToListAsync(), "Id", "Name", viewModel.SubjectName);
            ViewData["PClasses"] = new SelectList(await _context.PClasses.ToListAsync(), "Id", "Name");
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTeacherVM userDto)
        {
            if (ModelState.IsValid)
            {
                var errorMessages = new List<string>();

                var user = await _context.Users.FindAsync(userDto.Id);
                if (user == null)
                {
                    errorMessages.Add($"Student with ID {userDto.Id} not found.");
                    ViewBag.ErrorMessages = errorMessages;
                    return View(userDto);
                }

                if (userDto.AssignClassId == null)
                {
                    errorMessages.Add("Class is required.");
                }

                if (userDto.SubjectName == null)
                {
                    errorMessages.Add("SubjectName is required.");
                }

                if (errorMessages.Any())
                {
                    ViewBag.ErrorMessages = errorMessages;
                    return View(userDto);
                }

                string oldImageFileName = user.Image;

                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.UserName = userDto.Email.Split("@")[0];
                user.NationalNum = userDto.NationalNum;
                user.Subject = userDto.SubjectName;

                if (_context.Users.Any(t => t.NationalNum == userDto.NationalNum && t.Id != userDto.Id))
                {
                    errorMessages.Add("This National Number already exists.");
                    ViewBag.ErrorMessages = errorMessages;
                    return View(userDto);
                }

                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        errorMessages.Add("Only .png, .jpg, and .jpeg images are allowed!");
                        ViewBag.ErrorMessages = errorMessages;
                        return View(userDto);
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
                                    errorMessages.Add("Failed to upload image.");
                                    ViewBag.ErrorMessages = errorMessages;
                                    return View(userDto);
                                }
                            }
                        }
                    }
                }
                else
                {
                    user.Image = oldImageFileName;
                }

                var existingEntries = _context.AppLicationUserPClasses
                    .Where(ptu => ptu.UserId == user.Id)
                    .ToList();

                foreach (var entry in existingEntries)
                {
                    _context.AppLicationUserPClasses.Remove(entry);
                }
                await _context.SaveChangesAsync();

                foreach (var pClassId in userDto.AssignClassId.Distinct())
                {
                    var pclass = await _context.PClasses.FindAsync(pClassId);

                    if (pclass == null)
                    {
                        errorMessages.Add($"Invalid Class ID: {pClassId}");
                        ViewBag.ErrorMessages = errorMessages;
                        return View(userDto);
                    }

                    _context.AppLicationUserPClasses.Add(new AppLicationUserPClass { UserId = userDto.Id, PClassId = pClassId });
                }

                var result = await usermanager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction(nameof(Index));
                }

                errorMessages.AddRange(result.Errors.Select(e => e.Description));
                ViewBag.ErrorMessages = errorMessages;
            }

            ViewBag.ErrorMessages = new List<string> { "Invalid ModelState" };
            return View(userDto);
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
