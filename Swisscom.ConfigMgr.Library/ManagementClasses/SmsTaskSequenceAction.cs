// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsTaskSequenceAction.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.ConfigurationManagement.ManagementProvider;

    /// <summary>
    /// The SMS_TaskSequence_Action Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in Configuration Manager, that serves as the abstract base class for all task sequence actions.
    /// </summary>
    [Serializable]
    public class SmsTaskSequenceAction : SmsTaskSequenceStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceAction"/> class.
        /// </summary>
        public SmsTaskSequenceAction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceAction"/> class.
        /// </summary>
        /// <param name="taskSequenceAction">The task sequence action.</param>
        public SmsTaskSequenceAction(IResultObject taskSequenceAction) : base(taskSequenceAction)
        {
            if (taskSequenceAction["Timeout"].ObjectValue != null) this.Timeout = taskSequenceAction["Timeout"].IntegerValue;
            SupportedEnvironmentTypes supportedEnvironment;
            if (Enum.TryParse(taskSequenceAction["SupportedEnvironment"].StringValue, true, out supportedEnvironment))
            {
                this.SupportedEnvironment = supportedEnvironment;
            }
            else
            {
                this.SupportedEnvironment = SupportedEnvironmentTypes.WinPeAndFullOs;
            }
        }

        /// <summary>
        /// The environment required to run the task sequence action.
        /// </summary>
        [Serializable]
        public enum SupportedEnvironmentTypes
        {
            /// <summary>
            /// Environment is WinPE.
            /// </summary>
            WinPe = 0,

            /// <summary>
            /// Environment is FullOS.
            /// </summary>
            FullOs = 1,

            /// <summary>
            /// Environment is WinPE and FullOS.
            /// </summary>
            WinPeAndFullOs = 2
        }

        /// <summary>
        /// The Tasksequence action types.
        /// </summary>
        [Serializable]
        public enum TaskSequenceActionTypes
        {
            /// <summary>
            /// The action is install application.
            /// </summary>
            InstallApplication = 0,

            /// <summary>
            /// The action is install package.
            /// </summary>
            InstallPackage = 1
        }

        /// <summary>
        /// Gets or sets the supported environment.
        /// </summary>
        /// <value>
        /// The supported environment.
        /// </value>
        public SupportedEnvironmentTypes SupportedEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get; set; }
    }
}
