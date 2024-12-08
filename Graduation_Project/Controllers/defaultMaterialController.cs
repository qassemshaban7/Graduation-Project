using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project.Controllers
{
    [Authorize(Roles = "Manager,Teacher,Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class defaultMaterialController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public defaultMaterialController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("getallmaterials")]
        public IActionResult GetAllMaterials()
        {
            var allClasses = _context.DefaultMateriales
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    Subject_Name = p.Name
                })
                .ToList();

            return Ok(allClasses);
        }

        [AllowAnonymous]
        [HttpGet("getallmaterialsNamesAndIds")]
        public IActionResult GetAllMaterialsNamesAndIds()
        {
            var allClasses = _context.DefaultMateriales
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    Id = p.Id,
                    Subject_Name = p.Name
                })
                .ToList();

            return Ok(allClasses);
        }

        [HttpPost]
        public IActionResult AddSubject(DefaltSubjectDto pClass)
        {
            if (pClass == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid Subject data"
                });
            }
            var pc = new DefaultMaterial
            {
                Name = pClass.Name,
            };

            if (_context.DefaultMateriales.Any(c => c.Name == pClass.Name))
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Subject with name {pClass.Name} already exists"
                });
            }
            _context.DefaultMateriales.Add(pc);
            _context.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Subject added successfully"
            });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSubject(int id, [FromBody] DefaltSubjectDto updatedClass)
        {
            var existingClass = _context.DefaultMateriales.Find(id);

            if (existingClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = "Subject not found"
                });
            }
            if (_context.DefaultMateriales.Any(c => c.Name == updatedClass.Name && c.Id != id))
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = $"Subject with name {updatedClass.Name} already exists"
                });
            }

            existingClass.Name = updatedClass.Name;
            _context.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Subject updated successfully"
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSubject(int id)
        {
            var existingClass = _context.DefaultMateriales.Find(id);

            if (existingClass == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Subject with ID {id} not found."
                });
            }

            _context.DefaultMateriales.Remove(existingClass);
            _context.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Subject deleted successfully"
            });
        }
    }
}
