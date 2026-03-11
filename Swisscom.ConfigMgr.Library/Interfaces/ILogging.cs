// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogging.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.Interfaces
{
    public interface ILogging
    {
        /// <summary>
        /// Gets or sets the log file path and name.
        /// </summary>
        string LogFile { get; set; }

        /// <summary>
        /// Gets or sets the maximum size the log file may have.
        /// If the size is reached, a new log file will be created.
        /// </summary>
        int MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of log files to retain.
        /// Older files will be deleted when this limit is exceeded.
        /// </summary>
        int MaxLogFiles { get; set; }

        /// <summary>
        /// Gets or sets the global logging level.
        /// </summary>
        LoggingLevels LoggingLevel { get; set; }

        /// <summary>
        /// Writes a message into a log file.
        /// </summary>
        void WriteMessage(SeverityTypes severity, string contextName, int threadId, string sourceName, string message);

        /// <summary>
        /// Writes a message into a log file without a source.
        /// </summary>
        void WriteMessage(SeverityTypes severity, string contextName, int threadId, string message);
    }

    public enum SeverityTypes
    {
        Information = 1,
        Warning = 2,
        Error = 3
    }

    public enum LoggingLevels
    {
        ErrorOnly = 2,
        All = 0
    }
}