using DocumentFormat.OpenXml.InkML;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace Graduation_Project.Controllers
{
    [Authorize(Roles = "Manager,Teacher,Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialController(ApplicationDbContext context)
        {
            _context = context;
        }
        [AllowAnonymous]
        [HttpGet("GetMaterialsByTeacherId/{TeacherId}/{TermId}")]
        public async Task<IActionResult> GetMaterialsByTeacherId(string TeacherId, int TermId)
        {
            var Teacher = await _context.Users.FindAsync(TeacherId);

            if (Teacher == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Teacher with ID {TeacherId} not found."
                });
            }

            var materials = await _context.Materials
                .Where(m => m.TermId == TermId && m.Name == Teacher.Subject)
                .Select(m => new
                {
                    MaterialId = m.Id,
                    MaterialName = m.Name,
                    MaterialGrade = m.M_grade
                })
                .ToListAsync();

            return Ok(materials);
        }



        [AllowAnonymous]
        [HttpGet("GetMaterialsByTeacherIdTwo/{TeacherId}")]
        public async Task<IActionResult> GetMaterialsByTeacherIdTwo(string TeacherId)
        {
            var Teacher = await _context.Users.FindAsync(TeacherId);

            if (Teacher == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Teacher with ID {TeacherId} not found."
                });
            }

            var years = _context.Years
                .Where(y => y.Id != 1)
                .Include(y => y.Terms)
                .ThenInclude(t => t.Materials)
                .ToList();

            var result = years.Select(y => new
            {
                YearId = y.Id,
                YearName = y.Name,
                Terms = y.Terms.Select(t => new
                {
                    TermId = t.Id,
                    TermName = t.TermName,
                    Materials = t.Materials.Where(m => m.Name == Teacher.Subject).Select(m => new
                    {
                        MaterialId = m.Id,
                        MaterialName = m.Name,
                        MaterialGrade = m.M_grade
                    }).ToList()
                }).ToList()
            }).ToList();

            return Ok(result);
        }


        [AllowAnonymous]
        [HttpGet("getAllMaterials/{termId}")]
        public IActionResult GetAllMaterialsByTermId(int termId)
        {
            var materials = _context.Materials
                .Where(m => m.TermId == termId)
                .Select(m => new { m.Id, m.Name })
                .ToList();

            return Ok(materials);
        }

        [AllowAnonymous]
        [HttpGet("getmaterials/{termId}")]
        public IActionResult GetMaterialsByTermId(int termId)
        {
            var materials = _context.Materials
                .Where(m => m.TermId == termId)
                .Select(m => new { m.Id, m.Name, m.M_grade })
                .ToList();

            return Ok(materials);
        }

        [HttpGet("getstudentmaterialgrade/{studentId}/{termId}")]
        public IActionResult GetStudentMaterialGrade(string studentId, int termId)
        {
            var totalMaterialGrades = _context.Exams
                .Join(
                    _context.StudentGrades,
                    exam => exam.Id,
                    grade => grade.Exam.Id,
                    (exam, grade) => new { Exam = exam, Grade = grade }
                )
                .Where(j => j.Grade.ApplicationUser.Id == studentId && j.Exam.Material.TermId == termId)
                .GroupBy(j => j.Exam.Material.Id)
                .Select(group => new
                {
                    MaterialId = group.Key,
                    MaterialName = group.First().Exam.Material.Name,
                    MaterialGrade = group.First().Exam.Material.M_grade,
                    StudentTotalGrade = group.Sum(g => g.Grade.Student_Grade)   
                })
                .ToList();

            return Ok(totalMaterialGrades);
        }

        [HttpGet("getStudentGradesDetailsForOneMaterial/{studentId}/{materialId}")]
        public IActionResult GetStudentGradesDetails(string studentId, int materialId)
        {
            var examGradesDetails = _context.Exams
                .Join(
                    _context.StudentGrades,
                    exam => exam.Id,
                    grade => grade.Exam.Id,
                    (exam, grade) => new { Exam = exam, Grade = grade }
                )
                .Where(j => j.Grade.ApplicationUser.Id == studentId && j.Exam.Material.Id == materialId)
                .GroupBy(j => new { j.Exam.Material.Name, j.Exam.Material.M_grade })
                .Select(group => new
                {
                    MaterialName = group.Key.Name,
                    MaterialGrade = group.Key.M_grade,
                    StudentTotalGrade = group.Sum(g => g.Grade.Student_Grade),
                    Exams = group.Select(j => new
                    {
                        ExamId = j.Exam.Id,
                        ExamName = j.Exam.Name,
                        ExamGrade = j.Exam.Exam_Grade,
                        GeneralExamImage = $"http://ablexav1.runasp.net/GeneralExams//{j.Exam.Image}",
                        StudentExamGrade = j.Grade.Student_Grade
                    })
                })
                .ToList();

            return Ok(examGradesDetails);
        }



        [HttpPost("addMaterialGrade/{materialid}")]
        public async Task<IActionResult> AddMaterialGrade(int materialid,[FromForm]MaterialDto materialDto)
        {
            if (materialDto == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid material data."
                });
            }

            var existingMaterial = await _context.Materials.FindAsync(materialid);
            if (existingMaterial == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Material with ID '{materialid}' not found."
                });
            }

            if (materialDto.M_grade <= 0)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Material Grade Muste Be Positive Number"
                });
            }

            existingMaterial.M_grade = materialDto.M_grade;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Material_Grade added successfully."
            });
        }

        [HttpPut("updateMaterialGrade/{materialid}")]
        public async Task<IActionResult> UpdateMaterialGrade(int materialid, [FromForm]MaterialDto materialDto)
        {
            if (materialDto == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid material data."
                });
            }

            var existingMaterial = await _context.Materials.FindAsync(materialid);
            if (existingMaterial == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Material with ID '{materialid}' not found."
                });
            }

            if (materialDto.M_grade <= 0)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Material Grade Muste Be Positive Number"
                });
            }

            existingMaterial.M_grade = materialDto.M_grade;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Material_Grade updated successfully."
            });
        }
        //[HttpDelete("deletematerial/{materialid}")]
        //public async Task<IActionResult> DeleteMaterial(int materialid)
        //{
        //    var material = await _context.Materials.FindAsync(materialid);
        //    if (material == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Material with ID '{materialid}' not found."
        //        });
        //    }

        //    _context.Materials.Remove(material);
        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Material deleted successfully."
        //    });
        //}
    }
}
