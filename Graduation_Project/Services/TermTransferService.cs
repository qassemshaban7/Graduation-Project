//using Graduation_Project.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Graduation_Project.Services
//{
//    public class TermTransferService
//    {
//        private readonly ApplicationDbContext _context;

//        public TermTransferService(ApplicationDbContext context)
//        {
//            _context = context;
//        }


//        //[ApiExplorerSettings(IgnoreApi = true)]
//        //public async Task<bool> TransferAllStudentsToNextTerm()
//        //{
//        //    var allStudents = _context.Users.ToList();
//        //    foreach (var student in allStudents)
//        //    {
//        //        await TransferStudentToNextTerm(student);
//        //    }
//        //    return true;
//        //}

//        [ApiExplorerSettings(IgnoreApi = true)]
//        public async Task<bool> TransferStudentToNextTerm(ApplicationUser student)
//        {
//            int currentTermIndex = GetCurrentTermIndex(student);
//            int currentYearIndex = GetCurrentYearIndex(student);
//            if (currentTermIndex == -1 && currentYearIndex == -1)
//            {
//                return false;
//            }

//            var currentTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == currentTermIndex);
//            if (currentTerm == null)
//            {
//                return false;
//            }

//            //if (currentTerm.EndDate.AddDays(35) < DateTime.Now)
//            //{
//            //    currentTerm.EndDate = currentTerm.EndDate.AddYears(1);
//            //    _context.Terms.Update(currentTerm);
//            //    await _context.SaveChangesAsync();
//            //}

//            if (DateTime.Now <= currentTerm.EndDate)
//            {
//                return false;
//            }

//            bool passedAllMaterials = await new GradeCalculationService(_context).PassedAllMaterialsInTermAsync(student, currentTerm.Id);

//            if (passedAllMaterials)
//            {
//                var currentYearId = _context.Years.Where(y => y.Index == currentYearIndex)
//                    .Select(y => y.Id).FirstOrDefault();

//                var maxIndexInCurrentYear = _context.ApplicationUserTerms
//                    .Where(st => st.UserId == student.Id && st.Term.YearId == currentYearId /*&& st.Term.Index != null*/)
//                    .Select(st => st.Term.Index)
//                    .OrderByDescending(index => index)
//                    .FirstOrDefault();


//                if (maxIndexInCurrentYear == currentTermIndex)
//                {
//                    int nextTermIndex = currentTermIndex + 1;
//                    int nextYearIndex = currentYearIndex + 1;

//                    var nextYear = _context.Years.Include(t => t.Terms).FirstOrDefault(y => y.Index == nextYearIndex);
//                    var nextTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == nextTermIndex);
//                    if (nextTerm == null && nextYear == null)
//                    {
//                        var GraduatedYear = _context.Years.Include(t => t.Terms).FirstOrDefault(y => y.Index == 100);
//                        _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = student.Id, YearId = GraduatedYear.Id });
//                        await _context.SaveChangesAsync();
//                        var GraduatedTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == 100);
//                        _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = GraduatedTerm.Id });
//                        await _context.SaveChangesAsync();

//                        return false;
//                    }
//                    if (student.PClassId != null)
//                    {
//                        /////// from here 
//                        string[] classNames = student.PClass.Name.Split('/');
//                        if (classNames.Length == 2 && int.TryParse(classNames[0], out int firstNumber))
//                        {
//                            firstNumber += 1;
//                            var nextClassName = $"{firstNumber}/{classNames[1]}";
//                            var nextClass = _context.PClasses.FirstOrDefault(p => p.Name == nextClassName);
//                            if (nextClass == null)
//                            {
//                                nextClass = new PClass { Name = nextClassName };
//                                _context.PClasses.Add(nextClass);
//                                await _context.SaveChangesAsync();
//                            }
//                            student.PClassId = nextClass.Id;
//                            await _context.SaveChangesAsync();
//                        }
//                    }
//                    else return false;
//                    _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = nextTerm.Id });

//                    _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = student.Id, YearId = nextYear.Id });
//                    await _context.SaveChangesAsync();
//                    return true;
//                }
//                else
//                {
//                    int nextTermIndex = currentTermIndex + 1;
//                    var nextTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == nextTermIndex);
//                    _context.ApplicationUserTerms.Add(new ApplicationUserTerm { UserId = student.Id, TermId = nextTerm.Id });
//                    await _context.SaveChangesAsync();
//                    return true;
//                }
//            }

//            return false;
//        }

//        private int GetCurrentTermIndex(ApplicationUser student)
//        {
//            var currentTermIndex = _context.ApplicationUserTerms
//                .Where(st => st.UserId == student.Id)
//                .Select(st => st.Term.Index)
//                .OrderByDescending(index => index)
//                .FirstOrDefault();


//            var termCount = _context.ApplicationUserTerms
//                .Count(st => st.UserId == student.Id);

//            var yearCount = _context.ApplicationUserYear
//                .Count(st => st.UserId == student.Id);

//            if (termCount == 1 && yearCount == 0)
//            {
//                var currentTerm = _context.Terms.Include(t => t.Materials).FirstOrDefault(t => t.Index == currentTermIndex);
//                if (currentTerm != null)
//                {

//                    _context.ApplicationUserYear.Add(new ApplicationUserYear { UserId = student.Id, YearId = currentTerm.YearId });
//                    _context.SaveChanges();
//                }
//            }
//            return currentTermIndex;
//        }

//        private int GetCurrentYearIndex(ApplicationUser student)
//        {
//            var currentYearIndex = _context.ApplicationUserYear
//                .Where(st => st.UserId == student.Id)
//                .Select(st => st.Year.Index)
//                .OrderByDescending(index => index)
//                .FirstOrDefault();
//            return currentYearIndex;
//        }

//    }
//}
