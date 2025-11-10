using System.Collections.Generic;

namespace UniversityBonusSystem.Models
{
    public class Department
    {
        public string DepartmentId { get; set; }
        public string Name { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();
        
        public virtual string GetDepartmentInfo()
        {
            return $"Кафедра: {Name} (ID: {DepartmentId}), Курсов: {Courses.Count}";
        }
        
        public virtual void AddCourse(Course course)
        {
            Courses.Add(course);
        }
        
        public virtual decimal CalculateTotalBonus(decimal purchaseAmount)
        {
            // Базовая логика расчета бонусов - 1% от суммы
            return purchaseAmount * 0.01m;
        }
    }
}