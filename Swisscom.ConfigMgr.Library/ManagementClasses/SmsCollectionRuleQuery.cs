// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsCollectionRuleQuery.cs" company="LANexpert S.A.">
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
    public class SmsCollectionRuleQuery : SmsCollectionRule, ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsCollectionRuleQuery"/> class.
        /// </summary>
        public SmsCollectionRuleQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsCollectionRuleQuery"/> class.
        /// </summary>
        /// <param name="collectionRule">The collection rule.</param>
        public SmsCollectionRuleQuery(WqlResultObject collectionRule)
        {
            this.QueryId = collectionRule["QueryID"].IntegerValue;
            this.QueryExpression = collectionRule["QueryExpression"].StringValue;
            this.RuleName = collectionRule["RuleName"].StringValue;
        }

        /// <summary>
        /// Gets or sets the query expression.
        /// </summary>
        /// <value>
        /// The query expression.
        /// </value>
        public string QueryExpression { get; set; }

        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        /// <value>
        /// The query id.
        /// </value>
        public int QueryId { get; set; }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// A new direct collection rule.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            var collectionRuleQuery = connection.CreateInstance("SMS_CollectionRuleQuery");
            if (!string.IsNullOrEmpty(this.QueryExpression)) collectionRuleQuery["QueryExpression"].StringValue = this.QueryExpression;
            if (!string.IsNullOrEmpty(this.RuleName)) collectionRuleQuery["RuleName"].StringValue = this.RuleName;
            //// collectionRuleQuery.Put();
            return collectionRuleQuery;
        }
    }
}
