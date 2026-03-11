// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsTaskSequenceGroup.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Collections.Generic;

    using Microsoft.ConfigurationManagement.ManagementProvider;

    /// <summary>
    /// The SMS_TaskSequence_Group Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in Configuration Manager, that represents a group of steps in a task sequence. 
    /// </summary>
    [Serializable]
    public class SmsTaskSequenceGroup : SmsTaskSequenceStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceGroup"/> class.
        /// </summary>
        public SmsTaskSequenceGroup()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceGroup"/> class.
        /// </summary>
        /// <param name="taskSequenceGroup">The task sequence group.</param>
        public SmsTaskSequenceGroup(IResultObject taskSequenceGroup) : base(taskSequenceGroup)
        {    
        }

        /// <summary>
        /// Gets or sets the task sequence steps.
        /// </summary>
        /// <value>
        /// The task sequence steps.
        /// </value>
        public List<SmsTaskSequenceStep> TaskSequenceSteps { get; set; }
    }
}
