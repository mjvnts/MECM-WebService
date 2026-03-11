// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISmsBaseClass.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.Interfaces
{
    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ISmsBaseClass
    {
        /// <summary>
        /// Creates a new instance of the SMS object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        IResultObject CreateInstance(WqlConnectionManager connection);
    }
}
