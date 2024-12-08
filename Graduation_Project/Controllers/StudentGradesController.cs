using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Graduation_Project.Services;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using System.Diagnostics;

namespace Graduation_Project.Controllers
{
    [Authorize(Roles = "Manager,Teacher,Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentGradesController : ControllerBase
    {
        IWebHostEnvironment webHostEnvironment;
        private readonly ApplicationDbContext _context;
        //private readonly TermTransferService _termTransferService;

        public StudentGradesController(ApplicationDbContext context/*, TermTransferService termTransferService*/ , IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
            //_termTransferService = termTransferService;
        }

        [AllowAnonymous]
        [HttpGet("StudentGradeById/{Id}")]
        public async Task<IActionResult> StudentGradeById(int Id)
        {
            var pClass = await _context.StudentGrades.FindAsync(Id);

            if (pClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"StudentGrades with ID {Id} not found."
                });
            }

            var ClassInfo = new
            {
                StudentGrade = pClass.Student_Grade
            };

            return Ok(ClassInfo);
        }

        [HttpGet("getstudentgrades/{studentId}/{termId}")]
        public async Task<IActionResult> GetStudentGrades(string studentId, int termId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Student with ID {studentId} not found."
                });
            }

            var studentGrades = _context.StudentGrades
                .Include(sg => sg.Exam)
                .ThenInclude(ex => ex.Material)
                .Where(sg => sg.ApplicationUser.Id == studentId && sg.Exam.Material.Term.Id == termId)
                .GroupBy(sg => new { sg.Exam.Material.Id, sg.Exam.Material.Name })
                .Select(group => new
                {
                    MaterialId = group.Key.Id,
                    MaterialName = group.Key.Name,
                    StudentTotalGrade = group.Sum(g => g.Student_Grade),
                    Exams = group.Select(sg => new
                    {
                        ExamId = sg.Exam.Id,
                        ExamName = sg.Exam.Name,
                        ExamGrade = sg.Exam.Exam_Grade,
                        StudentExamGrade = sg.Student_Grade
                    })
                }).ToList();


            return Ok(studentGrades);
        }

        [HttpPost("addgrades/{StudentId}/{ExamId}/{TeacherId}")]
        public async Task<IActionResult> AddGrades([FromForm] AddStudentGradeDto studentGradesDtos, int ExamId, string StudentId, string TeacherId)
        {

            if (studentGradesDtos == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid student grades data."
                });
            }

            var exam = await _context.Exams.FindAsync(ExamId);
            if (exam == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Invalid exam ID: {ExamId}."
                });
            }

            var user = await _context.Users.FindAsync(StudentId);
            if (user == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Invalid Student ID: {StudentId}."
                });
            }

            var materialId = await _context.Exams
                .Where(e => e.Id == ExamId)
                .Select(e => e.Material.Id)
                .FirstOrDefaultAsync();

            //var isTeacherForMaterial = await _context.ApplicationUserMaterial
            //    .AnyAsync(um => um.UserId == TeacherId && um.MaterialId == materialId);

            var teacher = await _context.Users.FindAsync(TeacherId);
            var Materiall = await _context.Materials.FindAsync(materialId);

            if (teacher.Subject != Materiall.Name)
            {
                return Unauthorized(new
                {
                    status = 401,
                    errorMessage = "You are not authorized to add grades for this material."
                });
            }

            var PclassId = await _context.Users 
               .Where(i => i.Id == StudentId)
               .Select(e => e.PClass.Id)
               .FirstOrDefaultAsync();

            var isTeacherForClass = await _context.AppLicationUserPClasses
                .AnyAsync(um => um.UserId == TeacherId && um.PClassId == PclassId);

            if (!isTeacherForClass)
            {
                return Unauthorized(new
                {
                    status = 401,
                    errorMessage = "You are not authorized to add grades for this Student."
                });
            }

            //foreach (var studentGradeDto in studentGradesDtos)
            //{
                var studentGradeDto = studentGradesDtos;
                if (studentGradeDto.Student_Grade < 0)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "Student grade must be positive."
                    });
                }
                if (studentGradeDto.Student_Grade > exam.Exam_Grade)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "Student grade must be not bigger than exam grade."
                    });
                }

                var existingStudentGrade = await _context.StudentGrades
                    .FirstOrDefaultAsync(s => s.Exam.Id == ExamId && s.ApplicationUser.Id == StudentId);

                if (existingStudentGrade != null)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = $"Grade for this Exam already exists for this student"
                    });
                }
                else
                {

                    var newStudentGrade = new StudentGrade
                    {
                        Student_Grade = studentGradeDto.Student_Grade,
                        Exam = exam,
                        ApplicationUser = user
                    };
                    //if (studentGradeDto.Image != null)
                    //{
                    //    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    //    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "Exams");

                    //    if (!allowedExtensions.Contains(Path.GetExtension(studentGradeDto.Image.FileName).ToLower()))
                    //    {
                    //        return BadRequest(new
                    //        {
                    //            status = 400,
                    //            errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
                    //        });
                    //    }

                    //    string uniqueFileName = Guid.NewGuid().ToString() + "_" + studentGradeDto.Image.FileName;
                    //    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    //    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    //    {
                    //        await studentGradeDto.Image.CopyToAsync(fileStream);
                    //    }
                    //    newStudentGrade.Image = uniqueFileName;
                    //}

                    _context.StudentGrades.Add(newStudentGrade);
                }
            //}
            ////_termTransferService.TransferAllStudentsToNextTerm();

            //foreach (var studentGradeDto in studentGradesDtos)
            //{
            //    var user11 = await _context.Users.FindAsync(StudentId);
            //    if (user11 != null)
            //    {
            //        bool transferToNextTerm = await _termTransferService.TransferStudentToNextTerm(user11);
            //        //var termTransferService = new TermTransferService(_context);
            //        //await termTransferService.TransferStudentToNextTerm(user11);
            //    }
            //}
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Student grades added successfully."
            });
        }

        [HttpPut("updategrades/{studentGradeId}/{ExamId}/{StudentId}/{TeacherId}")]
        public async Task<IActionResult> UpdateGrades(int studentGradeId, string StudentId, int ExamId, string TeacherId, [FromForm] StudentGradeDto studentGradeDto)
        {
            var existingStudentGrade = await _context.StudentGrades.FindAsync(studentGradeId);
            //string TeacherId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (existingStudentGrade == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Student grade with ID {studentGradeId} not found."
                });
            }

            var exam = await _context.Exams.FindAsync(ExamId);
            if (exam == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Invalid exam ID: {ExamId}."
                });
            }

            var user = await _context.Users.FindAsync(StudentId);
            if (user == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Invalid Student ID: {StudentId}."
                });
            }

            var materialId = await _context.Exams
                .Where(e => e.Id == ExamId)
                .Select(e => e.Material.Id)
                .FirstOrDefaultAsync();

            //var isTeacherForMaterial = await _context.ApplicationUserMaterial
            //    .AnyAsync(um => um.UserId == TeacherId && um.MaterialId == materialId);

            var teacher = await _context.Users.FindAsync(TeacherId);
            var Materiall = await _context.Materials.FindAsync(materialId);

            if (teacher.Subject != Materiall.Name)
            {
                return Unauthorized(new
                {
                    status = 401,
                    errorMessage = "You are not authorized to add grades for this material."
                });
            }

            var PclassId = await _context.Users
               .Where(i => i.Id == StudentId)
               .Select(e => e.PClass.Id)
               .FirstOrDefaultAsync();

            var isTeacherForClass = await _context.AppLicationUserPClasses
                .AnyAsync(um => um.UserId == TeacherId && um.PClassId == PclassId);

            if (!isTeacherForClass)
            {
                return Unauthorized(new
                {
                    status = 401,
                    errorMessage = "You are not authorized to add grades for this Student."
                });
            }

            if (studentGradeDto.Student_Grade < 0)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Student grade must be positive."
                });
            }
            if (studentGradeDto.Student_Grade > exam.Exam_Grade)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Student grade must be not bigger than exam grade."
                });
            }

            existingStudentGrade.Student_Grade = studentGradeDto.Student_Grade;
            existingStudentGrade.Exam = exam;
            existingStudentGrade.ApplicationUser = user;

            //string oldImageFileName = existingStudentGrade.Image;

            //if (studentGradeDto.Image != null)
            //{
            //    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
            //    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "Exams");

            //    if (!allowedExtensions.Contains(Path.GetExtension(studentGradeDto.Image.FileName).ToLower()))
            //    {
            //        return BadRequest(new
            //        {
            //            status = 400,
            //            errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
            //        });
            //    }

            //    string uniqueFileName = Guid.NewGuid().ToString() + "_" + studentGradeDto.Image.FileName;
            //    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //    await using (var fileStream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await studentGradeDto.Image.CopyToAsync(fileStream);
            //    }

            //    if (!string.IsNullOrEmpty(oldImageFileName))
            //    {
            //        string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
            //        if (System.IO.File.Exists(oldFilePath))
            //        {
            //            System.IO.File.Delete(oldFilePath);
            //        }
            //    }
            //    existingStudentGrade.Image = uniqueFileName;
            //}
            //else
            //{
            //    existingStudentGrade.Image = oldImageFileName;
            //}

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Student grade updated successfully."
            });
        }

        [HttpDelete("deletegrades/{studentGradeId}")]
        public async Task<IActionResult> DeleteGrades(int studentGradeId)
        {
            var existingStudentGrade = await _context.StudentGrades.FindAsync(studentGradeId);

            if (existingStudentGrade == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Student grade with ID {studentGradeId} not found."
                });
            }

            _context.StudentGrades.Remove(existingStudentGrade);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Student grade deleted successfully."
            });
        }
    }
}
