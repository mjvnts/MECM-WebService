// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsTaskSequenceStep.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;

    using Microsoft.ConfigurationManagement.ManagementProvider;

    /// <summary>
    /// The SMS_TaskSequence_Step Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in Configuration Manager.
    /// This class serves as an abstract base class representing a single step in a task sequence.
    /// </summary>
    [Serializable]
    public class SmsTaskSequenceStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceStep"/> class.
        /// </summary>
        public SmsTaskSequenceStep()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequenceStep"/> class.
        /// </summary>
        /// <param name="taskSequenceStep">The task sequence step.</param>
        public SmsTaskSequenceStep(IResultObject taskSequenceStep)
        {
            this.ContinueOnError = taskSequenceStep["ContinueOnError"].BooleanValue;
            this.Description = taskSequenceStep["Description"].StringValue;
            this.Enabled = taskSequenceStep["Enabled"].BooleanValue;
            this.Name = taskSequenceStep["Name"].StringValue;
            this.TaskSequenceStepType = taskSequenceStep["__CLASS"].StringValue.Equals("SMS_TaskSequenceGroup", StringComparison.CurrentCultureIgnoreCase) ? TaskSequenceStepTypes.Group : TaskSequenceStepTypes.Action;
        }

        /// <summary>
        /// The Tasksequence step types.
        /// </summary>
        [Serializable]
        public enum TaskSequenceStepTypes
        {
            /// <summary>
            /// Step is group.
            /// </summary>
            Group = 0,

            /// <summary>
            /// Step is action.
            /// </summary>
            Action = 1
        }

        /// <summary>
        /// Gets or sets a value indicating whether [continue on error].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [continue on error]; otherwise, <c>false</c>.
        /// </value>
        public bool ContinueOnError { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SmsTaskSequenceStep"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the Tasksequence step name.
        /// </summary>
        /// <value>
        /// The Tasksequence step name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the task sequence step.
        /// </summary>
        /// <value>
        /// The task sequence step.
        /// </value>
        public TaskSequenceStepTypes TaskSequenceStepType { get; set; }
    }
}
