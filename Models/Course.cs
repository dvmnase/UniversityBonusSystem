namespace UniversityBonusSystem.Models
{
    public class Course
    {
        public string CourseId { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }
        public string DepartmentId { get; set; }
        
        public string GetCourseInfo()
        {
            return $"Курс: {Title} (Кредиты: {Credits})";
        }
    }
}