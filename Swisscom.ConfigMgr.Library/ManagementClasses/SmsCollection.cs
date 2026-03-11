// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsCollection.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Collections.Generic;

    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
    using Swisscom.ConfigMgr.Library.Interfaces;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class SmsCollection : ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsCollection"/> class.
        /// </summary>
        public SmsCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public SmsCollection(WqlResultObject collection)
        {
            collection.Get();
            this.CollectionId = collection["CollectionID"].StringValue;
            this.CollectionType = (CollectionTypes)collection["CollectionType"].IntegerValue;
            this.CollectionVariablesCount = collection["CollectionVariablesCount"].IntegerValue;
            this.Comment = collection["Comment"].StringValue;
            this.CurrentStatus = (CollectionCurrentStatus)collection["CurrentStatus"].IntegerValue;
            this.HasProvisionedMember = collection["HasProvisionedMember"].BooleanValue;
            this.IncludeExcludeCollectionsCount = collection["IncludeExcludeCollectionsCount"].IntegerValue;
            this.IsBuiltIn = collection["IsBuiltIn"].BooleanValue;
            this.IsReferenceCollection = collection["IsReferenceCollection"].BooleanValue;
            this.LimitToCollectionId = collection["LimitToCollectionID"].StringValue;
            this.LimitToCollectionName = collection["LimitToCollectionName"].StringValue;
            this.LocalMemberCount = collection["LocalMemberCount"].IntegerValue;
            this.MemberCount = collection["MemberCount"].IntegerValue;
            this.Name = collection["Name"].StringValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsCollection"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="collectionId">The collection id.</param>
        public SmsCollection(WqlConnectionManager connection, string collectionId)
        {
            using (var collection = connection.GetInstance("SMS_Collection.CollectionID='" + collectionId + "'"))
            {
                collection.Get();
                this.CollectionId = collection["CollectionID"].StringValue;
                this.CollectionType = (CollectionTypes)collection["CollectionType"].IntegerValue;
                this.CollectionVariablesCount = collection["CollectionVariablesCount"].IntegerValue;
                this.Comment = collection["Comment"].StringValue;
                this.CurrentStatus = (CollectionCurrentStatus)collection["CurrentStatus"].IntegerValue;
                this.HasProvisionedMember = collection["HasProvisionedMember"].BooleanValue;
                this.IncludeExcludeCollectionsCount = collection["IncludeExcludeCollectionsCount"].IntegerValue;
                this.IsBuiltIn = collection["IsBuiltIn"].BooleanValue;
                this.IsReferenceCollection = collection["IsReferenceCollection"].BooleanValue;
                this.LimitToCollectionId = collection["LimitToCollectionID"].StringValue;
                this.LimitToCollectionName = collection["LimitToCollectionName"].StringValue;
                this.LocalMemberCount = collection["LocalMemberCount"].IntegerValue;
                this.MemberCount = collection["MemberCount"].IntegerValue;
                this.Name = collection["Name"].StringValue;
            }
        }

        /// <summary>
        /// Type of the collection.
        /// </summary>
        [Serializable]
        public enum CollectionTypes
        {
            /// <summary>
            /// The collection is of type other.
            /// </summary>
            Other = 0,

            /// <summary>
            /// The collection is of type user.
            /// </summary>
            User = 1,

            /// <summary>
            /// The collection is of type device.
            /// </summary>
            Device = 2
        }

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        /// <value>
        /// The collection id.
        /// </value>
        public string CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the type of the collection.
        /// </summary>
        /// <value>
        /// The type of the collection.
        /// </value>
        public CollectionTypes CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the collection variables count.
        /// </summary>
        /// <value>
        /// The collection variables count.
        /// </value>
        public int CollectionVariablesCount { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        public CollectionCurrentStatus CurrentStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has provisioned member.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has provisioned member; otherwise, <c>false</c>.
        /// </value>
        public bool HasProvisionedMember { get; set; }

        /// <summary>
        /// Gets or sets the include exclude collections count.
        /// </summary>
        /// <value>
        /// The include exclude collections count.
        /// </value>
        public int IncludeExcludeCollectionsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is built in.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is built in; otherwise, <c>false</c>.
        /// </value>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reference collection.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is reference collection; otherwise, <c>false</c>.
        /// </value>
        public bool IsReferenceCollection { get; set; }

        /// <summary>
        /// Gets or sets the limit to collection id.
        /// </summary>
        /// <value>
        /// The limit to collection id.
        /// </value>
        public string LimitToCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the limit to collection.
        /// </summary>
        /// <value>
        /// The name of the limit to collection.
        /// </value>
        public string LimitToCollectionName { get; set; }

        /// <summary>
        /// Gets or sets the local member count.
        /// </summary>
        /// <value>
        /// The local member count.
        /// </value>
        public int LocalMemberCount { get; set; }

        /// <summary>
        /// Gets or sets the member count.
        /// </summary>
        /// <value>
        /// The member count.
        /// </value>
        public int MemberCount { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Finds the collection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>
        /// A <c>List</c> of <see cref="SmsCollection"/>
        /// </returns>
        public static List<SmsCollection> FindCollection(WqlConnectionManager connection, string collectionName)
        {
            var collectionList = new List<SmsCollection>();
            using (var collections = connection.QueryProcessor.ExecuteQuery("SELECT CollectionId FROM SMS_Collection WHERE Name = '" + collectionName + "'"))
            {
                foreach (IResultObject collection in collections) using (collection)
                    {
                        var collectionObject = new SmsCollection(connection, collection["CollectionId"].StringValue);
                        collectionList.Add(collectionObject);
                    }
            }

            return collectionList;
        }

        /// <summary>
        /// Creates a new instance of SMS_Collection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new collection.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            var collection = connection.CreateInstance("SMS_Collection");
            collection["Name"].StringValue = this.Name;
            collection["Comment"].StringValue = this.Comment;
            collection["OwnedByThisSite"].BooleanValue = true;
            if (string.IsNullOrEmpty(this.LimitToCollectionName))
            {
                switch (this.CollectionType)
                {
                    case CollectionTypes.Device:
                        this.LimitToCollectionName = "All Systems";
                        break;
                    case CollectionTypes.User:
                        this.LimitToCollectionName = "All Users";
                        break;
                }
            }
            else
            {
                collection["LimitToCollectionName"].StringValue = this.LimitToCollectionName;
            }

            if (string.IsNullOrEmpty(this.LimitToCollectionId))
            {
                foreach (var limitingCollection in FindCollection(connection, this.LimitToCollectionName))
                {
                    collection["LimitToCollectionID"].StringValue = limitingCollection.LimitToCollectionId;
                    break;
                }
            }
            else
            {
                collection["LimitToCollectionId"].StringValue = this.LimitToCollectionId;
            }

            collection["CollectionType"].IntegerValue = (int)this.CollectionType;
            collection.Put();
            collection.Get();
            collection.ExecuteMethod("RequestRefresh", null);
            return collection;
        }
    }
}
