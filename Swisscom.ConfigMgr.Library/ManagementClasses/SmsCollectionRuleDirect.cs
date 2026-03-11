// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsCollectionRuleDirect.cs" company="LANexpert S.A.">
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
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
    using Swisscom.ConfigMgr.Library.Interfaces;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class SmsCollectionRuleDirect : SmsCollectionRule, ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsCollectionRuleDirect"/> class.
        /// </summary>
        public SmsCollectionRuleDirect()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsCollectionRuleDirect"/> class.
        /// </summary>
        /// <param name="collectionRule">The collection rule.</param>
        public SmsCollectionRuleDirect(WqlResultObject collectionRule)
        {
            this.ResourceClassName = collectionRule["ResourceClassName"].StringValue;
            this.ResourceId = collectionRule["ResourceID"].IntegerValue;
            this.RuleName = collectionRule["RuleName"].StringValue;
        }

        /// <summary>
        /// Gets or sets the name of the resource class.
        /// </summary>
        /// <value>
        /// The name of the resource class.
        /// </value>
        public string ResourceClassName { get; set; }

        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        /// <value>
        /// The resource id.
        /// </value>
        public int ResourceId { get; set; }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// A new direct collection rule.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            var collectionRuleDirect = connection.CreateInstance("SMS_CollectionRuleDirect");
            collectionRuleDirect["ResourceClassName"].StringValue = string.IsNullOrEmpty(this.ResourceClassName) ? string.Empty : this.ResourceClassName;
            collectionRuleDirect["ResourceId"].IntegerValue = this.ResourceId;
            collectionRuleDirect["RuleName"].StringValue = string.IsNullOrEmpty(this.RuleName) ? string.Empty : this.RuleName;
            //// collectionRuleDirect.Put();
            return collectionRuleDirect;
        }
    }
}
