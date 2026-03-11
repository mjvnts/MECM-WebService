// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsTaskSequenceInstallApplication.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Collections.Generic;
    using System.Management;

    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

    /// <summary>
    /// The SMS_TaskSequence_InstallApplicationAction Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in Configuration Manager,
    /// that represents a task sequence action specifying an System Center 2012 Configuration Manager
    /// application to install as part of the task sequence. 
    /// </summary>
    [Serializable]
    public class SmsTaskSequenceInstallApplication : SmsTaskSequenceAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceInstallApplication"/> class.
        /// </summary>
        public SmsTaskSequenceInstallApplication()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceInstallApplication"/> class.
        /// </summary>
        /// <param name="taskSequenceInstallApplication">The task sequence install application.</param>
        public SmsTaskSequenceInstallApplication(IResultObject taskSequenceInstallApplication) : base(taskSequenceInstallApplication)
        {
            if (this.ApplicationInfo == null) this.ApplicationInfo = new List<SmsApplicationInfo>();
            this.ApplicationName = taskSequenceInstallApplication["ApplicationName"].StringValue;
            this.BaseVariableName = taskSequenceInstallApplication["BaseVariableName"].StringValue;
            this.ContinueOnInstallError = taskSequenceInstallApplication["ContinueOnInstallError"].BooleanValue;
            this.NumApps = taskSequenceInstallApplication["NumApps"].IntegerValue;
            var applicationInfoObject = taskSequenceInstallApplication.GetArrayItems("AppInfo");
            foreach (IResultObject appInfo in applicationInfoObject) using (appInfo)
            {
                var applicationInfo = new SmsApplicationInfo(appInfo);
                this.ApplicationInfo.Add(applicationInfo);
            }
        }

        /// <summary>
        /// Gets or sets the application info.
        /// </summary>
        /// <value>
        /// The application info.
        /// </value>
        public List<SmsApplicationInfo> ApplicationInfo { get; set; }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the name of the base variable.
        /// </summary>
        /// <value>
        /// The name of the base variable.
        /// </value>
        public string BaseVariableName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [continue on install error].
        /// </summary>
        /// <value>
        /// <c>true</c> if [continue on install error]; otherwise, <c>false</c>.
        /// </value>
        public bool ContinueOnInstallError { get; set; }

        /// <summary>
        /// Gets or sets the num apps.
        /// </summary>
        /// <value>
        /// The num apps.
        /// </value>
        public int NumApps { get; set; }
    }
}
