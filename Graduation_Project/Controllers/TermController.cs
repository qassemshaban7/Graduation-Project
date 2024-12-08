using DocumentFormat.OpenXml.Bibliography;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Graduation_Project.Controllers
{
    [Authorize(Roles = "Manager,Teacher,Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class TermController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TermController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("getallSemester")]
        public async Task<IActionResult> GetAllSemester()
        {
            var allSemester = await _context.Terms
                .OrderBy(y => y.Index)
                .Select(semester => new { semester.Id, semester.TermName })
                .ToListAsync();

            return Ok(allSemester);
        }

        [AllowAnonymous]
        [HttpGet("getSemesterbyYearID/{yearId}")]
        public async Task<IActionResult> GetTermsByYear(int yearId)
        {
            var year = await  _context.Years.FindAsync(yearId);

            if (year == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Year with ID {yearId} not found."
                });
            }

            var termsByYear = await _context.Terms
                .Where(term => term.YearId == yearId)
                .OrderBy(term => term.Index)
                .Select(term => new { term.Id, term.TermName }) 
                .ToListAsync();

            return Ok(termsByYear);
        }

        //[AllowAnonymous]
        //[HttpGet("getSemesterByStudentId/{StudentId}/{YearId}")]
        //public async Task<IActionResult> getClassesByTeacherId(string StudentId, int YearId)
        //{
        //    var Teacher = await _context.Users.FindAsync(StudentId);

        //    if (Teacher == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Student with ID {StudentId} not found."
        //        });
        //    }

        //    var ClassesByTeacherId = await _context.Years
        //        .Include(y => y.Terms)
        //        .Where(x => x.Id == YearId)
        //        .OrderBy(term => term.Name)
        //        .Select(term => new { term.Terms.TermId, term.Terms.TermName })
        //        .ToListAsync();

        //    return Ok(ClassesByTeacherId);
        //}

        //[HttpPost("addterm/{YearId}")]
        //public async Task<IActionResult> AddTerm([FromBody] TermDto termDto, int YearId)
        //{
        //    if (termDto == null)
        //    {
        //        return BadRequest("Invalid term data.");
        //    }

        //    var year = await _context.Years.FindAsync(YearId);
        //    if (year == null)
        //    {
        //        return NotFound($"Year with ID '{YearId}' not found.");
        //    }

        //    if (_context.Terms.Any(t => t.Index == termDto.Index))
        //    {
        //        return BadRequest("Index value already exists.");
        //    }

        //    if (_context.Terms.Any(t => t.TermName == termDto.TermName && t.Year.Id == YearId))
        //    {
        //        return BadRequest($"Term with name '{termDto.TermName}' already exists for this year.");
        //    }

        //    var term = new Term
        //    {
        //        Index = termDto.Index,
        //        TermName = termDto.TermName,
        //        EndDate = termDto.EndDate.Date,
        //        Year = year
        //    };
        //    _context.Terms.Add(term);
        //    await _context.SaveChangesAsync();

        //    return Ok("Term added successfully.");
        //}

        //[HttpPut("editterm/{termid}/{YearId}")]
        //public async Task<IActionResult> EditTerm(int termid, [FromBody] TermDto termDto, int YearId)
        //{
        //    if (termDto == null)
        //    {
        //        return BadRequest("Invalid term data.");
        //    }

        //    var existingTerm = await _context.Terms.FindAsync(termid);
        //    if (existingTerm == null)
        //    {
        //        return NotFound($"Term with ID '{termid}' not found.");
        //    }

        //    var existingyear = await _context.Years.FindAsync(YearId);
        //    if (existingyear == null)
        //    {
        //        return NotFound($"Year with ID '{YearId}' not found.");
        //    }

        //    if (_context.Terms.Any(t => t.Index == termDto.Index && t.Id != termid))
        //    {
        //        return BadRequest($"Index value '{termDto.Index}' already exists for another term.");
        //    }

        //    if (_context.Terms.Any(t => t.TermName == termDto.TermName && t.Year.Id == YearId && t.Id != termid))
        //    {
        //        return BadRequest($"Term with name '{termDto.TermName}' already exists for this year.");
        //    }

        //    existingTerm.Index = termDto.Index;
        //    existingTerm.TermName = termDto.TermName;
        //    existingTerm.EndDate = termDto.EndDate.Date;
        //    existingTerm.Year = existingyear;

        //    await _context.SaveChangesAsync();

        //    return Ok("Term updated successfully.");
        //}

        //[HttpDelete("deleteterm/{termid}")]
        //public async Task<IActionResult> DeleteTerm(int termid)
        //{
        //    var termToDelete = await _context.Terms.FindAsync(termid);
        //    if (termToDelete == null)
        //    {
        //        return NotFound($"Term with ID '{termid}' not found.");
        //    }

        //    _context.Terms.Remove(termToDelete);

        //    await _context.SaveChangesAsync();

        //    return Ok("Term deleted successfully.");
        //}
    }
}