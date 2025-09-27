using System;
using System.IO;
using UnityEngine;

namespace LicenseChain.Unity
{
    /// <summary>
    /// Simple logging utility for the LicenseChain SDK
    /// </summary>
    public static class Logger
    {
        private static LogLevel _logLevel = LogLevel.Info;
        private static bool _logToFile = false;
        private static string _logFilePath = Path.Combine(Application.persistentDataPath, "licensechain.log");

        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Fatal = 4
        }

        /// <summary>
        /// Sets the minimum log level
        /// </summary>
        /// <param name="level">Minimum log level</param>
        public static void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }

        /// <summary>
        /// Enables or disables file logging
        /// </summary>
        /// <param name="enabled">True to enable file logging</param>
        /// <param name="filePath">Optional custom file path</param>
        public static void SetFileLogging(bool enabled, string filePath = null)
        {
            _logToFile = enabled;
            if (!string.IsNullOrEmpty(filePath))
            {
                _logFilePath = filePath;
            }
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="context">Optional context object</param>
        public static void Debug(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Debug, message, context);
        }

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="context">Optional context object</param>
        public static void Info(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Info, message, context);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="context">Optional context object</param>
        public static void Warning(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Warning, message, context);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="context">Optional context object</param>
        public static void Error(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Error, message, context);
        }

        /// <summary>
        /// Logs a fatal message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="context">Optional context object</param>
        public static void Fatal(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Fatal, message, context);
        }

        /// <summary>
        /// Internal logging method
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Message to log</param>
        /// <param name="context">Optional context object</param>
        private static void Log(LogLevel level, string message, UnityEngine.Object context = null)
        {
            if (level < _logLevel)
                return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string levelName = level.ToString().ToUpper();
            string logMessage = $"[{timestamp}] [{levelName}] {message}";

            // Unity console output
            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(logMessage, context);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(logMessage, context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logMessage, context);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(logMessage, context);
                    break;
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError($"[FATAL] {logMessage}", context);
                    break;
            }

            // File output
            if (_logToFile)
            {
                try
                {
                    File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to write to log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Clears the log file
        /// </summary>
        public static void ClearLogFile()
        {
            if (File.Exists(_logFilePath))
            {
                try
                {
                    File.Delete(_logFilePath);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to clear log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gets the current log file path
        /// </summary>
        /// <returns>Current log file path</returns>
        public static string GetLogFilePath()
        {
            return _logFilePath;
        }

        /// <summary>
        /// Gets the log file size in bytes
        /// </summary>
        /// <returns>Log file size in bytes</returns>
        public static long GetLogFileSize()
        {
            if (File.Exists(_logFilePath))
            {
                try
                {
                    return new FileInfo(_logFilePath).Length;
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// Rotates the log file if it exceeds the specified size
        /// </summary>
        /// <param name="maxSizeBytes">Maximum file size in bytes</param>
        public static void RotateLogFile(long maxSizeBytes = 1024 * 1024) // 1MB default
        {
            if (GetLogFileSize() > maxSizeBytes)
            {
                try
                {
                    string backupPath = _logFilePath + ".backup";
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                    File.Move(_logFilePath, backupPath);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to rotate log file: {ex.Message}");
                }
            }
        }
    }
}
