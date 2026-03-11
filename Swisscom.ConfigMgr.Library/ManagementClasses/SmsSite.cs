// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsSite.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SmsSite
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsSite"/> class.
        /// </summary>
        /// <param name="smsSiteResult">The SMS site result.</param>
        public SmsSite(IResultObject smsSiteResult)
        {
            foreach (WqlResultObject smsSite in smsSiteResult) using (smsSite)
            {
                this.BuildNumber = smsSite["BuildNumber"].StringValue;
                this.InstallDir = smsSite["InstallDir"].StringValue;
                this.ServerName = smsSite["ServerName"].StringValue;
                this.SiteCode = smsSite["SiteCode"].StringValue;
                this.SiteName = smsSite["SiteName"].StringValue;
                this.Version = smsSite["Version"].StringValue;
            }
        }

        /// <summary>
        /// Gets the build number.
        /// </summary>
        public string BuildNumber { get; private set; }

        /// <summary>
        /// Gets the installation directory.
        /// </summary>
        public string InstallDir { get; private set; }

        /// <summary>
        /// Gets the name of the server.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// Gets the site code.
        /// </summary>
        public string SiteCode { get; private set; }

        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        public string SiteName { get; private set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version { get; private set; }
    }
}
