using DocumentFormat.OpenXml.Drawing.Diagrams;
using Graduation_Project.DTO;
using Graduation_Project.Helper;
using Graduation_Project.Models;
using Graduation_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.IO.Compression;
using static Azure.Core.HttpHeader;

namespace Graduation_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelAttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IManageImage _iManageImage;

        public ModelAttendanceController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IManageImage iManageImage)
        {
            this.context = context;
            _webHostEnvironment = webHostEnvironment;
            _iManageImage = iManageImage;
        }


        [HttpGet("DownloadImages")]
        public async Task<IActionResult> DownloadImages()
        {
            try
            {
                var students = await context.Users
                    .Where(u => u.PClassId == 500)
                    .ToListAsync();

                if (students.Count == 0)
                {
                    return NotFound(new { status = 404, errorMessage = "There are no students in this class." });
                }

                var tempFolderPath = Path.Combine(Path.GetTempPath(), "StudentsImages");
                Directory.CreateDirectory(tempFolderPath);

                foreach (var student in students)
                {
                    var fileName = student.Image;
                    var filePath = CommonA.GetFilePath(fileName);

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                    var tempFilePath = Path.Combine(tempFolderPath, fileName);
                    await System.IO.File.WriteAllBytesAsync(tempFilePath, fileBytes);
                }

                var directoryInfo = new DirectoryInfo(tempFolderPath);
                var zipFileName = "StudentsImages.zip";
                var zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);
                ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);

                var memoryStream = new MemoryStream();
                using (var zipStream = new FileStream(zipFilePath, FileMode.Open))
                {
                    await zipStream.CopyToAsync(memoryStream);
                }

                Directory.Delete(tempFolderPath, true);
                System.IO.File.Delete(zipFilePath);

                memoryStream.Position = 0;
                return File(memoryStream, "multipart/form-data", zipFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, errorMessage = "Failed to download files." });
            }
        }


        //[HttpGet("DownloadFiles1")]
        //public async Task<IActionResult> DownloadFiles1()
        //{
        //    try
        //    {
        //        var students = await context.Users
        //            .Where(u => u.PClassId == 500)
        //            .ToListAsync();

        //        if (students.Count == 0)
        //        {
        //            return NotFound(new { status = 404, errorMessage = "There are no students in this class." });
        //        }

        //        List<(byte[], string, string)> files = new List<(byte[], string, string)>();

        //        foreach (var student in students)
        //        {
        //            var fileName = student.Image;
        //            var filePath = CommonA.GetFilePath(fileName);

        //            var provider = new FileExtensionContentTypeProvider();
        //            if (!provider.TryGetContentType(filePath, out var contentType))
        //            {
        //                contentType = "application/octet-stream";
        //            }

        //            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

        //            files.Add((fileBytes, contentType, fileName));
        //        }

        //        foreach (var file in files)
        //        {
        //            return File(file.Item1, file.Item2, file.Item3);
        //        }

        //        return NotFound(new { status = 404, errorMessage = "No files were downloaded." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { status = 500, errorMessage = "Failed to download files." });
        //    }
        //}

        //[HttpGet("DownloadImages")]
        //public async Task<IActionResult> DownloadImages(string FileName)
        //{
        //    try
        //    {
        //        string imagesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
        //        var filePath = Path.Combine(imagesFolder, FileName);

        //        var provider = new FileExtensionContentTypeProvider();
        //        if (!provider.TryGetContentType(filePath, out var contentType))
        //        {
        //            contentType = "application/octet-stream";
        //        }

        //        return PhysicalFile(filePath, contentType, Path.GetFileName(filePath));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}


        [HttpGet("GetStudentsImage")]
        public async Task<IActionResult> GetStudentsImage()
        {
            var studentsByClass = await context.Users
                .Where(u => u.PClassId == 500)
                .Select(u => new { Image = Path.GetFileNameWithoutExtension(u.Image) })
                .ToListAsync();

            return Ok(studentsByClass);
        }
        [HttpGet("GetStudentsImagewithPathAndExtention")]
        public async Task<IActionResult> GetStudentsImagewithPathAndExtention()
        {
            var studentsByClass = await context.Users
                .Where(u => u.PClassId == 500)
                .Select(u => new { Image = $"http://ablexav1.runasp.net/Images//{u.Image}" })
                .ToListAsync();
            return Ok(studentsByClass);
        }

        [HttpGet("GetStudentsImagewithPathOnly")]
        public async Task<IActionResult> GetStudentsImagewithPathOnly()
        {
            var studentsByClass = await context.Users
                .Where(u => u.PClassId == 500)
                .Select(u => new { Image = $"http://ablexav1.runasp.net/Images//{Path.GetFileNameWithoutExtension(u.Image)}" })
                .ToListAsync();
            return Ok(studentsByClass);
        }

        [HttpPost("AddAttendance")]
        public async Task<IActionResult> AddAttendance([FromBody] AddAttendenceDto attendanceDto)
        {
            try
            {
                attendanceDto.ImageName = attendanceDto.ImageName + ".jpg";
                var student = await context.Users.FirstOrDefaultAsync(x => x.Image == attendanceDto.ImageName);

                if (student == null)
                {
                    attendanceDto.ImageName = attendanceDto.ImageName.Replace(".jpg", ".jpeg");
                    student = await context.Users.FirstOrDefaultAsync(x => x.Image == attendanceDto.ImageName);
                }
                if (student == null)
                {
                    attendanceDto.ImageName = attendanceDto.ImageName.Replace(".jpeg", ".png");
                    student = await context.Users.FirstOrDefaultAsync(x => x.Image == attendanceDto.ImageName);
                }

                if (student == null)
                {
                    return NotFound(new { status = 404, errorMessage = "Student not found." });
                }

                DateTime now = DateTime.Now;

                TimeOnly TimeNow = TimeOnly.FromDateTime(DateTime.Now);
                string formattedTime = TimeNow.ToString("hh:mm tt");


                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                var existingAttendance = await context.Attendences
                    .Where(a => a.ApplicationUser == student && a.Date_Day == today).FirstOrDefaultAsync();

                if (existingAttendance != null)
                {
                    if (existingAttendance.Total == 2)
                    {
                        return BadRequest(new { status = 400, errorMessage = "Student is already attended twice today." });
                    }
                    else if (existingAttendance.PartTwo != "Not Present")
                    {
                        return BadRequest(new { status = 400, errorMessage = "Student is already attended in Part Two today." });
                    }
                    else if (existingAttendance.PartOne != "Not Present" && existingAttendance.Total ==1)
                    {
                        return BadRequest(new { status = 400, errorMessage = "Student is already attended in Part One today." });
                    }
                }
                
                if (now.Hour >= 0 && now.Hour < 12)
                {
                    var attendance = new Attendence
                    {
                        Date_Day = today,
                        Total = 1,
                        PartOne = formattedTime,
                        PartTwo = "Not Present",
                        ApplicationUser = student,
                    };
                    context.Attendences.Add(attendance);
                    await context.SaveChangesAsync();

                    return Ok(new { status = 200, message = "Attendance added successfully in Part One today." });
                }
                else if (now.Hour >= 12 && now.Hour < 24)
                {
                    if (existingAttendance != null)
                    {
                        existingAttendance.Total = 2;
                        existingAttendance.PartTwo = formattedTime;
                        context.Attendences.Update(existingAttendance);
                        await context.SaveChangesAsync();

                        return Ok(new { status = 200, message = "Attendance added successfully in Part Two today." });
                    }
                    else
                    {
                        var attendance = new Attendence
                        {
                            Date_Day = today,
                            Total = 1,
                            PartOne = "Not Present",
                            PartTwo = formattedTime,
                            ApplicationUser = student,
                        };
                        context.Attendences.Add(attendance);
                        await context.SaveChangesAsync();

                        return Ok(new { status = 200, message = "Attendance added successfully in Part Two today." });
                    }
                }
                else
                {
                    return BadRequest(new { status = 400, errorMessage = "Attendance starts from 8 AM and ends at 4 PM." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, errorMessage = "Failed to add attendance." });
            }
        }


        [HttpPost("AddBehavior")]
        public async Task<IActionResult> AddBehavior([FromBody] AddBehaviorDto addBehaviorDto)
        {
            try
            {
                var behavior = new Behavior
                {
                    Behavior_Value = addBehaviorDto.Behavior_Value,
                    Time = DateTime.Now,
                };

                context.Behaviors.Add(behavior);
                await context.SaveChangesAsync();

                return Ok(new { status = 200, message = "behavior added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, errorMessage = "Failed to add behavior." });
            }
        }

        
        [HttpGet("getallAttendances")]
        public async Task<IActionResult> GetAllAttendances()
        {
            var AllAttendances = await context.Attendences
                .OrderBy(y => y.Date_Day)
                .Include(x => x.ApplicationUser)
                .Select(s => new { s.Id, s.PartOne, s.PartTwo, s.Total, s.Date_Day, s.ApplicationUser.Name })
                .ToListAsync();

            return Ok(AllAttendances);
        }


        [HttpGet("getallBehaiviors")]
        public async Task<IActionResult> GetAllBehaviors()
        {
            var AllBehaiviors = await context.Behaviors
                .Select(s => new { s.Id, s.Time, s.Behavior_Value })
                .ToListAsync();

            return Ok(AllBehaiviors);
        }


        [HttpDelete("deleteAttendance/{attendanceId}")]
        public async Task<IActionResult> DeleteYear(int attendanceId)
        {
            var ToDelete = await context.Attendences.FindAsync(attendanceId);
            if (ToDelete == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Attendance with ID '{attendanceId}' not found."
                });
            }

            context.Attendences.Remove(ToDelete);
            await context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Attendance deleted successfully."
            });
        }
        
        
        [HttpDelete("DeleteBehavior/{BehaviorsId}")]
        public async Task<IActionResult> DeleteBehavior(int BehaviorId)
        {
            var ToDelete = await context.Behaviors.FindAsync(BehaviorId);
            if (ToDelete == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Behavior with ID '{BehaviorId}' not found."
                });
            }

            context.Behaviors.Remove(ToDelete);
            await context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Behavior deleted successfully."
            });
        }
    }
}