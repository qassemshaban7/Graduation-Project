using DocumentFormat.OpenXml.Spreadsheet;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Graduation_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IWebHostEnvironment webHostEnvironment;

        private readonly UserManager<ApplicationUser> usermanager;
        private readonly RoleManager<IdentityRole> rolemanager;
        private readonly IConfiguration config;
        private readonly ApplicationDbContext context;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png", ".jpeg" };

        public UserController(UserManager<ApplicationUser> usermanager, IConfiguration config, RoleManager<IdentityRole> rolemanager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.usermanager = usermanager;
            this.rolemanager = rolemanager;
            this.config = config;
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        //[HttpGet("getallManager")]
        //public async Task<IActionResult> GetAllManager()
        //{
        //    var teachers = await usermanager.GetUsersInRoleAsync("manager");

        //    var teacherData = teachers.Select(u => new { u.Id, u.UserName, u.Email, u.Name, Image = $"Images\\{u.Image}" }).ToList();

        //    return Ok(teacherData);
        //}

        //[Authorize(Roles = "Manager")]
        [HttpGet("getallteachers")]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await usermanager.GetUsersInRoleAsync("teacher");

            var teacherData = teachers.Select(u => new { u.Id, u.UserName,u.NationalNum, u.Email, u.Name, u.Subject, Image = $"http://ablexav1.runasp.net/Images//{u.Image}" }).ToList();

            return Ok(teacherData);
        }

       [HttpGet("getTeacherById/{TeacherId}")]
        public async Task<IActionResult> GetTeacherById(string TeacherId)
        {
            var student = await context.Users
                .Include(u => u.AppLicationUserPClasses)
                 .ThenInclude(auy => auy.PClass)
                 .FirstOrDefaultAsync(x => x.Id == TeacherId);

            if (student == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Teacher with ID {TeacherId} not found."
                });
            }

            var classes = student.AppLicationUserPClasses
               .Select(auy => new
               {
                   classId = auy.PClassId,
                   className = auy.PClass.Name
               })
               .ToList();

            var studentData = new
            {
                Id = student.Id,
                name = student.Name,
                email = student.Email,
                NationalNumber = student.NationalNum,
                //Image = $"http://ablexav1.runasp.net/Images//{student.Image}",
                Subject = student.Subject,
                Classes = classes
            };

            return Ok(studentData);
        }

        //[Authorize(Roles = "Manager,Teacher")]
        [HttpGet("getallstudentd")]
        public async Task<IActionResult> GetAllStudents()
        {
            var stu = await usermanager.GetUsersInRoleAsync("student");

            var DATA = stu.Select(u => new { u.Id, u.UserName, u.NationalNum, u.Email, u.Name, Image = $"http://ablexav1.runasp.net/Images//{u.Image}", u.PClassId }).ToList();

            return Ok(DATA);
        }

        [HttpGet("getStudentById/{studentId}")]
        public async Task<IActionResult> GetStudentById(string studentId)
        {
            var student = await context.Users
                .Include(u => u.ApplicationUserYears)
                .ThenInclude(auy => auy.Year)
                .Include(u => u.ApplicationUserTerms)
                .ThenInclude(auy => auy.Term)
                .FirstOrDefaultAsync(x => x.Id == studentId);

            if (student == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Student with ID {studentId} not found."
                });
            }

            var lastYear = student.ApplicationUserYears?.OrderByDescending(auy => auy.Year.Index).FirstOrDefault();
            var lastTrem = student.ApplicationUserTerms?.OrderByDescending(auy => auy.Term.Index).FirstOrDefault();

            if (lastYear == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"No year data found for student with ID {studentId}."
                });
            }

            if (lastTrem == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"No Term data found for student with ID {studentId}."
                });
            }

            var studentData = new
            {
                Id = student.Id,
                name = student.Name,
                email = student.Email,
                NationalNumber = student.NationalNum,
                //Image = $"http://ablexav1.runasp.net/Images//{student.Image}",
                classId = student.PClassId,
                Term = new
                {
                    TermId = lastTrem.TermId,
                    TermName = lastTrem.Term.TermName
                }
            };

            return Ok(studentData);
        }


        //[Authorize(Roles = "Manager,Teacher")]
        [HttpGet("getstudentsbyclass/{classId}")]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var studentsByClass = await context.Users
                .Where(u => u.PClassId == classId)
                .Select(u => new { u.Id, u.UserName, u.Email, u.NationalNum, u.Name, Image = $"http://ablexav1.runasp.net/Images//{u.Image}"  })
                .ToListAsync();

            return Ok(studentsByClass);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("addStudent")]//api/user/addStudent
        public async Task<IActionResult> AddStudent([FromForm] AddStudentDto studentdto)
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

                if (context.Users.Any(t => t.UserName == user.UserName))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = $"Username '{studentdto.Email.Split("@")[0]}' is already taken."
                    });
                }
                if (context.Users.Any(t => t.NationalNum == studentdto.NationalNum))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "this National Number already exists."
                    });
                }
                if (studentdto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg" , ".jpeg" };
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

                    if (!allowedExtensions.Contains(Path.GetExtension(studentdto.Image.FileName).ToLower()))
                    {
                        return BadRequest(new
                        {
                            status = 400,
                            errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
                        });
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + studentdto.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await studentdto.Image.CopyToAsync(fileStream);
                    }
                    user.Image = uniqueFileName;
                }

                //string AsPassword = "P@ssw0rd";
                //IdentityResult result = await usermanager.CreateAsync(user, AsPassword);
                //if (result.Succeeded)
                //{
                    //var role = "student";
                    //await usermanager.AddToRoleAsync(user, role);

                    if (studentdto.PClassId != null)
                    {
                        var pClass = await context.PClasses.FindAsync(studentdto.PClassId);

                        if (pClass == null)
                        {
                            return BadRequest(new
                            {
                                status = 400,
                                errorMessage = $"Invalid PClass ID: {studentdto.PClassId}"
                            });
                        }

                        user.PClassId = pClass.Id;
                    }
                    var t = await context.Years.FindAsync(studentdto.YearId);
                    if (studentdto.YearId != null)
                    {
                        var term = await context.Years.FindAsync(studentdto.YearId);

                        if (term == null)
                        {
                            return BadRequest(new
                            {
                                status = 400,
                                errorMessage = $"Invalid Year_ID: {studentdto.YearId}"
                            });
                        }
                    }

                    string AsPassword = "P@ssw0rd";
                    IdentityResult result = await usermanager.CreateAsync(user, AsPassword);
                    if (result.Succeeded)
                    {
                        var role = "student";
                        await usermanager.AddToRoleAsync(user, role);
                        context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = user.Id, YearId = t.Id });
                        await context.SaveChangesAsync();
                    return Ok(new
                    {
                        status = 200,
                        message = "Student added successfully"
                    });
                    }
                //}
                return BadRequest(result.Errors.FirstOrDefault());
            }
            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost("AddUserImage")]
        public async Task<IActionResult> AddUserImage(IFormFile Image)
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Image.CopyToAsync(fileStream);
            }
            return Ok(uniqueFileName);
        }


        [Authorize(Roles = "Manager")]
        [HttpPost("addteacher")]//api/user/addteacher
        public async Task<IActionResult> AddTeacher([FromForm] TeacherDto userDto)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.UserName = userDto.Email.Split("@")[0];
                user.NationalNum = userDto.NationalNum;
                user.Subject = userDto.SubjectName;
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;

                if (context.Users.Any(t => t.UserName == user.UserName))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = $"Username '{userDto.Email.Split("@")[0]}' is already taken."
                    });
                }
                if (context.Users.Any(t => t.NationalNum == userDto.NationalNum))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "This National Number already exists."
                    });
                }

                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        return BadRequest(new
                        {
                            status = 400,
                            errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
                        });
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + userDto.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(fileStream);
                    }
                    user.Image = uniqueFileName;
                }

                if (userDto.AssignClassId == null || !userDto.AssignClassId.Any())
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "At least one Class ID is required."
                    });
                }

                foreach (int pClassId in userDto.AssignClassId)
                {
                    var pclass = await context.PClasses.FindAsync(pClassId);

                    if (pclass == null)
                    {
                        return BadRequest(new
                        {
                            status = 400,
                            errorMessage = $"Invalid Class ID: {pClassId}"
                        });
                    }

                    context.AppLicationUserPClasses.Add(new AppLicationUserPClass { UserId = user.Id, PClassId = pclass.Id });
                }

                string AsPassword = "P@ssw0rd";
                IdentityResult result = await usermanager.CreateAsync(user, AsPassword);
                if (result.Succeeded)
                {
                    var role = "teacher";
                    await usermanager.AddToRoleAsync(user, role);
                    await context.SaveChangesAsync();

                    return Ok(new
                    {
                        status = 200,
                        message = "Teacher added successfully"
                    });
                }
                return BadRequest(result.Errors.FirstOrDefault());
            }
            return BadRequest(ModelState);
        }

        //[Authorize(Roles = "Manager")]
        [HttpPost("addmanager")]//api/user/addmanager
        public async Task<IActionResult> AddManager([FromForm] ManagerDto userDto)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.UserName = userDto.Email.Split("@")[0];
                user.NationalNum = userDto.NationalNum;

                if (context.Users.Any(t => t.NationalNum == userDto.NationalNum))
                {
                    return BadRequest("This National Number already exists.");
                }

                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        return BadRequest("Only .png and .jpg and .jpeg images are allowed!");
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + userDto.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(fileStream);
                    }
                    user.Image = uniqueFileName;
                }

                string AsPassword = "P@ssw0rd";
                IdentityResult result = await usermanager.CreateAsync(user, AsPassword);
                if (result.Succeeded)
                {
                    var role = "manager";
                    await usermanager.AddToRoleAsync(user, role);

                    await context.SaveChangesAsync();
                    return Ok("Manager added successfully");
                }
                return BadRequest(result.Errors.FirstOrDefault());
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]//api/user/login
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            if (ModelState.IsValid == true)
            {
                //check - create token
                ApplicationUser user = await usermanager.FindByEmailAsync(userDto.Email);
                if (user != null)//email found
                {
                    bool found = await usermanager.CheckPasswordAsync(user, userDto.Password);
                    if (found)
                    {
                        //Claims Token
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Email, user.Email));
                        claims.Add(new Claim(ClaimTypes.Name, user.Name));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                        //get role
                        var roles = await usermanager.GetRolesAsync(user);
                        foreach (var itemRole in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, itemRole/*.ToString()*/));
                        }
                        SecurityKey securityKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));

                        //Create token
                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            issuer: config["JWT:ValidIssuer"],//url web api
                            audience: config["JWT:ValidAudiance"],//url consumer angular
                            expires: DateTime.Now.AddDays(double.Parse(config["JWT:DurationInDay"])),
                            claims: claims,
                            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                            );
                        return Ok(new
                        {
                            Status =200,
                            token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = mytoken.ValidTo,
                            Id = user.Id,
                            email = user.Email,
                            username = user.UserName,
                            nationalNum = user.NationalNum,
                            photo = $"http://ablexav1.runasp.net/Images//{user.Image}",
                            classId = user.PClassId,
                            roleName = roles.FirstOrDefault()
                        });
                    }
                    return Unauthorized(new 
                    { 
                        status = 401,
                        errorMessage =  "Email or password are invalid" 
                    });
                }
				return Unauthorized(new
				{
					status = 401,
					errorMessage = "Email or password are invalid"
				});
			}
			return Unauthorized(new
			{
				status = 401,
				errorMessage = "Email or password are invalid"
			});
		}

        [Authorize(Roles = "Manager")]
        [HttpPut("editstudent/{userId}")]
        public async Task<IActionResult> Editstudent(string userId, [FromForm] StudentDto userDto) 
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await usermanager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        errorMessage = $"Studnt with ID {userId} not found."
                    });
                }
                string oldImageFileName = user.Image;
                string Oldd = user.Image;


                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.UserName = userDto.Email.Split("@")[0];
                user.NationalNum = userDto.NationalNum;
                if (context.Users.Any(t => t.NationalNum == userDto.NationalNum && t.Id != userId))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "this National Number already exists."
                    });
                }
                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        return BadRequest(new
                        {
                            status = 400,
                            errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
                        });
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + userDto.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(oldImageFileName))
                    {
                        string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    user.Image = uniqueFileName;
                }
                else
                {
                    user.Image = Oldd;
                }

                //IdentityResult result = await usermanager.UpdateAsync(user);
                //if (result.Succeeded)
                //{
                    if (userDto.PClassId != null)
                    {
                        var pClass = await context.PClasses.FindAsync(userDto.PClassId);

                        if (pClass == null)
                        {
                            return BadRequest(new
                            {
                                status = 400,
                                errorMessage = $"Invalid PClass ID: {userDto.PClassId}"
                            });
                        }

                        user.PClassId = pClass.Id;
                    }

                    if (userDto.YearId != null)
                    {
                        var term = await context.Years.FindAsync(userDto.YearId);

                        if (term == null)
                        {
                            return BadRequest(new
                            {
                                status = 400,
                                errorMessage = $"Invalid Year ID: {userDto.YearId}"
                            });
                        }

                        var userCurrentTerm = context.ApplicationUserYear.FirstOrDefault(t => t.UserId == user.Id);
                        if (userCurrentTerm != null)
                        {
                            context.ApplicationUserYear.Remove(userCurrentTerm);
                        }

                    }

                var t = await context.Years.FindAsync(userDto.YearId);

                IdentityResult result = await usermanager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = user.Id, YearId = t.Id });
                    await context.SaveChangesAsync();

                    return Ok(new
                    {
                        status = 200,
                        message = "Student updated successfully"
                    });
                }
                //}
                //return BadRequest(result.Errors.FirstOrDefault());
            }
            return BadRequest(new
            {
                status = 400,
                errorMessage = $"Invalid ModelState"
            });
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("editTeacher/{userId}")]
        public async Task<IActionResult> EditTeacher(string userId, [FromForm] EditTeacherDto userDto)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await usermanager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        errorMessage = $"Teacher with ID {userId} not found."
                    });
                }
                string oldImageFileName = user.Image;
                string Oldd = user.Image;

                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.UserName = userDto.Email.Split("@")[0];
                user.NationalNum = userDto.NationalNum;
                user.Subject = userDto.SubjectName;

                if (context.Users.Any(t => t.NationalNum == userDto.NationalNum && t.Id != userId))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "this National Number already exists."
                    });
                }
                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        return BadRequest(new
                        {
                            status = 400,
                            errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
                        });
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + userDto.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(oldImageFileName))
                    {
                        string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.Image = uniqueFileName;
                }
                else
                {
                    user.Image = Oldd;
                }

                if (userDto.AssignClassId != null)
                {
                    var existingEntries = context.AppLicationUserPClasses
                    .Where(ptu => ptu.UserId == user.Id)
                    .ToList();

                    foreach (var entry in existingEntries)
                    {
                        context.AppLicationUserPClasses.Remove(entry);
                    }
                    await context.SaveChangesAsync();

                    foreach (int pClassId in userDto.AssignClassId)
                    {
                        var pclass = await context.PClasses.FindAsync(pClassId);

                        if (pclass == null)
                        {
                            return BadRequest(new
                            {
                                status = 400,
                                errorMessage = $"Invalid Class ID: {pClassId}"
                            });
                        }

                        var assignment = await context.AppLicationUserPClasses
                            .SingleOrDefaultAsync(apc => apc.UserId == userId && apc.PClassId == pClassId);

                        if (assignment != null)
                        {
                            return NotFound(new
                            {
                                status = 404,
                                errorMessage = $"Assignment for Teacher: '{user.UserName}' and Class : '{pclass.Name}' Already Existing"
                            });
                        }

                        context.AppLicationUserPClasses.Add(new AppLicationUserPClass { UserId = userId, PClassId = pclass.Id });
                    }
                }

                IdentityResult result = await usermanager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await context.SaveChangesAsync();

                    return Ok(new
                    {
                        status = 200,
                        message = "Teacher updated successfully"
                    });
                }
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid ModelState"
                });
            }
            return BadRequest(new
            {
                status = 400,
                errorMessage = "Invalid ModelState"
            });
        }

        //[HttpPut("editManager/{userId}")]
        //public async Task<IActionResult> EditManager(string userId, [FromForm] TeacherDto userDto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ApplicationUser user = await usermanager.FindByIdAsync(userId);
        //        if (user == null)
        //        {
        //            return NotFound($"Manager with ID {userId} not found.");
        //        }
        //        string oldImageFileName = user.Image;

        //        user.Name = userDto.Name;
        //        user.Email = userDto.Email;
        //        user.UserName = userDto.Email.Split("@")[0];
        //        user.NationalNum = userDto.NationalNum;
        //        if (context.Users.Any(t => t.NationalNum == userDto.NationalNum && t.Id != userId))
        //        {
        //            return BadRequest("this National Number already exists.");
        //        }
        //        if (userDto.Image != null)
        //        {
        //            string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
        //            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

        //            if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
        //            {
        //                return BadRequest("Only .png and .jpg and .jpeg images are allowed!");
        //            }

        //            string uniqueFileName = Guid.NewGuid().ToString() + "_" + userDto.Image.FileName;
        //            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //            await using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await userDto.Image.CopyToAsync(fileStream);
        //            }

        //            if (!string.IsNullOrEmpty(oldImageFileName))
        //            {
        //                string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
        //                if (System.IO.File.Exists(oldFilePath))
        //                {
        //                    System.IO.File.Delete(oldFilePath);
        //                }
        //            }

        //            user.Image = uniqueFileName;
        //        }

        //        IdentityResult result = await usermanager.UpdateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            await context.SaveChangesAsync();
        //            return Ok("Manager Account updated successfully");
        //        }
        //        return BadRequest(result.Errors.FirstOrDefault());
        //    }
        //    return BadRequest(ModelState);
        //}

        [Authorize(Roles = "Manager")]
        [HttpDelete("deleteuser/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await usermanager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"User with ID {userId} not found."
                });
            }

            string oldImageFileName = user.Image;

            var result = await usermanager.DeleteAsync(user);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(oldImageFileName))
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                return Ok(new
                {
                    status = 200,
                    message = "Account Delete Success"
                });
            }
            return BadRequest(new
            {
                status = 400,
                errorMessage = "Invalid model state."
            });
            //return BadRequest(result.Errors);
        }
        [HttpPost("send_reset_code")]
        public async Task<IActionResult> SendResetCode(SendPINDto model, [FromServices] IEmailProvider _emailProvider)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid ModelState"
                });
            }
            var user = await usermanager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = "Email Not Found!"
                });
            }
            int pin = await _emailProvider.SendResetCode(model.Email);
            user.PasswordResetPin = pin;
            user.ResetExpires = DateTime.Now.AddMinutes(10);
            var expireTime = user.ResetExpires.Value.ToString("hh:mm tt");
            await usermanager.UpdateAsync(user);
            return Ok(new
            {
                status = 200,
                ExpireAt = "expired at " + expireTime,
                email = model.Email,
            });
        }
        [HttpPost("verify_pin/{email}")]
        public async Task<IActionResult> VerifyPin([FromBody] VerfiyPINDto model, [FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid ModelState"
                });
            }
            var user = await usermanager.FindByEmailAsync(email);
            if (user is null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = "Email Not Found!"
                });
            }
            if (user.ResetExpires < DateTime.Now || user.ResetExpires is null) 
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Time Expired try to send new pin"
                });
            }
            if (user.PasswordResetPin != model.pin)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid pin"
                });
            }
            user.ResetExpires = null;
            user.PasswordResetPin = null;
            await usermanager.UpdateAsync(user);
            return Ok(new
            {
                status = 200,
                message = "PIN verified successfully",
                email = user.Email,
            });
        }

        [HttpPost("forget_password/{email}")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPassDto model, [FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid model state."
                });
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "New password and confirm new password do not match."
                });
            }

            var user = await usermanager.FindByEmailAsync(email);
            if (user is null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Email Not Found!"
                });
            }
            var token = await usermanager.GeneratePasswordResetTokenAsync(user);
            var result = await usermanager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                await usermanager.UpdateAsync(user);
                return Ok(new
                {
                    status = 200,
                    message = "Password changed successfully"
                });
            }
            return BadRequest(new
            {
                status = 400,
                errorMessage = "Invalid model state."
            });
            //return BadRequest(result.Errors.FirstOrDefault());
        }

        [Authorize(Roles = "Manager,Teacher,Student")]
        [HttpPost("changepassword/{userId}")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, [FromRoute] string userId)
        {
            if (ModelState.IsValid)
            {
                var user = await usermanager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        errorMessage = $"User with ID {userId} not found."
                    });
                }

                var passwordVerificationResult = await usermanager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword);
                if (!passwordVerificationResult)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "Incorrect current password."
                    });
                }

                var result = await usermanager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        status = 200,
                        message = "Password changed successfully"
                    });
                }
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid model state."
                });
            }
            return BadRequest(new
            {
                status = 400,
                errorMessage = "Invalid model state."
            });
        }

        //[Authorize(Roles = "Manager")]
        //[HttpPost("AssignMaterialToTeacher/{teacherId}")]
        //public async Task<IActionResult> AssignMaterialToTeacher(AssignMaterialtoTeacherDto MTDto, [FromRoute] string teacherId)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await usermanager.FindByIdAsync(teacherId);
        //        if (user == null)
        //        {
        //            return NotFound(new
        //            {
        //                status = 404,
        //                errorMessage = $"Teacher with ID {teacherId} not found."
        //            });
        //        }
        //    }

        //    if (MTDto.MaterialId != null)
        //    {
        //        var material = await context.Terms.FindAsync(MTDto.MaterialId);

        //        if (material == null)
        //        {
        //            return BadRequest(new
        //            {
        //                status = 400,
        //                errorMessage = $"Invalid Material ID: {MTDto.MaterialId}"
        //            });
        //        }

        //        context.ApplicationUserMaterial.Add(new ApplicationUserMaterial { UserId = teacherId, MaterialId = material.Id });
        //        await context.SaveChangesAsync();
        //    }
        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Assign Material to Teacher is succeeded"
        //    });
        //}

        //[Authorize(Roles = "Manager")]
        //[HttpDelete("UnAssignMaterialFromTeacher/{teacherId}/{materialId}")]
        //public async Task<IActionResult> UnAssignMaterialFromTeacher([FromRoute] string teacherId, [FromRoute] int materialId)
        //{
        //    var user = await usermanager.FindByIdAsync(teacherId);
        //    if (user == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Teacher with ID {teacherId} not found."
        //        });
        //    }

        //    var material = await context.Materials.FindAsync(materialId);
        //    if (material == null)
        //    {
        //        return BadRequest(new
        //        {
        //            status = 400,
        //            errorMessage = $"Invalid Material ID: {materialId}"
        //        });
        //    }

        //    var assignment = await context.ApplicationUserMaterial
        //        .SingleOrDefaultAsync(aum => aum.UserId == teacherId && aum.MaterialId == materialId);

        //    if (assignment == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Assignment not found for Teacher ID: {teacherId} and Material ID: {materialId}"
        //        });
        //    }

        //    context.ApplicationUserMaterial.Remove(assignment);
        //    await context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "UnAssign Material for Teacher is succeeded"
        //    });
        //}

        //[Authorize(Roles = "Manager")]
        //[HttpPost("AssignClassToTeacher/{teacherId}")]
        //public async Task<IActionResult> AssignClassToTeacher(AssignClassToTeacher CDto, [FromRoute] string teacherId)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await usermanager.FindByIdAsync(teacherId);
        //        if (user == null)
        //        {
        //            return NotFound(new
        //            {
        //                status = 404,
        //                errorMessage = $"Teacher with ID {teacherId} not found."
        //            });
        //        }
        //    }

        //    if (CDto.PClassId != null)
        //    {
        //        var pclass = await context.PClasses.FindAsync(CDto.PClassId);

        //        if (pclass == null)
        //        {
        //            return BadRequest(new
        //            {
        //                status = 400,
        //                errorMessage = $"Invalid Class ID: {CDto.PClassId}"
        //            });
        //        }

        //        context.AppLicationUserPClasses.Add(new AppLicationUserPClass { UserId = teacherId, PClassId = pclass.Id });
        //        await context.SaveChangesAsync();
        //    }
        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Assign Class to Teacher is succeeded"
        //    });
        //}
        //[Authorize(Roles = "Manager")]
        //[HttpDelete("UnAssignClassFromTeacher/{teacherId}/{pClassId}")]
        //public async Task<IActionResult> UnAssignClassFromTeacher([FromRoute] string teacherId, [FromRoute] int pClassId)
        //{
        //    var user = await usermanager.FindByIdAsync(teacherId);
        //    if (user == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Teacher with ID {teacherId} not found."
        //        });
        //    }

        //    var pclass = await context.PClasses.FindAsync(pClassId);
        //    if (pclass == null)
        //    {
        //        return BadRequest(new
        //        {
        //            status = 400,
        //            errorMessage = $"Invalid Class ID: {pClassId}"
        //        });
        //    }

        //    var assignment = await context.AppLicationUserPClasses
        //        .SingleOrDefaultAsync(apc => apc.UserId == teacherId && apc.PClassId == pClassId);

        //    if (assignment == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Assignment not found for Teacher ID: {teacherId} and Class ID: {pClassId}"
        //        });
        //    }

        //    context.AppLicationUserPClasses.Remove(assignment);
        //    await context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "UnAssign Class for Teacher is succeeded"
        //    });
        //}

        //[Authorize(Roles = "Manager")]
        //[HttpPost("AssignClassToTeacher/{teacherId}")]
        //public async Task<IActionResult> AssignClassToTeacher([FromRoute] string teacherId, [FromForm][Required] List<int> pClassIds)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await usermanager.FindByIdAsync(teacherId);
        //        if (user == null)
        //        {
        //            return NotFound(new
        //            {
        //                status = 404,
        //                errorMessage = $"Teacher with ID {teacherId} not found."
        //            });
        //        }
        //    }

        //    if (pClassIds == null || !pClassIds.Any())
        //    {
        //        return BadRequest(new
        //        {
        //            status = 400,
        //            errorMessage = "At least one Class ID is required."
        //        });
        //    }

        //    foreach (int pClassId in pClassIds) 
        //    {
        //        var pclass = await context.PClasses.FindAsync(pClassId);

        //        if (pclass == null)
        //        {
        //            return BadRequest(new
        //            {
        //                status = 400,
        //                errorMessage = $"Invalid Class ID: {pClassId}"
        //            });
        //        }

        //        context.AppLicationUserPClasses.Add(new AppLicationUserPClass { UserId = teacherId, PClassId = pclass.Id });
        //    }

        //    await context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Assign Class to Teacher is succeeded"
        //    });
        //}

        //[Authorize(Roles = "Manager")]
        //[HttpDelete("UnAssignClassFromTeacher/{teacherId}")]
        //public async Task<IActionResult> UnAssignClassFromTeacher([FromRoute] string teacherId, [FromForm][Required] List<int> pClassIds)
        //{
        //    var user = await usermanager.FindByIdAsync(teacherId);
        //    if (user == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Teacher with ID {teacherId} not found."
        //        });
        //    }

        //    if (pClassIds == null || !pClassIds.Any())
        //    {
        //        return BadRequest(new
        //        {
        //            status = 400,
        //            errorMessage = "At least one Class ID is required."
        //        });
        //    }

        //    foreach (int pClassId in pClassIds) // تكرار لكل فصل في القائمة
        //    {
        //        var pclass = await context.PClasses.FindAsync(pClassId);
        //        if (pclass == null)
        //        {
        //            return BadRequest(new
        //            {
        //                status = 400,
        //                errorMessage = $"Invalid Class ID: {pClassId}"
        //            });
        //        }

        //        var assignment = await context.AppLicationUserPClasses
        //            .SingleOrDefaultAsync(apc => apc.UserId == teacherId && apc.PClassId == pClassId);

        //        if (assignment == null)
        //        {
        //            return NotFound(new
        //            {
        //                status = 404,
        //                errorMessage = $"Assignment not found for Teacher ID: {teacherId} and Class ID: {pClassId}"
        //            });
        //        }

        //        context.AppLicationUserPClasses.Remove(assignment);
        //    }

        //    await context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "UnAssign Class for Teacher is succeeded"
        //    });
        //}


        //[Authorize(Roles = "Manager")]
        //[HttpPut("testeditTeacher/{userId}")]
        //public async Task<IActionResult> TestEditTeacher(string userId, [FromForm] TestEditTeacherDto userDto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ApplicationUser user = await usermanager.FindByIdAsync(userId);
        //        if (user == null)
        //        {
        //            return NotFound(new
        //            {
        //                status = 404,
        //                errorMessage = $"Teacher with ID {userId} not found."
        //            });
        //        }
        //        string oldImageFileName = user.Image;
        //        string Oldd = user.Image;

        //        user.Name = userDto.Name;
        //        user.Email = userDto.Email;
        //        user.UserName = userDto.Email.Split("@")[0];
        //        user.NationalNum = userDto.NationalNum;
        //        user.Subject = userDto.SubjectName;

        //        if (context.Users.Any(t => t.NationalNum == userDto.NationalNum && t.Id != userId))
        //        {
        //            return BadRequest(new
        //            {
        //                status = 400,
        //                errorMessage = "this National Number already exists."
        //            });
        //        }
        //        if (userDto.Image != null)
        //        {
        //            string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
        //            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

        //            if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
        //            {
        //                return BadRequest(new
        //                {
        //                    status = 400,
        //                    errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
        //                });
        //            }

        //            string uniqueFileName = Guid.NewGuid().ToString() + "_" + userDto.Image.FileName;
        //            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //            await using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await userDto.Image.CopyToAsync(fileStream);
        //            }

        //            if (!string.IsNullOrEmpty(oldImageFileName))
        //            {
        //                string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
        //                if (System.IO.File.Exists(oldFilePath))
        //                {
        //                    System.IO.File.Delete(oldFilePath);
        //                }
        //            }

        //            user.Image = uniqueFileName;
        //        }
        //        else
        //        {
        //            user.Image = Oldd;
        //        }

        //        if (userDto.AssignClassId != null)
        //        {
        //            foreach (int pClassId in userDto.AssignClassId)
        //            {
        //                var pclass = await context.PClasses.FindAsync(pClassId);

        //                if (pclass == null)
        //                {
        //                    return BadRequest(new
        //                    {
        //                        status = 400,
        //                        errorMessage = $"Invalid Class ID: {pClassId}"
        //                    });
        //                }

        //                var assignment = await context.AppLicationUserPClasses
        //                    .SingleOrDefaultAsync(apc => apc.UserId == userId && apc.PClassId == pClassId);

        //                if (assignment != null)
        //                {
        //                    return NotFound(new
        //                    {
        //                        status = 404,
        //                        errorMessage = $"Assignment for Teacher: '{user.UserName}' and Class : '{pclass.Name}' Already Existing"
        //                    });
        //                }

        //                context.AppLicationUserPClasses.Add(new AppLicationUserPClass { UserId = userId, PClassId = pclass.Id });
        //            }
        //        }
        //        if (userDto.UnAssignClassId != null)
        //        {
        //            foreach (int pClassId in userDto.UnAssignClassId)
        //            {
        //                var pclass1 = await context.PClasses.FindAsync(pClassId);
        //                if (pclass1 == null)
        //                {
        //                    return BadRequest(new
        //                    {
        //                        status = 400,
        //                        errorMessage = $"Invalid Class ID: {pClassId}"
        //                    });
        //                }

        //                var assignment = await context.AppLicationUserPClasses
        //                    .SingleOrDefaultAsync(apc => apc.UserId == userId && apc.PClassId == pClassId);

        //                if (assignment == null)
        //                {
        //                    return NotFound(new
        //                    {
        //                        status = 404,
        //                        errorMessage = $"Assignment not found for Teacher: '{user.UserName}' and Class : '{pclass1.Name}'"
        //                    });
        //                }

        //                context.AppLicationUserPClasses.Remove(assignment);
        //            }
        //        }

        //        IdentityResult result = await usermanager.UpdateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            await context.SaveChangesAsync();

        //            return Ok(new
        //            {
        //                status = 200,
        //                message = "Teacher updated successfully"
        //            });
        //        }
        //        return BadRequest(new
        //        {
        //            status = 400,
        //            errorMessage = "Invalid ModelState"
        //        });
        //        //return BadRequest(result.Errors.FirstOrDefault());
        //    }
        //    return BadRequest(new
        //    {
        //        status = 400,
        //        errorMessage = "Invalid ModelState"
        //    });
        //}
    }
}