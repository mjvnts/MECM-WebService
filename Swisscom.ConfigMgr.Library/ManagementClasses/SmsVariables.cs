// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsVariables.cs" company="LANexpert S.A.">
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
    public class SmsVariables : ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsVariables"/> class.
        /// </summary>
        public SmsVariables()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsVariables"/> class.
        /// </summary>
        /// <param name="smsVariable">The collection variable.</param>
        /// <param name="collectionId">The collection id.</param>
        public SmsVariables(WqlResultObject smsVariable, string collectionId)
        {
            this.IsMasked = smsVariable["IsMasked"].BooleanValue;
            this.Name = smsVariable["Name"].StringValue;
            this.Value = smsVariable["Value"].StringValue;
            this.Id = collectionId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsVariables"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="collectionId">The collection id.</param>
        public SmsVariables(WqlConnectionManager connection, VariableTypes variableType, string variableName, string collectionId)
        {
            var variableTypeName = string.Empty;

            switch (variableType)
            {
                case VariableTypes.CollectionVariable:
                    variableTypeName = "SMS_CollectionVariable";
                    break;
                case VariableTypes.MachineVariable:
                    variableTypeName = "SMS_MachineVariable";
                    break;
            }

            using (var smsVariable = connection.QueryProcessor.ExecuteQuery("SELECT * FROM " + variableTypeName + " WHERE Name ='" + variableName + "'"))
            {
                this.Id = collectionId;
                this.IsMasked = smsVariable["IsMasked"].BooleanValue;
                this.Name = smsVariable["Name"].StringValue;
                this.Value = smsVariable["Value"].StringValue;
            }
        }

        /// <summary>
        /// The type of variable.
        /// </summary>
        [Serializable]
        public enum VariableTypes
        {
            /// <summary>
            /// Variable is for collection.
            /// </summary>
            CollectionVariable = 0,
            
            /// <summary>
            /// Variable is for machine.
            /// </summary>
            MachineVariable = 1
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The collection id.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is masked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is masked; otherwise, <c>false</c>.
        /// </value>
        public bool IsMasked { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new instance of the SMS_CollectionVariable object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        public IResultObject CreateCollectionVariable(WqlConnectionManager connection)
        {
            return this.CreateInstance(connection, VariableTypes.CollectionVariable);
        }

        /// <summary>
        /// Creates a new instance of the SMS_MachineVariable object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        public IResultObject CreateComputerVariable(WqlConnectionManager connection)
        {
            return this.CreateInstance(connection, VariableTypes.MachineVariable);
        }

        /// <summary>
        /// Creates a new instance of the SMS_CollectionVariable object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            return this.CreateInstance(connection, VariableTypes.CollectionVariable);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <returns>
        /// The <c>IResultObject</c> of the created object.
        /// </returns>
        private IResultObject CreateInstance(WqlConnectionManager connection, VariableTypes variableType)
        {
            var variableTypeName = string.Empty;

            switch (variableType)
            {
                case VariableTypes.CollectionVariable:
                    variableTypeName = "SMS_CollectionVariable";
                    break;
                case VariableTypes.MachineVariable:
                    variableTypeName = "SMS_MachineVariable";
                    break;
            }

            var collectionVariable = connection.CreateInstance(variableTypeName);
            collectionVariable["Name"].StringValue = this.Name;
            collectionVariable["Value"].StringValue = this.Value;
            collectionVariable["IsMasked"].BooleanValue = this.IsMasked;
            collectionVariable.Put();
            return collectionVariable;
        }
    }
}
