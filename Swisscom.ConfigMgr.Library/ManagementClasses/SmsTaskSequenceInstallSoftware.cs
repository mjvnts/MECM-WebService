// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsTaskSequenceInstallSoftware.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;

    using Microsoft.ConfigurationManagement.ManagementProvider;

    /// <summary>
    /// The SMS_TaskSequence_InstallSoftwareAction Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in Configuration Manager, that represents a task sequence action that installs software.
    /// </summary>
    [Serializable]
    public class SmsTaskSequenceInstallSoftware : SmsTaskSequenceAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceInstallSoftware"/> class.
        /// </summary>
        public SmsTaskSequenceInstallSoftware()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceInstallSoftware"/> class.
        /// </summary>
        /// <param name="taskSequenceInstallSoftware">The task sequence install software.</param>
        public SmsTaskSequenceInstallSoftware(IResultObject taskSequenceInstallSoftware) : base(taskSequenceInstallSoftware)
        {
            this.BaseVariableName = taskSequenceInstallSoftware["BaseVariableName"].StringValue;
            if (taskSequenceInstallSoftware["ContinueOnInstallError"].ObjectValue != null) this.ContinueOnInstallError = taskSequenceInstallSoftware["ContinueOnInstallError"].BooleanValue;
            this.PackageId = taskSequenceInstallSoftware["PackageID"].StringValue;
            this.ProgramName = taskSequenceInstallSoftware["ProgramName"].StringValue;
        }

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
        /// Gets or sets the package id.
        /// </summary>
        /// <value>
        /// The package id.
        /// </value>
        public string PackageId { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>
        /// The name of the program.
        /// </value>
        public string ProgramName { get; set; }
    }
}
