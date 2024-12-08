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
    public class YearController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public YearController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("getYearById/{yearId}")]
        public IActionResult GetYearById(int yearId)
        {
            var year = _context.Years
                .Include(y => y.Terms)
                .ThenInclude(t => t.Materials)
                .FirstOrDefault(y => y.Id == yearId);

            if (year == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Year with ID {yearId} not found."
                });
            }

            var yearInfo = new
            {
                index = year.Index,
                name = year.Name,
                Terms = year.Terms.OrderBy(x => x.Index).Select(t => new
                {
                    Materials = t.Materials.Select(m => new
                    {
                        name = m.Name,
                    })
                })
            };

            return Ok(yearInfo);
        }

        [AllowAnonymous]
        [HttpGet("getallyears")]
        public IActionResult GetAllYears()
        {
            var allYears = _context.Years
                .OrderBy(y => y.Index)
                .Select(year => new { year.Id, year.Name })
                .ToList();

            return Ok(allYears);
        }

        //[HttpPost("addyear")]
        //public async Task<IActionResult> AddYear([FromBody] YearDto yearDto)
        //{
        //    if (yearDto == null)
        //    {
        //        return BadRequest("Invalid Year data.");
        //    }

        //    if (_context.Years.Any(t => t.Index == yearDto.Index))
        //    {
        //        return BadRequest("Index value already exists.");
        //    }

        //    var Newye = new Year
        //    {
        //        Index = yearDto.Index,
        //        Name = yearDto.YearName
        //    };
        //    _context.Years.Add(Newye);
        //    await _context.SaveChangesAsync();

        //    return Ok("Year added successfully.");
        //}

        [HttpPost("addyear")]
        public async Task<IActionResult> AddYear([FromForm] YearMaterialDto yearMaterialDto)
        {
            if (ModelState.IsValid)
            {

                if (yearMaterialDto == null)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "Invalid Year or Material data."
                    });
                }

                if (yearMaterialDto.FirstSemesterMaterial == null)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "At least one Material in Semester."
                    });
                }

                if (yearMaterialDto.SecondSemesterMaterial == null)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "At least one Material in Semester."
                    });
                }

                var thisYear = DateTime.Now.Year;
                if (_context.Years.Any(t => t.Name == $"{yearMaterialDto.YearName}_{thisYear}"))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = $"This Name: {yearMaterialDto.YearName} already exists."
                    });
                }

                if (_context.Years.Any(t => t.Index == yearMaterialDto.Index))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = "Index value already exists."
                    });
                }
               
                var newYear = new Year
                {
                    Index = yearMaterialDto.Index,
                    Name = $"{yearMaterialDto.YearName}_{thisYear}"
                };
                _context.Years.Add(newYear);
                await _context.SaveChangesAsync();

                var termOne = new Term
                {
                    TermName = $"First_Semester",
                    EndDate = new DateTime(thisYear, 2, 10),
                    Index = (newYear.Index * 2) - 1,
                    YearId = newYear.Id
                };
                
                var termTwo = new Term
                {
                    TermName = $"Second_Semester",
                    EndDate = new DateTime(thisYear, 8, 10),
                    Index = newYear.Index * 2,
                    YearId = newYear.Id
                };
                _context.Terms.Add(termOne);
                _context.Terms.Add(termTwo);
                await _context.SaveChangesAsync();

                foreach (var material in yearMaterialDto.FirstSemesterMaterial)
                {
                    var newMaterial1 = new Material
                    {
                        Name = material,
                        M_grade = 0,
                        TermId = termOne.Id
                    };

                    _context.Materials.Add(newMaterial1);
                }

                foreach (var material in yearMaterialDto.SecondSemesterMaterial)
                {
                    var newMaterial2 = new Material
                    {
                        Name = material,
                        M_grade = 0,
                        TermId = termTwo.Id
                    };

                    _context.Materials.Add(newMaterial2);
                }

                await _context.SaveChangesAsync();
                
                return Ok(new
                {
                    status = 200,
                    message = "Year added successfully."
                });

            }
            else
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid ModelState"
                });
            }
        }

        [HttpPut("edityear/{yearId}")]
        public async Task<IActionResult> EditYear(int yearId, [FromForm] EditYearAndMaterialDto yearMaterialDto)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Years.FindAsync(yearId);
                if (existing == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        errorMessage = $"Year with ID '{yearId}' not found."
                    });
                }

                if (_context.Years.Any(t => t.Name == yearMaterialDto.YearName && t.Id != yearId))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = $"This Name: {yearMaterialDto.YearName} already exists."
                    });
                }

                if (_context.Years.Any(t => t.Index == yearMaterialDto.Index && t.Id != yearId))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        errorMessage = $"Index value '{yearMaterialDto.Index}' already exists for another year."
                    });
                }

                var thisYear = DateTime.Now.Year;
                existing.Index = yearMaterialDto.Index;
                existing.Name = $"{yearMaterialDto.YearName}_{ thisYear}";

                var termOne = await _context.Terms.OrderBy(x => x.Index).Where(x => x.YearId == yearId).FirstOrDefaultAsync();
                var termTwo = await _context.Terms.OrderByDescending(x => x.Index).Where(x => x.YearId == yearId).FirstOrDefaultAsync();

                if (yearMaterialDto.AddFirstSemesterMaterial != null)
                {
                    var existingEntries = _context.Materials
                    .Where(ptu => ptu.TermId == termOne.Id)
                    .ToList();

                    foreach (var entry in existingEntries)
                    {
                        _context.Materials.Remove(entry);
                    }
                    await _context.SaveChangesAsync();

                    foreach (var material in yearMaterialDto.AddFirstSemesterMaterial)
                    {
                        if (_context.Materials.Where(x => x.TermId == termOne.Id).Any(t => t.Name == material))
                        {
                            return BadRequest(new
                            {
                                status = 400,
                                errorMessage = $"This Name: {material} already exists in this semester."
                            });
                        }

                        var newMaterial1 = new Material
                        {
                            Name = material,
                            M_grade = 0,
                            TermId = termOne.Id
                        };

                        _context.Materials.Add(newMaterial1);
                    }
                }

                if (yearMaterialDto.AddSecondSemesterMaterial != null)
                {
                    var existingEntries = _context.Materials
                    .Where(ptu => ptu.TermId == termTwo.Id)
                    .ToList();

                    foreach (var entry in existingEntries)
                    {
                        _context.Materials.Remove(entry);
                    }
                    await _context.SaveChangesAsync();

                    foreach (var material in yearMaterialDto.AddSecondSemesterMaterial)
                    {
                        if (_context.Materials.Where(x => x.TermId == termTwo.Id).Any(t => t.Name == material))
                        {
                            return BadRequest(new
                            {
                                status = 400,
                                errorMessage = $"This Name: {material} already exists in this semester."
                            });
                        }

                        var newMaterial2 = new Material
                        {
                            Name = material,
                            M_grade = 0,
                            TermId = termTwo.Id
                        };

                        _context.Materials.Add(newMaterial2);
                    }
                }
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = 200,
                    message = "Year updated successfully."
                });
            }
            else
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid ModelState"
                });
            }
        }

        //[HttpPut("edityear/{yearId}")]
        //public async Task<IActionResult> EditYear(int yearId, [FromBody] YearDto yearDto)
        //{
        //    if (yearDto == null)
        //    {
        //        return BadRequest(new
        //        {
        //            status = 400,
        //            errorMessage = "Invalid year data."
        //        });
        //    }

        //    var existing = await _context.Years.FindAsync(yearId);
        //    if (existing == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Year with ID '{yearId}' not found."
        //        });
        //    }

        //    if (_context.Years.Any(t => t.Index == yearDto.Index && t.Id != yearId))
        //    {
        //        return BadRequest(new
        //        {
        //            status = 400,
        //            errorMessage = $"Index value '{yearDto.Index}' already exists for another year."
        //        });
        //    }

        //    existing.Index = yearDto.Index;
        //    existing.Name = yearDto.YearName;

        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Year updated successfully."
        //    });
        //}

        [HttpDelete("deleteyear/{yearId}")]
        public async Task<IActionResult> DeleteYear(int yearId)
        {
            var ToDelete = await _context.Years.FindAsync(yearId);
            if (ToDelete == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Year with ID '{yearId}' not found."
                });
            }

            _context.Years.Remove(ToDelete);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Year deleted successfully."
            });
        }
    }
}
