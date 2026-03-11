// -----------------------------------------------------------------------
// <copyright file="SmsResource.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// -----------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class SmsResource
    {
        /// <summary>
        /// Type of resources on the site.
        /// </summary>
        [Serializable]
        public enum ResourceTypes
        {
            /// <summary>
            /// Class SMS_R_UserGroup.
            /// </summary>
            UserGroup = 3,

            /// <summary>
            /// Class SMS_R_User.
            /// </summary>
            User = 4,

            /// <summary>
            /// Class SMS_R_System.
            /// </summary>
            System = 5,

            /// <summary>
            /// Class SMS_R_IPNetwork.
            /// </summary>
            IpNetwork = 6
        }

        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        /// <value>
        /// The resource id.
        /// </value>
        public int ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        public ResourceTypes ResourceType { get; set; }
    }
}
