using System;
using System.Security.Cryptography;
using System.Text;

namespace UniversityBonusSystem.Extensions
{
    public static class StringExtensions
    {
        public static string ToSha256Hash(this string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        
        public static bool IsValidCardNumber(this string cardNo)
        {
            return !string.IsNullOrWhiteSpace(cardNo) && cardNo.Length >= 8 && cardNo.Length <= 20;
        }
        
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}