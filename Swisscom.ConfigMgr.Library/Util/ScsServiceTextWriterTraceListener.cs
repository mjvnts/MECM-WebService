// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScsServiceTextWriterTraceListener.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.Util
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Implements the <see cref="TextWriterTraceListener"/> and overrides
    /// the methods Write, WriteLine and TraceEvent to format the message
    /// in the following style:
    /// <list type="bullet">
    /// <item><term>Timestamp</term></item>
    /// <item><term>Calling Thread</term></item>
    /// <item><term>Calling Class and Method</term></item>
    /// <item><term>Message</term></item>
    /// </list>
    /// this <see cref="TextWriterTraceListener"/> also monitor its size specified by <see cref="_logFileMaxSize"/>
    /// how to use the <see cref="ScsServiceTextWriterTraceListener"/>:
    /// first create a new instance of the <see cref="ScsServiceTextWriterTraceListener"/>
    /// and specify the location of the log file
    /// <code>var textWriterTraceListener = new LxpServiceTextWriterTraceListener("c:\\temp\\testoutput.log");</code>
    /// then, if desired, create a new instance of a <see cref="TraceSwitch"/> specifying a new name and description
    /// <code>var mySwitch = new TraceSwitch("ServiceTraceSwitch", "Service Trace Switch");</code>
    /// or use the one that will be created automatically and just modify the <see cref="TraceLevel"/>
    /// <code>LxpServiceTextWriterTraceListener.SetCurrentTraceLevel(TraceLevel.Verbose)</code>
    /// finally add the <see cref="ScsServiceTextWriterTraceListener"/> to the <see cref="Trace.Listeners"/>
    /// <code>Trace.Listeners.Add(textWriterTraceListener);</code>
    /// The <see cref="ScsServiceTextWriterTraceListener"/> sets the <see cref="Trace.AutoFlush"/> property to <see cref="bool.True"/>
    /// use the <see cref="Trace.WriteLineIf(bool,string)"/> to trace messages that match the specified <see cref="TraceLevel"/> in the following way
    /// <code>Trace.WriteLineIf(LxpServiceTextWriterTraceListener.GetTraceSwitch().TraceVerbose, "My Message");</code>
    /// </summary>
    public class ScsServiceTextWriterTraceListener : TextWriterTraceListener
    {
        /// <summary>
        /// <value name="_traceLevel"> general logging level</value>
        /// </summary>
        private static TraceSwitch _globalTraceSwitch = new TraceSwitch("LxpServiceTextWriterTraceListenerSwitch", "LANexpert Service Trace Listener Switch") { Level = TraceLevel.Off };

        /// <summary>
        /// <value name="_logfileMaxSize"> the maximum size of the log file</value>
        /// </summary>
        private static int _logFileMaxSize = 10240;

        /// <summary>
        /// <value name="_logFile"> the specified log file</value>
        /// </summary>
        private readonly string _logFile;

        /// <summary>
        /// <value name="_fileSystemWatcher"> the <see cref="FileSystemWatcher"/> that monitors the size of the <see cref="_logFile"/></value>
        /// </summary>
        private readonly FileSystemWatcher _fileSystemWatcher;

        /// <summary>
        /// <value name="_fileReplaceLock"> to lock the <see cref="_logFile"/> during replacing it</value>
        /// </summary>
        private readonly object _fileReplaceLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsServiceTextWriterTraceListener"/> class.
        /// </summary>
        /// <param name="logFile">The log file.</param>
        /// /// <exception cref="ArgumentNullException">Throws an exception if no file name is specified.</exception>
        /// <exception cref="ArgumentException">Throws an exception if there is an error accessing the path of the file.</exception>
        public ScsServiceTextWriterTraceListener(string logFile) : base(logFile)
        {
            if (logFile == null) throw new ArgumentNullException("logFile");
            var logFilePath = Path.GetDirectoryName(logFile);
            if (logFilePath == null) throw new ArgumentNullException(logFile);
            this._logFile = logFile;
            this._fileSystemWatcher = new FileSystemWatcher(logFilePath);
            this._fileSystemWatcher.Filter = new FileInfo(logFile).Name;
            this._fileSystemWatcher.EnableRaisingEvents = true;
            this._fileSystemWatcher.NotifyFilter = NotifyFilters.Size;
            this._fileSystemWatcher.IncludeSubdirectories = false;
            this._fileSystemWatcher.Changed += this.LogFileChanged;
            Trace.AutoFlush = true;
        }

        /// <summary>
        /// Gets the log file.
        /// </summary>
        public string LogFile
        {
            get { return this._logFile; }
        }

        /// <summary>
        /// Get the general logging level.
        /// </summary>
        /// <returns>The <see cref="TraceSwitch"/></returns>
        public static TraceSwitch GetTraceSwitch()
        {
            return _globalTraceSwitch;
        }

        /// <summary>
        /// Set the general logging level.
        /// </summary>
        /// <param name="traceLevel">The trace level.</param>
        public static void SetCurrentTraceLevel(TraceLevel traceLevel)
        {
            _globalTraceSwitch.Level = traceLevel;
        }

        /// <summary>
        /// Sets a new <see cref="TraceSwitch"/>.
        /// </summary>
        /// <param name="traceSwitch">The new <see cref="TraceSwitch"/></param>
        public static void SetTraceSwitch(TraceSwitch traceSwitch)
        {
            _globalTraceSwitch = traceSwitch;
        }

        /// <summary>
        /// Gets the specified maximum size of the log file.
        /// </summary>
        /// <returns>The specified size.</returns>
        public static int GetLogFileMaxSize()
        {
            return _logFileMaxSize;
        }

        /// <summary>
        /// Sets the maximum size of the log file.
        /// </summary>
        /// <param name="size">Maximum size of the file.</param>
        public static void SetLogFileMaxSize(int size)
        {
            _logFileMaxSize = size;
        }

        /// <summary>
        /// Writes a line in the trace log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
            string currentThreadName = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(currentThreadName))
            {
                currentThreadName = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;
            }

            // prevent any file rename operations on the log file
            lock (this._fileReplaceLock)
            {
                this.Write(DateTime.Now + "\t" + currentThreadName + "\t" + this.GetCallingMethod() + "\t" + message + Environment.NewLine);
            }
        }

        /// <summary>
        /// Writes a line in the trace log.
        /// </summary>
        /// <param name="o">The object is ToStringed.</param>
        public override void WriteLine(object o)
        {
            this.WriteLine(o.ToString());
        }

        /// <summary>
        /// Change event to check the size of the log file.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="fileSystemEventArgs">The file system event args.</param>
        private void LogFileChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var fileInfo = new FileInfo(fileSystemEventArgs.FullPath);

            // return if the file does not exist -> happens after the file is renamed, the renamed file sends a size change event and no new one has been created
            if (!fileInfo.Exists) return;
            if (fileInfo.Length > GetLogFileMaxSize())
            {
                var currentTimeStamp = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "_" +
                                       DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second;

                // prevent any Trace.WriteLine operations on the log file
                lock (this._fileReplaceLock)
                {
                    // close the open file to rename it
                    Trace.Close();
                    File.Move(fileInfo.FullName, fileInfo.FullName + "." + currentTimeStamp);
                }
            }
        }

        /// <summary>
        /// Gets information about the calling class and method.
        /// </summary>
        /// <returns>Return a formated string containing timestamp, calling class and method.</returns>
        private string GetCallingMethod()
        {
            // test block----------------------------------------------------------
            // method to determine in which Frame the parent class is located
            var stackTrace = new StackTrace();
            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var currentFrame = stackTrace.GetFrame(i).GetMethod().ReflectedType.FullName;
                if (currentFrame != null && (!currentFrame.StartsWith("System") && !currentFrame.Equals(this.GetType().FullName)))
                {
                    var stringBuilder = new StringBuilder();
                    var callerClass = stackTrace.GetFrame(i);
                    var methodName = callerClass.GetMethod().Name;
                    var className = callerClass.GetMethod().ReflectedType.FullName;
                    stringBuilder.Append(className);
                    stringBuilder.Append("-->");
                    stringBuilder.Append(methodName);
                    return stringBuilder.ToString();
                }
            }

            // test block----------------------------------------------------------
            return string.Empty;
        }
    }
}
