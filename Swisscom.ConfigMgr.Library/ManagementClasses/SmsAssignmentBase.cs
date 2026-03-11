// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsAssignmentBase.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class SmsAssignmentBase
    {
        /// <summary>
        /// The to offer.
        /// </summary>
        [Serializable]
        public enum OfferTypeTypes
        {
            /// <summary>
            /// The offer is required.
            /// </summary>
            Required = 0,

            /// <summary>
            /// The offer is available.
            /// </summary>
            Available = 2
        }

        /// <summary>
        /// Gets or sets the assignment id.
        /// </summary>
        /// <value>
        /// The assignment id.
        /// </value>
        public int AssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the expiration time.
        /// </summary>
        /// <value>
        /// The expiration time.
        /// </value>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets the offer type id.
        /// </summary>
        /// <value>
        /// The offer type id.
        /// </value>
        public OfferTypeTypes OfferType { get; set; }

        /// <summary>
        /// Gets or sets the source site.
        /// </summary>
        /// <value>
        /// The source site.
        /// </value>
        public string SourceSite { get; set; }
    }
}
