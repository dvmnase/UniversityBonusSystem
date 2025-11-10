using System;
using System.IO;
using System.Threading;

namespace UniversityBonusSystem.Services
{
    public class LoggerService
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        
        public LoggerService(string logFilePath = "batch_log.txt")
        {
            _logFilePath = logFilePath;
        }
        
        public void LogInfo(string message)
        {
            Log("INFO", message);
        }
        
        public void LogError(string message, Exception ex = null)
        {
            var errorMessage = ex != null ? $"{message} | Ошибка: {ex.Message}" : message;
            Log("ERROR", errorMessage);
        }
        
        public void LogSuccess(string message)
        {
            Log("SUCCESS", message);
        }
        
        private void Log(string level, string message)
        {
            lock (_lockObject)
            {
                try
                {
                    var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                    Console.WriteLine(logEntry);
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка записи в лог: {ex.Message}");
                }
            }
        }
    }
}