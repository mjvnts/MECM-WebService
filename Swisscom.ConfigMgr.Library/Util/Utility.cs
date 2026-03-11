// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.Util
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// Static Utility class provides access to event log.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// EventLog name.
        /// </summary>
        private const string EventLogName = "Application";

        /// <summary>
        /// EventLog source.
        /// </summary>
        private const string EventLogSource = "Swisscom.ConfigMgr";

        /// <summary>
        /// Initializes static members of the <see cref="Utility"/> class.
        /// </summary>
        static Utility()
        {
#if DEBUG
            //if (!EventLog.SourceExists(EventLogSource))
            //{
            //    EventLog.CreateEventSource(EventLogSource, EventLogName);
            //}
#endif
        }

        /// <summary>
        /// Writes an event in the application eventlog.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id of the event log entry.</param>
        /// <param name="entryType">The <see cref="EventLogEntryType"/> of the event log entry.</param>
        public static void LogEvent(string message, int id, EventLogEntryType entryType)
        {
            using (var eventLog = new EventLog(EventLogName))
            {
                eventLog.Source = EventLogSource;
                eventLog.WriteEntry(message, entryType, id);
            }
        }

        /// <summary>
        /// Converts a string to a secure string.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The converted string as <see cref="SecureString"/>.</returns>
        public static SecureString StringToSecureString(string input)
        {
            var inputArray = input.ToCharArray();
            var secureString = new SecureString();
            foreach (var c in inputArray)
            {
                secureString.AppendChar(c);
            }

            return secureString;
        }

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="strings">The strings.</param>
        /// <returns>The function name, parameter name and parameters.</returns>
        public static string GetFunction(MethodBase method, object[] strings)
        {
            if (strings == null) return method.Name + "()";
            var sb = new StringBuilder();
            sb.Append(method.Name);
            sb.Append("(");
            var parameterInfo = method.GetParameters();
            for (var i = 0; i < parameterInfo.Length; i++)
            {
                if ((i + 1) == parameterInfo.Length)
                {
                    sb.Append(parameterInfo[i].Name + " = " + strings[i]);
                }
                else
                {
                    sb.Append(parameterInfo[i].Name + " = " + strings[i] + ", ");
                }
            }

            sb.Append(")");
            return sb.ToString();
        }

        public static XElement GetPackageXmlTemplate(string path)
        {
            var xmlDocument = XElement.Load(path);
            return xmlDocument;            
        }
    }
}
