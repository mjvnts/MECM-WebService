// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsScheduleToken.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;

    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
    using Swisscom.ConfigMgr.Library.Interfaces;

    /// <summary>
    /// The SMS_ScheduleToken abstract Windows Management Instrumentation (WMI) class 
    /// is an SMS Provider server class, in System Center 2012 Configuration Manager, 
    /// that represents a schedule token that is used for the scheduling of events 
    /// with different frequencies, for example, daily. 
    /// </summary>
    public class SmsScheduleToken : ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsScheduleToken"/> class.
        /// </summary>
        public SmsScheduleToken()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsScheduleToken"/> class.
        /// </summary>
        /// <param name="smsScheduleToken">The SMS schedule token.</param>
        public SmsScheduleToken(IResultObject smsScheduleToken)
        {
            this.StartTime = smsScheduleToken["StartTime"].DateTimeValue;
            if (smsScheduleToken.ObjectClass.Equals("SMS_ST_NonRecurring", StringComparison.CurrentCultureIgnoreCase))
            {
                this.ScheduleToken = ScheduleTokenTypes.SmsStNoRecurring;
            }
            else if (smsScheduleToken.ObjectClass.Equals("SMS_ST_RecurWeekly", StringComparison.CurrentCultureIgnoreCase))
            {
                this.ScheduleToken = ScheduleTokenTypes.SmsStRecurWeekly;
            }
        }

        /// <summary>
        /// The SMS_ScheduleToken abstract Windows Management Instrumentation (WMI) class
        /// is an SMS Provider server class, in System Center 2012 Configuration Manager,
        /// that represents a schedule token that is used for the scheduling of events
        /// with different frequencies, for example, daily.
        /// Caution: At the moment only the SMS_ST_NonRecurring is implemented.
        /// </summary>
        public enum ScheduleTokenTypes
        {
            /// <summary>
            /// Creates a SMS_ScheduleToken.
            /// </summary>
            SmsStDefault,

            /// <summary>
            /// Creates a SMS_ST_NonRecurring.
            /// </summary>
            SmsStNoRecurring,

            /// <summary>
            /// Creates a SMS_ST_RecurWeekly.
            /// </summary>
            SmsStRecurWeekly
        }

        /// <summary>
        /// Gets or sets the date and time when the scheduled action takes place. 
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the schedule token.
        /// </summary>
        /// <value>
        /// The schedule token.
        /// </value>
        public ScheduleTokenTypes ScheduleToken { get; set; }

        /// <summary>
        /// Creates a new embeded instance of the SMS_ScheduleToken object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            var tokenType = string.Empty;
            switch (this.ScheduleToken)
            {
                case ScheduleTokenTypes.SmsStNoRecurring:
                    tokenType = "SMS_ST_NonRecurring";
                    break;
            }

            if (string.IsNullOrEmpty(tokenType)) throw new NotImplementedException("The currently used token type " + this.ScheduleToken + " is not supported yet.");
            var scheduleToken = connection.CreateEmbeddedObjectInstance(tokenType);
            scheduleToken["StartTime"].DateTimeValue = this.StartTime;
            return scheduleToken;
        }
    }
}
