using System;

namespace UniversityBonusSystem.Models
{
    public partial class Student
    {
        // Константа
        public const decimal MIN_BONUS_AMOUNT = 0.01m;
        
        // Статическое readonly поле
        public static readonly string UniversityName = "Технический Университет";
        
        // Virtual метод
        public virtual string GetStudentInfo()
        {
            return $"Студент: {FullName} (ID: {StudentId}), Карта: {CardNo}, Бонусы: {TotalBonus}";
        }
        
        // Метод для начисления бонусов
        public void AddBonus(decimal amount)
        {
            if (amount < MIN_BONUS_AMOUNT)
                throw new ArgumentException($"Сумма бонуса не может быть меньше {MIN_BONUS_AMOUNT}");
                
            TotalBonus += amount;
        }
        
        // Статический метод
        public static string GetUniversityInfo()
        {
            return $"{UniversityName}. Всего студентов: {TotalStudents}";
        }
        
        // Переопределение ToString()
        public override string ToString()
        {
            return GetStudentInfo();
        }
    }
}