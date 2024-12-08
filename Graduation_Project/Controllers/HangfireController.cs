using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Controllers
{
    [Authorize(Roles = "Manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class HangfireController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HangfireController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("startbackgroundTask")]
        public async Task<IActionResult> StartBackGroundTasks()
        {
            RecurringJob.AddOrUpdate(() => TransferAllStudentsToNextTerm(), Cron.Minutely);

            return Ok("BackGround Tasks Started");
        }

        [HttpGet("startbackgroundTasktoAddstudentgrade")]
        public IActionResult StartBackGroundTaskstoAddstudentgrade()   
        {
            RecurringJob.AddOrUpdate(() => AddStudentGrade(), Cron.Minutely);

            return Ok("BackGround Tasks Started to add student grade");
        }


        //[HttpGet("test")]
        //public async Task<IActionResult> test()
        //{
        //    RecurringJob.AddOrUpdate(() => GetAllStudentGrades(), Cron.Minutely);

        //    return Ok(GetAllStudentGrades);
        //}
        //[HttpGet("getallgrades")]
        //public async Task<IActionResult> GetAllStudentGrades()
        //{
        //    var allStudentGrades = await _context.StudentGrades
        //        .Include(sg => sg.Exam)
        //        .Include(sg => sg.ApplicationUser)
        //        .ToListAsync();

        //    var result = allStudentGrades.Select(sg => new
        //    {
        //        ExamId = sg.Exam.Id,
        //        StudentId = sg.ApplicationUser.Id,
        //        StudentName = sg.ApplicationUser.Name,
        //        ExamGrade = sg.Exam.Exam_Grade,
        //        StudentGrade = sg.Student_Grade
        //    });

        //    return Ok(result);
        //}

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<bool> TransferAllStudentsToNextTerm()
        {
            var allStudents = _context.Users.ToList();
            foreach (var student in allStudents)
            {
                await TransferStudentToNextTerm(student);
            }
            return true;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<bool> TransferStudentToNextTerm(ApplicationUser student)
        {
            int currentYearIndex = GetCurrentYearIndex(student);
            int currentTermIndex = GetCurrentTermIndex(student);

            if (currentTermIndex == -1 && currentYearIndex == -1)
            {
                return false;
            }

            var currentTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == currentTermIndex);
            if (currentTerm == null)
            {
                return false;
            }

            if (currentTerm.EndDate.AddDays(30) < DateTime.Now)
            {
                currentTerm.EndDate = currentTerm.EndDate.AddYears(1);
                _context.Terms.Update(currentTerm);
                await _context.SaveChangesAsync();
            }

            if (DateTime.Now <= currentTerm.EndDate)
            {
                return false;
            }

            bool passedAllMaterials = await new GradeCalculationService(_context).PassedAllMaterialsInTermAsync(student, currentTerm.Id);

            if (passedAllMaterials)
            {
                var currentYearId = _context.Years.Where(y => y.Index == currentYearIndex)
                    .Select(y => y.Id).FirstOrDefault();

                var maxIndexInCurrentYear = _context.Terms
                   .Where(st => st.YearId == currentYearId)
                   .Select(st => st.Index)
                   .OrderByDescending(index => index)
                   .FirstOrDefault();


                if (maxIndexInCurrentYear == currentTermIndex)
                {
                    int nextTermIndex = currentTermIndex + 1;
                    int nextYearIndex = currentYearIndex + 1;

                    var nextYear = _context.Years.Include(t => t.Terms).FirstOrDefault(y => y.Index == nextYearIndex);
                    var nextTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == nextTermIndex && t.YearId == nextYear.Id);

                    if (nextTerm == null && nextYear == null)
                    {
                        var GraduatedYear = _context.Years.Include(t => t.Terms).FirstOrDefault(y => y.Index == 900000000);
                        if (GraduatedYear != null)
                        {
                            _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = student.Id, YearId = GraduatedYear.Id });
                            await _context.SaveChangesAsync();
                        }

                        var GraduatedTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == 900000000);
                        if (GraduatedTerm != null)
                        {
                            _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = GraduatedTerm.Id });
                            await _context.SaveChangesAsync();
                        }

                        // Graduation Student Class Id = be Fixed
                        student.PClassId = 500;
                        await _context.SaveChangesAsync();
                        return false;
                    }
                    else if (nextYear != null && nextTerm != null)
                    {
                        var currentclassid = student.PClassId;
                        string[] classNames = _context.PClasses.Where(p => p.Id == currentclassid).Select(p => p.Name).FirstOrDefault()?.Split('/');

                        if (classNames != null && classNames.Length == 2 && int.TryParse(classNames[0], out int firstNumber))
                        {
                            firstNumber += 1;
                            var nextClassName = $"{firstNumber}/{classNames[1]}";
                            var nextClass = _context.PClasses.FirstOrDefault(p => p.Name == nextClassName);

                            if (nextClass != null)
                            {
                                student.PClassId = nextClass.Id;
                                await _context.SaveChangesAsync();
                            }
                        }

                        _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = nextTerm.Id });
                        await _context.SaveChangesAsync();
                        _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = student.Id, YearId = nextYear.Id });
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
                else if (maxIndexInCurrentYear != currentTermIndex)
                {
                    int nextTermIndex = currentTermIndex + 1;
                    var nextTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == nextTermIndex && t.YearId == currentYearId);

                    if (nextTerm != null)
                    {
                        _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = nextTerm.Id });
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
            }
            return false;
        }

        private int GetCurrentTermIndex(ApplicationUser student)
        {
            var currentTermIndex = _context.ApplicationUserTerms
                .Where(st => st.UserId == student.Id)
                .Select(st => st.Term.Index)
                .OrderByDescending(index => index)
                .FirstOrDefault();

            return currentTermIndex;
        }

        private int GetCurrentYearIndex(ApplicationUser student)
        {
            var currentYearIndex = _context.ApplicationUserYear
                .Where(st => st.UserId == student.Id)
                .Select(st => st.Year.Index)
                .OrderByDescending(index => index)
                .FirstOrDefault();

            var termCount = _context.ApplicationUserTerms
                .Count(st => st.UserId == student.Id);

            var yearCount = _context.ApplicationUserYear
                .Count(st => st.UserId == student.Id);

            if (termCount == 0 && yearCount == 1)
            {
                var currentYear = _context.Years.FirstOrDefault(t => t.Index == currentYearIndex);

                var termsByYear = _context.Terms
                    .Where(term => term.YearId == currentYear.Id)
                    .OrderBy(term => term.Index)
                    .FirstOrDefault();


                if (currentYear != null && termsByYear != null)
                {
                    _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = termsByYear.Id });
                    _context.SaveChanges();
                }
            }

            return currentYearIndex;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task AddStudentGrade()
        {
            var exams = await _context.Exams.ToListAsync();

            foreach (var exam in exams)
            {
                var material = await _context.Materials.FindAsync(exam.MaterialId);
                if (material == null)
                {
                    continue;
                }

                var term = await _context.Terms.FindAsync(material.TermId);
                if (term == null)
                {
                    continue;
                }

                var students = await _context.Users
                    .Include(u => u.ApplicationUserTerms)
                    .Where(u => u.ApplicationUserTerms.Any(ut => ut.TermId == term.Id))
                    .ToListAsync();

                foreach (var student in students)
                {
                    var existingGrade = await _context.StudentGrades
                        .FirstOrDefaultAsync(sg => sg.Exam.Id == exam.Id && sg.ApplicationUser.Id == student.Id);

                    if (existingGrade == null)
                    {
                        var newStudentGrade = new StudentGrade
                        {
                            Student_Grade = 0,
                            Exam = exam,
                            ApplicationUser = student
                        };
                        _context.StudentGrades.Add(newStudentGrade);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }


        [HttpGet("startbackgroundTasktoAddAttendance")]
        public IActionResult StartBackgroundTaskToAddAttendance()
        {
            RecurringJob.AddOrUpdate(() => AddAttendance(), Cron.Minutely);

            return Ok("Background Tasks Started to Add Attendance");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task AddAttendance()
        {
            var students = await (from x in _context.Users
                                  join userRole in _context.UserRoles on x.Id equals userRole.UserId
                                  join role in _context.Roles on userRole.RoleId equals role.Id
                                  where role.Name == "Student"
                                  select x).ToListAsync();

            var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            if (yesterday.DayOfWeek == DayOfWeek.Friday)
            {
                return;
            }

            foreach (var student in students)
            {
                var exists = await _context.Attendences
                    .AnyAsync(x => x.ApplicationUser.Id == student.Id && x.Date_Day == yesterday);

                if (!exists)
                {
                    var newAttendance = new Attendence
                    {
                        Date_Day = yesterday,
                        Total = 0,
                        PartOne = "Not Present",
                        PartTwo = "Not Present",
                        ApplicationUser = student
                    };
                    _context.Attendences.Add(newAttendance);
                }
            }

            await _context.SaveChangesAsync();
        }


    }
}

//if (currentTermIndex % 2 == 0)
//{
//    string[] classNames = nextTerm.TermName.Split('/');
//    if (classNames.Length == 2 && int.TryParse(classNames[0], out int firstNumber))
//    {
//        firstNumber += 1;
//        nextTerm.TermName = $"{firstNumber}/{classNames[1]}";
//    }
//}

//[ApiExplorerSettings(IgnoreApi = true)]
//public async Task<bool> TransferAllStudentsToNextTerm()
// {
//    var allStudents = _context.Users.ToList();
//    foreach (var student in allStudents)
//    {
//        await TransferStudentToNextTerm(student);
//    }
//    return true;
//}

//[ApiExplorerSettings(IgnoreApi = true)]
//public async Task<bool> TransferStudentToNextTerm(ApplicationUser student)
//{
//    int currentYearIndex = GetCurrentYearIndex(student);
//    int currentTermIndex = GetCurrentTermIndex(student);

//    if (currentTermIndex == -1 && currentYearIndex == -1)
//    {
//        return false;
//    }

//    var currentTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == currentTermIndex);
//    if (currentTerm == null)
//    {
//        return false;
//    }

//    if (currentTerm.EndDate < DateTime.Now)
//    {
//        currentTerm.EndDate = currentTerm.EndDate.AddYears(1);
//        _context.Terms.Update(currentTerm);
//        await _context.SaveChangesAsync();
//    }

//    if (DateTime.Now <= currentTerm.EndDate)
//    {
//        return false;
//    }

//    bool passedAllMaterials = await new GradeCalculationService(_context).PassedAllMaterialsInTermAsync(student, currentTerm.Id);

//    if (passedAllMaterials)
//    {

//        var currentYearId = _context.Years.Where(y => y.Index == currentYearIndex)
//            .Select(y => y.Id).FirstOrDefault();

//        var maxIndexInCurrentYear = _context.Terms
//           .Where(st => st.YearId == currentYearId)
//           .Select(st => st.Index)
//           .OrderByDescending(index => index)
//           .FirstOrDefault();


//        if (maxIndexInCurrentYear == currentTermIndex)
//        {
//            int nextTermIndex = currentTermIndex + 1;
//            int nextYearIndex = currentYearIndex + 1;

//            var nextYear = _context.Years.Include(t => t.Terms).FirstOrDefault(y => y.Index == nextYearIndex);
//            var nextTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == nextTermIndex);
//            if (nextTerm == null && nextYear == null)
//            {
//                var GraduatedYear = _context.Years.Include(t => t.Terms).FirstOrDefault(y => y.Index == 900000000);
//                _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = student.Id, YearId = GraduatedYear.Id });
//                await _context.SaveChangesAsync();
//                var GraduatedTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == 900000000);
//                _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = GraduatedTerm.Id });
//                await _context.SaveChangesAsync();

//                // Graduation Student Class  Id = be Fixed
//                student.PClassId = 500;
//                await _context.SaveChangesAsync();

//                //if (student.PClassId != null)
//                //{
//                //    
//                //    var currentclassid = student.PClassId;
//                //    string[] classNames = _context.PClasses.Where(p => p.Id == currentclassid).Select(p => p.Name).FirstOrDefault()?.Split('/');

//                //    if (classNames != null && classNames.Length == 2 && int.TryParse(classNames[0], out int firstNumber))
//                //    {
//                //        firstNumber += 1;
//                //        var nextClassName = $"{firstNumber}/{classNames[1]}";
//                //        var nextClass = _context.PClasses.FirstOrDefault(p => p.Name == nextClassName);

//                //        student.PClassId = nextClass.Id;
//                //        await _context.SaveChangesAsync();
//                //    }
//                //}
//                return false;
//            }
//            else
//            {
//                var currentclassid = student.PClassId;
//                string[] classNames = _context.PClasses.Where(p => p.Id == currentclassid).Select(p => p.Name).FirstOrDefault()?.Split('/');

//                if (classNames != null && classNames.Length == 2 && int.TryParse(classNames[0], out int firstNumber))
//                {
//                    firstNumber += 1;
//                    var nextClassName = $"{firstNumber}/{classNames[1]}";
//                    var nextClass = _context.PClasses.FirstOrDefault(p => p.Name == nextClassName);

//                    student.PClassId = nextClass.Id;
//                    await _context.SaveChangesAsync();
//                }
//            }

//            _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = nextTerm.Id });
//            await _context.SaveChangesAsync();
//            _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = student.Id, YearId = nextYear.Id });
//            await _context.SaveChangesAsync();
//            return false;
//        }
//        else if(maxIndexInCurrentYear != currentTermIndex)
//        {
//            int nextTermIndex = currentTermIndex + 1;
//            var nextTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == nextTermIndex);
//            _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = nextTerm.Id });
//            await _context.SaveChangesAsync();
//            return false;
//        }
//    }
//    return false;
//}

//private int GetCurrentTermIndex(ApplicationUser student)
//{
//    var currentTermIndex = _context.ApplicationUserTerms
//        .Where(st => st.UserId == student.Id)
//        .Select(st => st.Term.Index)
//        .OrderByDescending(index => index)
//        .FirstOrDefault();

//    return currentTermIndex;
//}

//private int GetCurrentYearIndex(ApplicationUser student)
//{
//    var currentYearIndex = _context.ApplicationUserYear
//        .Where(st => st.UserId == student.Id)
//        .Select(st => st.Year.Index)
//        .OrderByDescending(index => index)
//        .FirstOrDefault();

//    var termCount = _context.ApplicationUserTerms
//        .Count(st => st.UserId == student.Id);

//    var yearCount = _context.ApplicationUserYear
//        .Count(st => st.UserId == student.Id);

//    if (termCount == 0 && yearCount == 1)
//    {
//        var currentYear = _context.Years.FirstOrDefault(t => t.Index == currentYearIndex);

//        var termsByYear =  _context.Terms
//            .Where(term => term.YearId == currentYear.Id)
//            .OrderBy(term => term.Index)
//            .FirstOrDefault();


//        if (currentYear != null)
//        {
//            _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = termsByYear.Id });
//            _context.SaveChanges();
//        }
//    }

//    return currentYearIndex;
//}
