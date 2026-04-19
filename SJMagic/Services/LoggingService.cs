using System;
using System.IO;

namespace SJMagic.Services
{
    public class LoggingService
    {
        private readonly string _logDirectory;

        public LoggingService()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public string GetCurrentLogFilePath()
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            return Path.Combine(_logDirectory, fileName);
        }

        public void LogToFile(string message, string level = "INFO")
        {
            try
            {
                string filePath = GetCurrentLogFilePath();
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";

                File.AppendAllText(filePath, logEntry);
            }
            catch
            {
                // Silently fail if log file cannot be written
            }
        }
    }
}
