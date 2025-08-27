// Services/LoggingService.cs
using System;
using System.IO;
using System.Threading.Tasks;

namespace SphericalImageViewer.Services
{
    public class LoggingService
    {
        private readonly string _logFilePath;
        private static readonly object _lockObject = new object();

        public LoggingService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "SphericalImageViewer");
            Directory.CreateDirectory(appFolder);
            _logFilePath = Path.Combine(appFolder, "application.log");
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message, Exception exception = null)
        {
            var fullMessage = exception != null ? $"{message} - {exception}" : message;
            WriteLog("ERROR", fullMessage);
        }

        private void WriteLog(string level, string message)
        {
            try
            {
                lock (_lockObject)
                {
                    var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write log: {ex.Message}");
            }
        }

        public async Task<string> GetRecentLogsAsync(int lineCount = 100)
        {
            try
            {
                if (!File.Exists(_logFilePath))
                    return "No logs available.";

                var lines = await File.ReadAllLinesAsync(_logFilePath);
                var recentLines = lines.Length > lineCount
                    ? lines[(lines.Length - lineCount)..]
                    : lines;

                return string.Join(Environment.NewLine, recentLines);
            }
            catch (Exception ex)
            {
                return $"Error reading logs: {ex.Message}";
            }
        }

        public void ClearLogs()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear logs: {ex.Message}");
            }
        }
    }
}