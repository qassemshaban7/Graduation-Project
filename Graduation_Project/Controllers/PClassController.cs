using DocumentFormat.OpenXml.Bibliography;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
    [Authorize(Roles = "Manager,Teacher,Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class PClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PClassController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("getallclasses")]
        public async Task<IActionResult> GetAllClasses()
        {
            var allClasses = await _context.PClasses
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    ClassId = p.Id,
                    ClassName = p.Name
                })
                .ToListAsync();

            return Ok(allClasses);
        }

        [AllowAnonymous]
        [HttpGet("getClassById/{ClassId}")]
        public async Task<IActionResult> GetClassById(int ClassId)
        {
            var pClass = await _context.PClasses.FindAsync(ClassId);

            if (pClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Class with ID {ClassId} not found."
                });
            }

            var ClassInfo = new
            {
                Id = pClass.Id,
                name = pClass.Name,
            };

            return Ok(ClassInfo);
        }

        [AllowAnonymous]
        [HttpGet("getClassesByTeacherId/{TeacherId}")]
        public async Task<IActionResult> getClassesByTeacherId(string TeacherId)
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

            var ClassesByTeacherId = await _context.AppLicationUserPClasses
                .Include(y => y.PClass)
                .Where(x => x.UserId == Teacher.Id)
                .OrderBy(term => term.PClass.Name)
                .Select(term => new { term.PClass.Id, term.PClass.Name })
                .ToListAsync();

            return Ok(ClassesByTeacherId);
        }

        [HttpPost]
        public IActionResult AddClass( PClassDto pClass)
        {
            if (pClass == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid class data"
                });
            }
            var pc = new PClass
            { 
                Name = pClass.Name,
            };

            if (_context.PClasses.Any(c => c.Name == pClass.Name))
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Class with name {pClass.Name} already exists"
                });
            }
            _context.PClasses.Add(pc);
            _context.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Class added successfully"
            });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClass(int id, [FromBody] PClassDto updatedClass)
        {
            var existingClass = _context.PClasses.Find(id);

            if (existingClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = "Class not found"
                });
            }
            if (_context.PClasses.Any(c => c.Name == updatedClass.Name &&c.Id != id))
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Class with name {updatedClass.Name} already exists"
                });
            }

            existingClass.Name = updatedClass.Name;
            _context.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Class updated successfully"
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClass(int id)
        {
            var existingClass = _context.PClasses.Find(id);

            if (existingClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Class with ID {id} not found."
                });
            }

            _context.PClasses.Remove(existingClass);
            _context.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Class deleted successfully"
            });
        }
    }
}
