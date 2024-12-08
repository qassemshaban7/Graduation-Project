using GP_MVC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GP_MVC.ViewModels
{
    public class AddStudentGradeVM
    {
        [Required]
        public double Student_Grade { get; set; }

        [Required]
        public int studentGradeId { get; set; }

        [Required]
        public string StudentId { get; set; }

        public int MaterialId { get; set; }

        public int ExamId { get; set; }

        public int TermId { get; set; }

        public int YearId { get; set; }

        public SelectListItem Material { get; set; }

        public List<SelectListItem> Exams { get; set; }

        public double? SelectedExamGrade { get; set; }

        public double? SelectedStudentGrade { get; set; }
    }

}
