using System;

namespace UniversityBonusSystem.Models
{
    public partial class Student
    {
        // Поля
        private string _studentId;
        private decimal _totalBonus;
        
        // Свойства
        public string StudentId 
        { 
            get => _studentId;
            set => _studentId = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        public string FullName { get; set; }
        public string CardNo { get; set; }
        public string DepartmentId { get; set; }
        
        public decimal TotalBonus 
        { 
            get => _totalBonus;
            set => _totalBonus = value >= 0 ? value : throw new ArgumentException("Бонусы не могут быть отрицательными");
        }
        
        // Статическое поле
        public static int TotalStudents = 0;
        
        // Readonly поле
        public readonly DateTime CreatedDate;
        
        // Конструктор
        public Student()
        {
            CreatedDate = DateTime.Now;
            TotalStudents++;
        }
    }
}