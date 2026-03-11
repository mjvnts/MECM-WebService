namespace Swisscom.ConfigMgr.Library.Util
{
    using Swisscom.ConfigMgr.Library.Interfaces;
    using System;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Threading;
    public sealed class Logging : ILogging
    {
        private const string MessageLogging = "<![LOG[{0}]LOG]!><time=\"{1}\" date=\"{2}\" component=\"{3}\" context=\"{4}\" type=\"{5}\" thread=\"{6}\" file=\"{7}\">";
        private const string MessageLogFileReplaced = "Created a new main log file because the previous one exceeded the maximum allowed size";
        private static volatile Logging _instance;
        private static object _syncRoot = new object();
        private volatile object _writeSync = new object();

        private Logging()
        {
            var dateTime = ManagementDateTimeConverter.ToDmtfDateTime(DateTime.Now);
            this.UtcOffset = dateTime.Substring(21, dateTime.Length - 21);
            this.MaxSize = 1024 * 1024 * 10; // Default: 10MB
            this.MaxLogFiles = 10; // Default: 10 log files
            this.LoggingLevel = LoggingLevels.ErrorOnly;
        }
        public static Logging Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null) _instance = new Logging();
                    }
                }
                return _instance;
            }
        }
        public string LogFile { get; set; }
        public int MaxSize { get; set; }
        public int MaxLogFiles { get; set; }
        public string UtcOffset { get; private set; }
        public LoggingLevels LoggingLevel { get; set; }

        public void WriteMessage(SeverityTypes severity, string methodName, int threadId, string message)
        {
            this.WriteMessage(severity, methodName, threadId, string.Empty, message);
        }

        public void WriteMessage(SeverityTypes severity, string methodName, int threadId, string sourceName, string message)
        {
            var severityCode = (int)severity;
            var loggingLevelCode = (int)this.LoggingLevel;
            if ((severityCode > loggingLevelCode) == false) return;
            var now = DateTime.Now;
            var dateString = now.ToString("MM-dd-yyyy");
            var timeString = now.ToString("HH:mm:ss.fff") + this.UtcOffset;
            var componentName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            lock (this._writeSync)
            {
                // check if the main logfile has reached the max size
                if (File.Exists(this.LogFile))
                {
                    var fileInfo = new FileInfo(this.LogFile);
                    if (fileInfo.Length > this.MaxSize)
                    {
                        // rename and version the main logfile
                        var newName = GetVersionedLogFileName();
                        fileInfo.MoveTo(newName);
                        // create a new main logfile automatically when the next logging happens
                        WriteMessage(SeverityTypes.Information, "WriteMessage", threadId, MessageLogFileReplaced);
                    }
                }
                // logging in the main logfile
                using (var fileStream = new FileStream(this.LogFile, FileMode.Append, FileAccess.Write))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(string.Format(MessageLogging, message, timeString, dateString, methodName, string.Empty, (int)severity, threadId, sourceName));
                }
                // remove old versioned logs
                CleanupOldLogs();
            }
        }

        private string GetVersionedLogFileName()
        {
            var directory = Path.GetDirectoryName(this.LogFile);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(this.LogFile);
            var extension = Path.GetExtension(this.LogFile);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"); 
            return Path.Combine(directory, $"{fileNameWithoutExt}_{timestamp}{extension}");
        }

        private void CleanupOldLogs()
        {
            var directory = Path.GetDirectoryName(this.LogFile);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(this.LogFile);
            var extension = Path.GetExtension(this.LogFile);

            // Sort after Filename instead of CreationTime
            var logFiles = Directory.GetFiles(directory, $"{fileNameWithoutExt}_*{extension}")
                                   .Select(f => new FileInfo(f))
                                   .OrderBy(f => f.Name) // Oldest timestamp first
                                   .ToList();

            // remove the oldest logfiles when the MaxLogFiles is reached
            if (logFiles.Count > this.MaxLogFiles)
            {
                var filesToDelete = logFiles.Take(logFiles.Count - this.MaxLogFiles);
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        WriteMessage(SeverityTypes.Error, "CleanupOldLogs", Thread.CurrentThread.ManagedThreadId, $"Failed to delete {file.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}
