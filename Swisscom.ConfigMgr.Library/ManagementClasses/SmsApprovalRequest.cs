// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsApprovalRequest.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;

    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class SmsApprovalRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApprovalRequest"/> class.
        /// </summary>
        public SmsApprovalRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApprovalRequest"/> class.
        /// </summary>
        /// <param name="approvalRequest">The approval request.</param>
        public SmsApprovalRequest(WqlResultObject approvalRequest)
        {
            if (approvalRequest.Count > 1)
            {
                throw new ArgumentException("The approval Request must be one item. Submitted " + approvalRequest.Count + " items.");
            }

            this.ApplicationName = approvalRequest["SmsApplication"].StringValue;
            this.Comment = approvalRequest["Comments"].StringValue;
            this.CurrentState = approvalRequest["CurrentState"].IntegerValue;
            this.RequestGuid = approvalRequest["RequestGuid"].StringValue;
            this.User = approvalRequest["User"].StringValue;
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the state of the current.
        /// </summary>
        public int CurrentState { get; set; }

        /// <summary>
        /// Gets or sets the request GUID.
        /// </summary>
        public string RequestGuid { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }
    }
}
