using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
    [Authorize(Roles = "Manager,Teacher,Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        IWebHostEnvironment webHostEnvironment;

        public ExamController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }


        [AllowAnonymous]
        [HttpGet("getExamsforMaterial/{MaterialId}")]
        public IActionResult GetExamsforMaterial(int MaterialId)
        {
            var exams = _context.Exams
                .Where(m => m.MaterialId == MaterialId)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Exam_Grade,
                    ImageUrl = $"http://ablexav1.runasp.net/GeneralExams/{m.Image}"
                })
                .ToList();

            return Ok(exams);
        }


        [AllowAnonymous]
        [HttpGet("getExamByIdForTeacher/{ExamId}")]
        public async Task<IActionResult> GetExamByIdForTeacher(int ExamId)
        {
            var pClass = await _context.Exams.FindAsync(ExamId);

            if (pClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Exam with ID {ExamId} not found."
                });
            }

            var ClassInfo = new
            {
                Id = pClass.Id,
                name = pClass.Name,
                Grade = pClass.Exam_Grade,
                ExamImage = $"http://ablexav1.runasp.net/GeneralExams//{pClass.Image}",
            };

            return Ok(ClassInfo);
        }

        [AllowAnonymous]
        [HttpGet("getExamById/{ExamId}")]
        public async Task<IActionResult> GetExamById(int ExamId)
        {
            var pClass = await _context.Exams.FindAsync(ExamId);

            if (pClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Exam with ID {ExamId} not found."
                });
            }

            var ClassInfo = new
            {
                Id = pClass.Id,
                name = pClass.Name,
                Grade = pClass.Exam_Grade,
                //ExamImage = $"http://ablexav1.runasp.net/GeneralExams//{pClass.Image}",
            };

            return Ok(ClassInfo);
        }

        [HttpPost("addexams")]
        public async Task<IActionResult> AddExams([FromForm] ExamDto examDtos, string TeacherId)
        {
            if (examDtos == null )
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid exam data."
                });
            }

            var materialId = examDtos.MaterialId;
            if (materialId == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Material ID is required."
                });
            }

            var material = await _context.Materials.FindAsync(materialId);
            if (material == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid material ID."
                });
            }

            var teacher = await _context.Users.FindAsync(TeacherId);

            if (teacher.Subject != material.Name)
            {
                return Unauthorized(new
                {
                    status = 401,
                    errorMessage = "You are not authorized to add grades for this material."
                });
            }

            //double totalExamGrades = 0;

            var examDto = examDtos;            
                if (_context.Exams.Any(e => e.Name == examDto.Name && e.Material.Id == material.Id))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = $"An exam with name '{examDto.Name}' already exists for this material."
                    });
                }

                ////totalExamGrades += examDto.Exam_Grade;
                var totalExamGradesInMaterial = _context.Exams.Where(e => e.Material.Id == material.Id).Sum(e => e.Exam_Grade);

                if (totalExamGradesInMaterial + examDto.Exam_Grade > material.M_grade)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "Total exam grades exceed material grade."
                    });
                }

                var exam = new Exam
                {
                    Name = examDto.Name,
                    Exam_Grade = examDto.Exam_Grade,
                    Material = material
                };
                if (examDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "GeneralExams");

                    if (!allowedExtensions.Contains(Path.GetExtension(examDto.Image.FileName).ToLower()))
                    {
                        return BadRequest(new
                        {
                            status = 400,
                            errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
                        });
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + examDto.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await examDto.Image.CopyToAsync(fileStream);
                    }
                    exam.Image = uniqueFileName;
                }

                _context.Exams.Add(exam);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Exams added successfully."
            });
        }

        [HttpPut("{examId}")]
        public async Task<IActionResult> UpdateExam(int examId, string TeacherId, [FromForm] EditExamDto examDto)
        {
            if (examDto == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid exam data."
                });
            }

            var existingExam = await _context.Exams.FindAsync(examId);
            if (existingExam == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Exam ID not found '{examId}'."
                });
            }

            var material = await _context.Materials.FindAsync(examDto.MaterialId);
            if (material == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid material ID."
                });
            }

            var teacher = await _context.Users.FindAsync(TeacherId);

            if (teacher.Subject != material.Name)
            {
                return Unauthorized(new
                {
                    status = 401,
                    errorMessage = "You are not authorized to add grades for this material."
                });
            }

            if (_context.Exams.Any(e => e.Name == examDto.Name && e.Material.Id == material.Id && e.Id != examId))
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"An exam with name '{examDto.Name}' already exists for this material."
                });
            }

            var totalExamGradesInMaterial = _context.Exams.Where(e => e.Material.Id == material.Id && e.Id != examId).Sum(e => e.Exam_Grade);

            if (totalExamGradesInMaterial + examDto.Exam_Grade > material.M_grade)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Total exam grades exceed material grade."
                });
            }

            existingExam.Name = examDto.Name;
            existingExam.Exam_Grade = examDto.Exam_Grade;
            existingExam.Material = material;

            string oldImageFileName = existingExam.Image;

            if (examDto.Image != null)
            {
                string[] allowedExtensions = { ".png", ".jpg" , ".jpeg" };
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "GeneralExams");

                if (!allowedExtensions.Contains(Path.GetExtension(examDto.Image.FileName).ToLower()))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "Only .png and .jpg and .jpeg images are allowed!"
                    });
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + examDto.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await examDto.Image.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(oldImageFileName))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                existingExam.Image = uniqueFileName;
            }
            else
            {
                existingExam.Image = oldImageFileName;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Exams updated successfully."
            });
        }


        [HttpDelete("deletexams/{id}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Exam with ID '{id}' not found."
                });
            }

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Exam deleted successfully."
            });
        }

        [AllowAnonymous]
        [HttpPost("AddExamImage")]
        public async Task<IActionResult> AddExamImage(IFormFile Image)  
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "GeneralExams");

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
    }
}
