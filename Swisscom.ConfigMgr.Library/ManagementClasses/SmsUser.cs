// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsUser.cs" company="LANexpert S.A.">
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
    public class SmsUser : SmsResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsUser"/> class.
        /// </summary>
        public SmsUser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsUser"/> class.
        /// </summary>
        /// <param name="user">The user to get.</param>
        public SmsUser(WqlResultObject user)
        {
            user.Get();
            this.DistinguishedName = user["DistinguishedName"].StringValue;
            this.FullUserName = user["FullUserName"].StringValue;
            this.Mail = user["Mail"].StringValue;
            this.Name = user["Name"].StringValue;
            this.ResourceId = user["ResourceID"].IntegerValue;
            this.ResourceType = (ResourceTypes)user["ResourceType"].IntegerValue;
            this.UniqueUserName = user["UniqueUserName"].StringValue;
            this.UserGroupName = user["UserGroupName"].StringArrayValue;
            this.UserName = user["UserName"].StringValue;
            this.UserOuName = user["UserOUName"].StringArrayValue;
            this.WindowsNtDomain = user["WindowsNTDomain"].StringValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsUser"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="resourceId">The resource id.</param>
        public SmsUser(WqlConnectionManager connection, int resourceId)
        {
            using (var user = connection.GetInstance("SMS_R_User.ResourceID=" + resourceId))
            {
                user.Get();
                this.DistinguishedName = user["DistinguishedName"].StringValue;
                this.FullUserName = user["FullUserName"].StringValue;
                this.Mail = user["Mail"].StringValue;
                this.Name = user["Name"].StringValue;
                this.ResourceId = user["ResourceID"].IntegerValue;
                this.ResourceType = (ResourceTypes)user["ResourceType"].IntegerValue;
                this.UniqueUserName = user["UniqueUserName"].StringValue;
                this.UserGroupName = user["UserGroupName"].StringArrayValue;
                this.UserName = user["UserName"].StringValue;
                this.UserOuName = user["UserOUName"].StringArrayValue;
                this.WindowsNtDomain = user["WindowsNTDomain"].StringValue;
            }
        }

        /// <summary>
        /// Gets or sets the name of the distinguished.
        /// </summary>
        /// <value>
        /// The name of the distinguished.
        /// </value>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        /// <value>
        /// The full name of the user.
        /// </value>
        public string FullUserName { get; set; }

        /// <summary>
        /// Gets or sets the mail.
        /// </summary>
        /// <value>
        /// The mail adress.
        /// </value>
        public string Mail { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the unique user.
        /// </summary>
        /// <value>
        /// The name of the unique user.
        /// </value>
        public string UniqueUserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the user ou.
        /// </summary>
        /// <value>
        /// The name of the user ou.
        /// </value>
        public string[] UserOuName { get; set; }

        /// <summary>
        /// Gets or sets the name of the user group.
        /// </summary>
        /// <value>
        /// The name of the user group.
        /// </value>
        public string[] UserGroupName { get; set; }

        /// <summary>
        /// Gets or sets the windows nt domain.
        /// </summary>
        /// <value>
        /// The windows nt domain.
        /// </value>
        public string WindowsNtDomain { get; set; }
    }
}
