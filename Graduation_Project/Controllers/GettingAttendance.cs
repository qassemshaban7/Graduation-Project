using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Graduation_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GettingAttendance : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public GettingAttendance(ApplicationDbContext _context)
        {
            this._context = _context;
        }


        [HttpGet("ExcelForOneStudent")]
        public ActionResult ExcelForOneStudent(string userId, int Month, int Year)
        {
            var user = _context.Users.Find(userId);

            var _empdata = ForOneStudent(userId, Month, Year);
            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet1 = wb.AddWorksheet(_empdata, $"{user.Name} Attendance");

                sheet1.Column(1).Style.Font.FontColor = XLColor.Black;

                sheet1.Columns(2, 4).Style.Font.FontColor = XLColor.Blue;

                sheet1.Row(1).CellsUsed().Style.Fill.BackgroundColor = XLColor.Black;
                sheet1.Row(1).Style.Font.FontColor = XLColor.White;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{user.Name} Attendance.xlsx");
                }
            }
        }


        [NonAction]
        private DataTable ForOneStudent(string userId, int Month, int Year)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                throw new Exception($"Student With ID {userId} Not Found.");
            }

            DataTable dt = new DataTable();
            dt.TableName = "Attendences";
            dt.Columns.Add("Student Name", typeof(string));
            dt.Columns.Add("date Of Day", typeof(DateOnly));
            dt.Columns.Add("Part One", typeof(string));
            dt.Columns.Add("Part Two", typeof(string));
            dt.Columns.Add("Total", typeof(int));

            var _list = _context.Attendences.Where(x => x.ApplicationUser == user && x.Date_Day.Year == Year).OrderBy(a => a.Date_Day).ToList();
            if (_list.Count > 0)
            {
                _list.ForEach(item =>
                {
                    dt.Rows.Add(item.ApplicationUser.Name, item.Date_Day, item.PartOne, item.PartTwo, item.Total);
                });
            }

            return dt;
        }


        [HttpGet("ExcelForOneClass")]
        public ActionResult ExcelForOneClass(int ClassId, int Day, int Month, int Year)
        {
            var pClass = _context.PClasses.Find(ClassId);

            var _empdata = ForOneClass(ClassId, Day, Month, Year);
            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet1 = wb.AddWorksheet(_empdata, $"{pClass.Name} Attendance");

                sheet1.Columns(1, 2).Style.Font.FontColor = XLColor.Black;

                sheet1.Column(3).Style.Font.FontColor = XLColor.Blue;
                sheet1.Columns(4, 5).Style.Font.FontColor = XLColor.Blue;

                sheet1.Row(1).CellsUsed().Style.Fill.BackgroundColor = XLColor.Black;
                sheet1.Row(1).Style.Font.FontColor = XLColor.White;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{pClass.Name} Attendance.xlsx");
                }
            }
        }


        [NonAction]
        private DataTable ForOneClass(int ClassId1, int Day1, int Month1, int Year1)
        {
            var pClass = _context.PClasses.Find(ClassId1);
            if (pClass == null)
            {
                throw new Exception($"Class With ID {ClassId1} Not Found.");
            }

            DataTable dt = new DataTable();
            dt.TableName = "Attendences";
            dt.Columns.Add("Student Name", typeof(string));
            dt.Columns.Add("date Of Day", typeof(DateOnly));
            dt.Columns.Add("Part One", typeof(string));
            dt.Columns.Add("Part Two", typeof(string));
            dt.Columns.Add("Total", typeof(int));

            var _list = _context.Attendences
                .Where(a => a.Date_Day.Day == Day1 && a.Date_Day.Month == Month1 && a.Date_Day.Year == Year1 && a.ApplicationUser.PClassId != null && a.ApplicationUser.PClassId == ClassId1)
                .Include(x => x.ApplicationUser).OrderBy(a => a.ApplicationUser.Name)
                .ToList();

            if (_list.Count > 0)
            {
                _list.ForEach(item =>
                {
                    dt.Rows.Add(item.ApplicationUser.Name, item.Date_Day, item.PartOne, item.PartTwo, item.Total);
                });
            }

            return dt;
        }




        [HttpGet("getAttendanceOneStudent")]
        public async Task<IActionResult> getAttendanceOneStudent(string Id)
        {
            var user = await _context.Users.FindAsync(Id);
            if (user == null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = $"Student with ID {Id} not found."
                });
            }

            var AllAttendances = await _context.Attendences
                .OrderBy(y => y.Date_Day)
                .Where(x => x.ApplicationUser == user)
                .Select(s => new { s.Total, s.Date_Day})
                .ToListAsync(); 

            return Ok(AllAttendances);
        }



        //[HttpGet("getAttendanceOneClass")]
        //public async Task<IActionResult> getAttendanceOneClass(int ClassId, int Day, int Month, int Year)
        //{
        //    var pClass = await _context.PClasses.FindAsync(ClassId);
        //    if (pClass == null)
        //    {
        //        return NotFound(new
        //        {
        //            status = 404,
        //            errorMessage = $"Class with ID {ClassId} not found."
        //        });
        //    }

        //    var AllAttendances = await _context.Attendences
        //        .Where(a => a.ApplicationUser.PClassId == ClassId && a.Date_Day.Day == Day && a.Date_Day.Month == Month && a.Date_Day.Year == Year)
        //        .OrderBy(a => a.ApplicationUser.Name)
        //        .Select(a => new { StudentName = a.ApplicationUser.Name, DateOfDay = a.Date_Day, PartOne = a.PartOne, PartTwo = a.PartTwo, total = a.Total })
        //        .ToListAsync();

        //    return Ok(AllAttendances);
        //}


        [HttpGet("getBehavior")]
        public async Task<IActionResult> GetBehavior()
        {
            var allBehaviors = await _context.Behaviors
                .OrderBy(y => y.Time)
                .Select(s => new
                {
                    Time = s.Time.ToString("dd/MM/yyyy hh:mm tt"),
                    ClassName = "Graduated_Students"
                })
                .ToListAsync();

            return Ok(allBehaviors);
        }


    }
}
