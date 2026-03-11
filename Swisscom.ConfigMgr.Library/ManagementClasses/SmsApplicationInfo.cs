// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsApplicationInfo.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Management;

    using Microsoft.ConfigurationManagement.ManagementProvider;

    /// <summary>
    /// The SMS_TaskSequence_ApplicationInfo Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in Configuration Manager,
    /// that represents application information which is installed by task sequence. 
    /// </summary>
    [Serializable]
    public class SmsApplicationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplicationInfo"/> class.
        /// </summary>
        public SmsApplicationInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplicationInfo"/> class.
        /// </summary>
        /// <param name="applicationInfo">The application info.</param>
        public SmsApplicationInfo(IResultObject applicationInfo)
        {
            this.Description = applicationInfo["Description"].StringValue;
            this.DisplayName = applicationInfo["DisplayName"].StringValue;
            this.Name = applicationInfo["Name"].StringValue;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string Name { get; set; }
    }
}
