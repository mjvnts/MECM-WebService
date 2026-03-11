// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigMgrUtility.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Management.Instrumentation;
    using System.Text;
    using System.Threading;

    using ManagementClasses;
    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
    using Microsoft.ConfigurationManagement.Messaging.Messages.Server;
    using Swisscom.ConfigMgr.Library.Interfaces;
    using static Azure.Core.HttpHeader;

    /// <summary>
    /// The ConfigMgr utility provides functions to automate the management of ConfigMgr 2012.
    /// To get familiar with WMI and Managed Code it's recommended to read the following
    /// msdn articles http://msdn.microsoft.com/en-us/library/cc144164.aspx .
    /// Important: ConfigMgr has a lot of "lazy WMI properties", please read the following
    /// msdn article for detailed information about how to read lazy properties
    /// http://msdn.microsoft.com/en-us/library/cc145526.aspx
    /// Very important: Never forget to dispose WMI objects explicitly. A good practice
    /// is to use "using ()".
    /// </summary>
    public class ConfigMgrUtility : IDisposable
    {
        /// <summary>
        /// The basic WMI select query.
        /// </summary>
        private const string WmiSelectQuery = "SELECT * FROM {0}";

        /// <summary>
        /// WMI query with selection and where clause.
        /// </summary>
        private const string WmiSelectQueryWhereString = "SELECT * FROM {0} WHERE {1} = '{2}'";

        /// <summary>
        /// WMI query with selection and custom complex where clause.
        /// </summary>
        private const string WmiSelectQueryWhereStringAndString = "SELECT * FROM {0} WHERE {1} = '{2}' AND {3} = '{4}'";

        /// <summary>
        /// WMI query with selection and custom complex where clause.
        /// </summary>
        private const string WmiSelectQueryWhereStringAndCustom = "SELECT * FROM {0} WHERE {1} = '{2}' AND {3}";

        /// <summary>
        /// The basic WMI select query with where clause as integer.
        /// </summary>
        private const string WmiSelectQueryWhereInteger = "SELECT * FROM {0} WHERE {1} = {2}";

        /// <summary>
        /// WMI query with selection as integer and string.
        /// </summary>
// ReSharper disable UnusedMember.Local
        private const string WmiSelectQueryWhereIntegerAndString = "SELECT * FROM {0} WHERE {1} = {2} AND {3} = '{4}'";
// ReSharper restore UnusedMember.Local

        /// <summary>
        /// Direct reference to a WMI object using its key as string.
        /// </summary>
        private const string WmiDirectReferenceString = "{0}.{1}='{2}'";

        /// <summary>
        /// Direct reference to a WMI object using its key as int.
        /// </summary>
        private const string WmiDirectReferenceInt = "{0}.{1}={2}";

        /// <summary>
        /// Error message unable to connect the SCCM server.
        /// </summary>
        private const string ErrorMessageUnableToConnect = "Unable to connect to {0}.";

        /// <summary>
        /// Error message invalid query.
        /// </summary>
        private const string ErrorMessageInvalidQuery = "The query \"{0}\" is invalid.";

        /// <summary>
        /// Error message failed to delete the collection rule.
        /// </summary>
        private const string ErrorMessageFailedDeleteCollectionRule = "Failed to delete the membership rule \"{0}\": {1}";

        /// <summary>
        /// Error message failed to add membership rule.
        /// </summary>
        private const string ErrorMessageFailedToModifyMembershipRule = "Failed to {0} the membership rule to collection = \"{1}\" using query \"{2}\".";

        /// <summary>
        /// Error message failed to remove member from rule.
        /// </summary>
        private const string ErrorMessageFailedToModifyMemberFromRule = "Failed to {0} the member \"{1}\" from \"{2}\": {3}";

        /// <summary>
        /// Error message failed to approve the client.
        /// </summary>
        private const string ErrorMessageFailedToApproveClient = "Failed to approve the client {0}";

        /// <summary>
        /// Error message failed to find collection with specific parameter.
        /// </summary>
        private const string ErrorMessageUnableToFindCollection = "Unable to find a collection with {0} {1}";

        /// <summary>
        /// Error message no MAC address or BIOS GUID.
        /// </summary>
        private const string ErrorMessageNoMacOrBiosGuid = "The parameters macAddress or smBiosGuid must have a value";

        /// <summary>
        /// Error message failed to clear the last PXE advertisement.
        /// </summary>
        private const string ErrorMessageFailedToClearPxeAdv = "Failed to clear the last PXE advertisement of {0}";

        /// <summary>
        /// Error message failed to find application with specific name.
        /// </summary>
        private const string ErrorMessageUnableToFindApplicationWithName = "Unable to find an application with name {0}";

        /// <summary>
        /// Error message unable to find a client.
        /// </summary>
        private const string ErrorMessageUnableToFindClient = "Unable to find a client with {0} {1}";

        private const string ErrorMessageUnableToFindUser = "Unable to find a User with {0} {1}";

        /// <summary>
        /// Error message failed to delete all members of a collection.
        /// </summary>
// ReSharper disable UnusedMember.Local
        private const string ErrorMessageFailedToDeleteAllMembers = "Failed to delete all members of collection {0}";
// ReSharper restore UnusedMember.Local

        /// <summary>
        /// Error message failed to get an object of type.
        /// </summary>
        private const string ErrorMessageFailedToFindObjectOfType = "Failed to get an object of type {0} and name {1}";

        /// <summary>
        /// Error message failed to add/remove a member from a collection.
        /// </summary>
        private const string ErrorMessageFailedToModifyMemberInCollection = "Failed to {0} the member {1} in {2}";

        /// <summary>
        /// Error message failed to create a user device affinity.
        /// </summary>
        private const string ErrorMessageFailedToCreateUserDeviceAffinity = "Failed to create a device affinity of {0} with the user {1}.";

        /// <summary>
        /// Error message failed to update collection because it is not ready.
        /// </summary>
        private const string ErrorMessageCollectionNotReady = "Failed to update the collection {0} beccause it is not ready.";

        /// <summary>
        /// The SCCM management server.
        /// </summary>
        private readonly WqlConnectionManager _managementServer;

        /// <summary>
        /// The SMS site.
        /// </summary>
        private readonly SmsSite _smsSite;

        /// <summary>
        /// Indicates if the object is disposed or not.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// How to add a new computer
        /// </summary>
        public enum AddComputerTypes
        {
            /// <summary>
            /// By Mac Adress
            /// </summary>
            MacAddress,
            /// <summary>
            /// By SMBIOSGUID
            /// </summary>
            SmBiosGuid
        }

        /// <summary>
        /// The scope of the deployment,
        /// if it is for an application
        /// or a taskequence.
        /// </summary>
        public enum DeploymentScopeTypes
        {
            /// <summary>
            /// Scope is for application.
            /// </summary>
            Application,

            /// <summary>
            /// Scope is for tasksequence.
            /// </summary>
            Tasksequence
        }

        /// <summary>
        /// How to handle maintenance windows.
        /// </summary>
        public enum MaintenanceWindowHandlingTypes
        {
            /// <summary>
            /// Maintenance window will be used.
            /// </summary>
            NoHandling,

            /// <summary>
            /// Software installation.
            /// </summary>
            OnlyInstall,

            /// <summary>
            /// System restart (if required to complete the installation).
            /// </summary>
            OnlyRebootIfRequired,

            /// <summary>
            /// Software installation and System restart.
            /// </summary>
            InstallAndRebootIfRequired
        }

        /// <summary>
        /// Rerun behaviour of an advertisement.
        /// </summary>
        public enum RerunBehaviourTypes
        {
            /// <summary>
            /// Always rerun the program.
            /// </summary>
            RerunAlways,

            /// <summary>
            /// Never rerun the program.
            /// </summary>
            RerunNever,

            /// <summary>
            /// Rerun the program if execution previously failed.
            /// </summary>
            RerunIfFailed,

            /// <summary>
            /// Rerun the program if execution previously succeeded.
            /// </summary>
            RerunIfSucceeded
        }

        /// <summary>
        /// The download behaviour.
        /// </summary>
        public enum DownloadBehaviourTypes
        {
            /// <summary>
            /// Download the program from the local distribution point.
            /// No remote distribution point will be used.
            /// </summary>
            DownloadOnlyFromLocalDp,

            /// <summary>
            /// Download the program from the local distribution point and
            /// Download the program from the remote distribution point.
            /// </summary>
            DownloadLocalAndRemoteDp,

            /// <summary>
            /// Download the program on demand from the local distribution point.
            /// No remote distribution point will be used.
            /// </summary>
            DownloadOnDemandOnlyFromLocalDp,

            /// <summary>
            /// Download the program on demand from the local distribution point and
            /// Download the program on demand from the remote distribution point.
            /// </summary>
            DownloadOnDemandLocalAndRemoteDp
        }

        /// <summary>
        /// The network behaviour. Is from
        /// DeviceFlags. Currently only AllNetwork
        /// is needed.
        /// </summary>
        public enum NetworkBehaviourTypes
        {
            /// <summary>
            /// Download on all networks.
            /// </summary>
            DownloadOnAllNetworks
        }

        /// <summary>
        /// The type of resource.
        /// </summary>
        public enum ResourceType
        {
            /// <summary>
            /// The resource is a collection.
            /// </summary>
            Collection,

            /// <summary>
            /// The resource is a computer.
            /// </summary>
            Computer,

            /// <summary>
            /// The resource is an application.
            /// </summary>
            Application,

            /// <summary>
            /// The resource is a package.
            /// </summary>
            Package,

            /// <summary>
            /// The resource is a driver.
            /// </summary>
            Driver,

            /// <summary>
            /// The resource is a task sequence.
            /// </summary>
            TaskSequence
        }

        /// <summary>
        /// User device affinity types.
        /// </summary>
        public enum DeviceAffinityTypes
        {
            /// <summary>
            /// Affinity type SoftwareCatalog.
            /// </summary>
            SoftwareCatalog = 1,

            /// <summary>
            /// Affinity type Administrator.
            /// </summary>
            Administrator = 2,

            /// <summary>
            /// Affinity type User.
            /// </summary>
            User = 3,

            /// <summary>
            /// Affinity type UsageAgent.
            /// </summary>
            UsageAgent = 4,

            /// <summary>
            /// Affinity type DeviceManagement.
            /// </summary>
            DeviceManagement = 5,

            /// <summary>
            /// Affinity type Osd.
            /// </summary>
            Osd = 6,

            /// <summary>
            /// Affinity type FastInstall.
            /// </summary>
            FastInstall = 7,

            /// <summary>
            /// Affinity type ExchangeConnector.
            /// </summary>
            ExchangeConnector = 8
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigMgrUtility"/> class.
        /// </summary>
        /// <param name="sccmServer">The SCCM server.</param>
        public ConfigMgrUtility(string sccmServer, string userName, string password)
        {
            this.SccmServer = sccmServer;
            var namedValues = new SmsNamedValuesDictionary();
            this._managementServer = new WqlConnectionManager(namedValues);
            this.IsConnected = false;
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                this.IsConnected = this._managementServer.Connect(this.SccmServer);
            }
            else
            {
                this.IsConnected = this._managementServer.Connect(this.SccmServer, userName, password);
            }

            if (!this.IsConnected)
            {
                // throw new SmsConnectionException(string.Format(ErrorMessageUnableToConnect, this.SccmServer));
            }

            using (var smsSiteResult = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQuery, "SMS_Site")))
            {
                this._smsSite = new SmsSite(smsSiteResult);
            }

            this.SccmConnection = this._managementServer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigMgrUtility"/> class.
        /// </summary>
        public ConfigMgrUtility() : this(System.Net.Dns.GetHostName(), string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigMgrUtility"/> class.
        /// </summary>
        public ConfigMgrUtility(string userName, string password) : this(System.Net.Dns.GetHostName(), userName, password)
        {
        }

        public ConfigMgrUtility(string sccmServer) : this(sccmServer, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConfigMgrUtility"/> class,
        /// releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ConfigMgrUtility"/> is reclaimed by garbage collection.
        /// </summary>
        ~ConfigMgrUtility()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// If to add or remove an agent from a collection.
        /// </summary>
        public enum CollectionAction
        {
            /// <summary>
            /// Action to add a new member.
            /// </summary>
            AddMembershipRule,

            /// <summary>
            /// Action to remove an existing member.
            /// </summary>
            DeleteMembershipRule
        }

        /// <summary>
        /// Type of computer identifier.
        /// </summary>
        private enum ComputerIdType
        {
            /// <summary>
            /// The computer name.
            /// </summary>
            ComputerName,

            /// <summary>
            /// The sm bios GUID.
            /// </summary>
            SmBiosGuid
        }

        /// <summary>
        /// Gets the SCCM server.
        /// </summary>
        public string SccmServer { get; private set; }

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the SCCM connection.
        /// </summary>
        public WqlConnectionManager SccmConnection { get; private set; }

        /// <summary>
        /// Gets the task sequence.
        /// </summary>
        /// <param name="tasksequenceName">Name of the tasksequence.</param>
        /// <returns>
        /// The <c>SmsTaskSequence</c>.
        /// </returns>
        public SmsTaskSequence GetTaskSequence(string tasksequenceName)
        {
            return new SmsTaskSequence(this._managementServer, tasksequenceName);
        }

        /// <summary>
        /// Gets the name of the tasksequence id by.
        /// </summary>
        /// <param name="tasksequenceName">Name of the tasksequence.</param>
        /// <returns>The id of the tasksequence.</returns>
        public string GetTasksequenceIdByName(string tasksequenceName)
        {
            using (var tasksequences = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_TasksequencePackage", "Name", tasksequenceName)))
            {
                foreach (IResultObject tasksequence in tasksequences) using (tasksequence)
                {
                    return tasksequence["PackageID"].StringValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether [is primary user] [the specified user name].
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>
        ///   <c>true</c> if [is primary user] [the specified user name]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPrimaryUser(string userName, string computerName)
        {
            foreach (var user in this.GetPrimaryUsers(computerName))
            {
                if (user.Contains("\\"))
                {
                    var fullName = user.Split('\\');
                    var expectedUserName = userName.Split('\\');
                    if (fullName[fullName.GetUpperBound(0)].Equals(expectedUserName[expectedUserName.GetUpperBound(0)], StringComparison.InvariantCultureIgnoreCase)) return true;
                }
                else
                {
                    if (user.Equals(userName, StringComparison.InvariantCultureIgnoreCase)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the primary users.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>The primary users of the specified computer.</returns>
        public string[] GetPrimaryUsers(string computerName)
        {
            var relatedUser = new List<string>();
            using (IResultObject machineRelationship = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereIntegerAndString, "SMS_UserMachineRelationship", "Sources", 2, "ResourceName", computerName)))
            {
                foreach (IResultObject machine in machineRelationship) using (machine)
                {
                    relatedUser.Add(machine["UniqueUserName"].StringValue);
                }
            }

            return relatedUser.ToArray();
        }

        public List<string> GetAllPrimaryUsersOfClient(string computerName)
        {
            var relatedUser = new List<string>();
            var primaryUsers = this.GetPrimaryUsers(computerName);
            foreach (var primaryUser in primaryUsers)
            {
                var arrPrimaryUser = primaryUser.Split('\\');
                //var userObjects = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_R_User", "UniqueUserName", arrPrimaryUser[0] + "\\\\" + arrPrimaryUser[1]));
                //foreach (WqlResultObject userObject in userObjects)
                //{
                //    var smsUser = new SmsUser(this._managementServer, userObject.Properties["ResourceID"].IntegerValue);
                //    relatedUser.Add(smsUser);
                //}

                relatedUser.Add(arrPrimaryUser[1]);
            }

            return relatedUser;
        }

        /// <summary>
        /// Sets the primary user.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="deviceAffinityType">Type of the device affinity.</param>
        public void SetPrimaryUser(string computerName, string userName, DeviceAffinityTypes deviceAffinityType)
        {
            var clientId = this.GetComputerResourceIdByName(computerName, true);
            var user = this.GetUser(userName);

            var inParameters = new Dictionary<string, object>();
            inParameters.Add("MachineResourceID", clientId);
            inParameters.Add("UserAccountName", user.UniqueUserName);
            inParameters.Add("SourceId", deviceAffinityType);
            inParameters.Add("TypeId", 1);
            using (var retVal = this._managementServer.ExecuteMethod("SMS_UserMachineRelationShip", "CreateRelationShip", inParameters))
            {
                if (retVal["ReturnValue"].IntegerValue != 0) throw new Exception(string.Format(ErrorMessageFailedToCreateUserDeviceAffinity, computerName, user.UniqueUserName));
            }
        }

        /// <summary>
        /// Deletes the primary user.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="userName">Name of the user.</param>
        public void DeletePrimaryUser(string computerName, string userName)
        {
            var clientId = this.GetComputerResourceIdByName(computerName);
            SmsUser user = null;
            try
            {
                user = this.GetUser(userName);
            }
            catch
            {
                // empty catch. If the user can not be retrieved, it does not exist in SCCM
            }

            if (user == null)
            {
                using (var relationships = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_UserMachineRelationShip", "ResourceID", clientId)))
                {
                    var userToRemove = userName.Split('\\');
                    foreach (IResultObject relationship in relationships) using (relationship)
                        {
                            var existingName = relationship.Properties["UniqueUserName"].StringValue;
                            if (existingName.ToLower().Contains(userToRemove[userToRemove.GetUpperBound(0)].ToLower()))
                            {
                                relationship.Delete();
                                break;
                            }
                        }
                }
            }
            else
            {
                using (var relationships = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereIntegerAndString, "SMS_UserMachineRelationShip", "ResourceID", clientId, "UniqueUserName", user.UniqueUserName.Replace("\\", "\\\\"))))
                {
                    foreach (IResultObject relationship in relationships) using (relationship)
                        {
                            relationship.Delete();
                        }
                }
            }
        }

        /// <summary>
        /// Creates a device collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="collectionDescription">The collection description.</param>
        /// <param name="limitingCollection">The limiting collection.</param>
        /// <returns>The id of the collection.</returns>
        public string CreateDeviceCollection(string collectionName, string collectionDescription, string limitingCollection)
        {
            return this.CreateCollection(collectionName, collectionDescription, limitingCollection, SmsCollection.CollectionTypes.Device);
        }

        /// <summary>
        /// Creates a user collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="collectionDescription">The collection description.</param>
        /// <param name="limitingCollection">The limiting collection.</param>
        /// <returns>The id of the collection.</returns>
        public string CreateUserCollection(string collectionName, string collectionDescription, string limitingCollection)
        {
            return this.CreateCollection(collectionName, collectionDescription, limitingCollection, SmsCollection.CollectionTypes.User);
        }

        /// <summary>
        /// Creates the collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="collectionDescription">The collection description.</param>
        /// <param name="limitingCollection">The limiting collection.</param>
        /// <param name="collectionType">Type of the collection.</param>
        /// <returns>
        /// The id of the collection.
        /// </returns>
        public string CreateCollection(string collectionName, string collectionDescription, string limitingCollection, SmsCollection.CollectionTypes collectionType)
        {
            var newCollection = new SmsCollection();
            newCollection.Name = collectionName;
            newCollection.Comment = collectionDescription;
            newCollection.CollectionType = collectionType;
            newCollection.LimitToCollectionName = limitingCollection;
            newCollection.LimitToCollectionId = this.GetCollectionIdByName(limitingCollection);
            using (var collection = newCollection.CreateInstance(this._managementServer))
            {
                return collection["CollectionId"].StringValue;
            }
        }

        /// <summary>
        /// Creates an association between two computers by their names. This function first retrieves
        /// the Resource IDs for both source and destination computers based on their names and then
        /// uses these IDs to create an association in SCCM.
        /// </summary>
        /// <param name="sourceComputerName">The name of the source computer.</param>
        /// <param name="destinationComputerName">The name of the destination computer.</param>
        /// <returns>A string indicating whether the association was created successfully or if there was an error.</returns>
        public bool AddUSMTComputerAssociation(string sourceComputerName, string destinationComputerName)
        {
            int sourceResourceId = GetResourceIdByComputerName(sourceComputerName);
            int destinationResourceId = GetResourceIdByComputerName(destinationComputerName);

            var associationParameters = new Dictionary<string, object>
            {
                {"SourceClientResourceID", sourceResourceId},
                {"RestoreClientResourceID", destinationResourceId}
            };

            using (IResultObject outParameter = this._managementServer.ExecuteMethod("SMS_StateMigration", "AddAssociation", associationParameters))
            {
                int returnValue = outParameter["ReturnValue"].IntegerValue;

                if (returnValue == 0)
                    return true;
                else
                    throw new Exception($"SCCM returned error code {returnValue} when creating association.");
            }
        }

        /// <summary>
        /// Deletes the collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        public void DeleteCollection(string collectionName)
        {
            var collectionId = this.GetCollectionIdByName(collectionName);
            using (var collectionToDelete = this.GetCollection(collectionId))
            {
                // The call to .Get() is necessary because of WMI lazy properties.
                collectionToDelete.Get();
                List<IResultObject> rules = collectionToDelete.GetArrayItems("CollectionRules");
                if (rules.Count > 0)
                {
                    var inParameters = new Dictionary<string, object>();
                    inParameters.Add("CollectionRules", rules);
                    collectionToDelete.ExecuteMethod("DeleteMembershipRules", inParameters);
                }

                this.UpdateCollectionMembership(collectionId);
                collectionToDelete.Delete();
            }
        }

        /// <summary>
        /// Folders the exists.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="folderType">Type of the folder.</param>
        /// <returns><c>True</c> if the folder exists, otherwise <c>false</c>.</returns>
        public bool FolderExists(string folderPath, SmsFolder.FolderObjectType folderType)
        {
            try
            {
                this.GetFolderByPath(folderPath, folderType);
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                // no need to do any error handling just return false
                return false;
            }
        }

        /// <summary>
        /// Gets the folder by path.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="folderType">Type of the folder.</param>
        /// <returns>
        /// The <c>SmsFolder</c>.
        /// </returns>
        public SmsFolder GetFolderByPath(string folderPath, SmsFolder.FolderObjectType folderType)
        {
            var folderNames = folderPath.Split('\\');
            var parentFolderId = 0;
            SmsFolder currentFolder = null;
            foreach (var folderName in folderNames)
            {
                // Complex query that is only used once, so there is no need to make it a const
                var query = "SELECT * FROM SMS_ObjectContainerNode WHERE ObjectType = " + (int)folderType + " AND Name ='" + folderName + "' AND ParentContainerNodeID = " + parentFolderId;
                using (var folders = this._managementServer.QueryProcessor.ExecuteQuery(query))
                {
                    foreach (IResultObject folder in folders) using (folder)
                    {
                        currentFolder = new SmsFolder(folder);
                        parentFolderId = currentFolder.FolderId;
                    }
                }
            }

            if (currentFolder == null || !folderPath.EndsWith(currentFolder.FolderName, StringComparison.CurrentCultureIgnoreCase)) throw new DirectoryNotFoundException("The folder " + folderPath + " does not exist.");
            return currentFolder;
        }

        /// <summary>
        /// Creates the new folder.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="parentFolderPath">The parent folder path.</param>
        /// <param name="folderType">Type of the folder.</param>
        public void CreateFolder(string folderName, string parentFolderPath, SmsFolder.FolderObjectType folderType)
        {
            var parentFolder = parentFolderPath == "0" ? null : this.GetFolderByPath(parentFolderPath, folderType);
            var newFolder = new SmsFolder();
            newFolder.FolderName = folderName;
            newFolder.FolderType = folderType;
            newFolder.ParentFolderId = parentFolder == null ? 0 : parentFolder.FolderId;
            newFolder.CreateInstance(this._managementServer);
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="folderType">Type of the folder.</param>
        public void DeleteFolder(string folderPath, SmsFolder.FolderObjectType folderType)
        {
            var folder = this.GetFolderByPath(folderPath, folderType);
            this.DeleteFolder(folder.FolderId);
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="folderId">The folder id.</param>
        public void DeleteFolder(int folderId)
        {
            var folder = this._managementServer.GetInstance(string.Format(WmiDirectReferenceInt, "SMS_ObjectContainerNode", "ContainerNodeID", folderId));
            if (folder != null) folder.Delete();
        }

        /// <summary>
        /// Moves the item to folder.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="folderType">Type of the folder.</param>
        public void MoveItemToFolder(string objectName, ResourceType resourceType, string sourcePath, string destinationPath, SmsFolder.FolderObjectType folderType)
        {
            var source = sourcePath == "0" ? null : this.GetFolderByPath(sourcePath, folderType);
            var sourceId = source == null ? 0 : source.FolderId;
            var destination = destinationPath == "0" ? null : this.GetFolderByPath(destinationPath, folderType);
            var destinationId = destination == null ? 0 : destination.FolderId;
            var objectId = string.Empty;
            switch (resourceType)
            {
                case ResourceType.Collection:
                    objectId = this.GetCollectionIdByName(objectName);
                    break;
                case ResourceType.Application:
                    objectId = this.GetApplicationId(objectName);
                    break;
            }

            if (string.IsNullOrEmpty(objectId)) throw new Exception(string.Format(ErrorMessageFailedToFindObjectOfType, Enum.GetName(typeof(ResourceType), resourceType), objectName));
            this.MoveItemToFolder(objectId, sourceId, destinationId, folderType);
        }

        /// <summary>
        /// Moves the item to folder.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <param name="sourceContainerId">The source container id.</param>
        /// <param name="destinationContainerId">The destination container id.</param>
        /// <param name="folderType">Type of the folder.</param>
        public void MoveItemToFolder(string objectId, int sourceContainerId, int destinationContainerId, SmsFolder.FolderObjectType folderType)
        {
            var inParameters = new Dictionary<string, object>();
            inParameters.Add("InstanceKeys", new[] { objectId });
            inParameters.Add("ContainerNodeID", sourceContainerId);
            inParameters.Add("TargetContainerNodeID", destinationContainerId);
            inParameters.Add("ObjectType", (int)folderType);
            this._managementServer.ExecuteMethod("SMS_ObjectContainerItem", "MoveMembers", inParameters);
        }

        public List<string> FindCollectionIdsWithSpecificQuery(string stringToSearch)
        {
            var collectionIds = new List<string>();
            using (var collections = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQuery, "SMS_Collection")))
            {
                foreach (IResultObject collection in collections) using (collection)
                {
                    collection.Get();
                    foreach (IResultObject collectionRule in collection.GetArrayItems("CollectionRules")) using (collectionRule)
                    {
                        var query = collectionRule["QueryExpression"].StringValue;
                        if (query.ToLower().Contains(stringToSearch.ToLower()))
                        {
                            collectionIds.Add(collection["CollectionID"].StringValue);
                        }
                    }
                }
            }

            return collectionIds;
        }

        /// <summary>
        /// Removes the name of the member from collection query rule by.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="queryRuleName">Name of the query rule.</param>
        /// <param name="computerName">Name of the computer.</param>
        public void RemoveMemberFromCollectionQueryRuleByName(string collectionId, string queryRuleName, string computerName)
        {
            var members = this.GetMembersOfCollection(collectionId);
            if (members.Any(member => member.ToString(CultureInfo.InvariantCulture).ToUpper().Equals(computerName.ToUpper())))
            {
                using (var collectionRuleQuery = this._managementServer.CreateInstance("SMS_CollectionRuleQuery"))
                using (IResultObject collection = this._managementServer.GetInstance(string.Format(WmiDirectReferenceString, "SMS_Collection", "CollectionID", collectionId)))
                {
                    collection.Get();
                    foreach (IResultObject collectionRule in collection.GetArrayItems("CollectionRules")) using (collectionRule)
                    {
                        if (collectionRule["RuleName"].StringValue.Equals(queryRuleName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var query = collectionRule["QueryExpression"].StringValue;
                            var splitter = new[] { '(', ')' };
                            var firstQueryArray = query.Split(splitter);
                            var newQuery = new StringBuilder(firstQueryArray[0]);
                            var computers = firstQueryArray[1].Split(',').ToList();
                            computers.Sort();
                            newQuery.Append("(");
                            foreach (var computer in computers)
                            {
                                if (!computer.ToUpper().Contains(computerName.ToUpper()))
                                {
                                    newQuery.Append(computer + ",");
                                }
                            }

                            newQuery.Remove(newQuery.Length - 1, 1);
                            newQuery.Append(")");
                            var queryValidationParameters = new Dictionary<string, object>();
                            queryValidationParameters.Add("WQLQuery", newQuery.ToString());
                            if (!this.ValidateQuery(queryValidationParameters))
                            {
                                throw new Exception(string.Format(ErrorMessageInvalidQuery, newQuery));
                            }

                            collectionRuleQuery["QueryExpression"].StringValue = newQuery.ToString();
                            collectionRuleQuery["RuleName"].StringValue = collectionRule["RuleName"].StringValue;
                            try
                            {
                                var inParameters = new Dictionary<string, object>();
                                inParameters.Add("SmsCollectionRule", collectionRule);
                                collection.ExecuteMethod("DeleteMembershipRule", inParameters);
                            }
                            catch (Exception exception)
                            {
                                throw new Exception(string.Format(ErrorMessageFailedDeleteCollectionRule, collectionRule["RuleName"].StringValue, exception.Message));
                            }

                            try
                            {
                                var inParameters = new Dictionary<string, object>();
                                inParameters.Add("SmsCollectionRule", collectionRuleQuery);
                                if (!this.CheckCollectionReadyness(collectionId)) throw new Exception(string.Format(ErrorMessageCollectionNotReady, collection["Name"].StringValue));
                                using (IResultObject result = collection.ExecuteMethod("AddMembershipRule", inParameters))
                                {
                                    if (result["ReturnValue"].IntegerValue != 0) throw new Exception(string.Format(ErrorMessageFailedToModifyMembershipRule, "add", collection["Name"].StringValue, query));
                                }
                            }
                            catch (Exception exception)
                            {
                                throw new Exception(string.Format(ErrorMessageFailedToModifyMemberFromRule, "remove", computerName, collectionRule["RuleName"].StringValue, exception.Message));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds to OSD collection.
        /// </summary>
        /// <param name="collectionId">The OSD collection id.</param>
        /// <param name="queryRuleName">Name of the query rule.</param>
        /// <param name="computerName">Name of the computer.</param>
        public void AddMemberToCollectionQueryRuleByName(string collectionId, string queryRuleName, string computerName)
        {
            var members = this.GetMembersOfCollection(collectionId);
            if (members.Any(member => member.ToString(CultureInfo.InvariantCulture).ToUpper().Equals(computerName.ToUpper()))) return;

            var found = false;
            using (var collectionRuleQuery = this._managementServer.CreateInstance("SMS_CollectionRuleQuery"))
            using (IResultObject collection = this._managementServer.GetInstance(string.Format(WmiDirectReferenceString, "SMS_Collection", "CollectionID", collectionId)))
            {
                collection.Get();
                foreach (IResultObject collectionRule in collection.GetArrayItems("CollectionRules")) using (collectionRule)
                {
                    if (collectionRule["RuleName"].StringValue.Equals(queryRuleName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var query = collectionRule["QueryExpression"].StringValue.Substring(0, collectionRule["QueryExpression"].StringValue.Length - 1) + ",\"" + computerName + "\")";
                        var queryValidationParameters = new Dictionary<string, object>();
                        queryValidationParameters.Add("WQLQuery", query);
                        if (!this.ValidateQuery(queryValidationParameters))
                        {
                            throw new Exception(string.Format(ErrorMessageInvalidQuery, query));
                        }

                        collectionRuleQuery["QueryExpression"].StringValue = query;
                        collectionRuleQuery["RuleName"].StringValue = collectionRule["RuleName"].StringValue;
                        try
                        {
                            var inParameters = new Dictionary<string, object>();
                            inParameters.Add("SmsCollectionRule", collectionRule);
                            collection.ExecuteMethod("DeleteMembershipRule", inParameters);
                        }
                        catch (Exception exception)
                        {
                            throw new Exception(string.Format(ErrorMessageFailedDeleteCollectionRule, collectionRule["RuleName"].StringValue, exception.Message));
                        }

                        try
                        {
                            var inParameters = new Dictionary<string, object>();
                            inParameters.Add("SmsCollectionRule", collectionRuleQuery);
                            if (!this.CheckCollectionReadyness(collectionId)) throw new Exception(string.Format(ErrorMessageCollectionNotReady, collection["Name"].StringValue));
                            using (IResultObject result = collection.ExecuteMethod("AddMembershipRule", inParameters))
                            {
                                if (result["ReturnValue"].IntegerValue != 0) throw new Exception(string.Format(ErrorMessageFailedToModifyMembershipRule, "add", collection["Name"].StringValue, query));
                            }
                        }
                        catch (Exception exception)
                        {
                            throw new Exception(string.Format(ErrorMessageFailedToModifyMemberFromRule, "add", computerName, collectionRule["RuleName"].StringValue, exception.Message));
                        }

                        // function succeeded
                        found = true;
                    }
                }

                // create a new query rule if the target rule does not exist
                if (!found)
                {
                    // Complex query just used once so there is no need to make it a const.
                    var query = "select SMS_R_SYSTEM.ResourceID, SMS_R_SYSTEM.ResourceType, SMS_R_SYSTEM.Name, SMS_R_SYSTEM.SMSUniqueIdentifier, SMS_R_SYSTEM.ResourceDomainORWorkgroup, SMS_R_SYSTEM.Client from SMS_R_System where SMS_R_System.Name in (\"dummyRecord\", \"" + computerName + "\")";
                    var queryValidationParameters = new Dictionary<string, object>();
                    queryValidationParameters.Add("WQLQuery", query);
                    if (!this.ValidateQuery(queryValidationParameters))
                    {
                        throw new Exception(string.Format(ErrorMessageInvalidQuery, query));
                    }

                    collectionRuleQuery["QueryExpression"].StringValue = query;
                    collectionRuleQuery["RuleName"].StringValue = queryRuleName;
                    try
                    {
                        var inParameters = new Dictionary<string, object>();
                        inParameters.Add("SmsCollectionRule", collectionRuleQuery);
                        if (!this.CheckCollectionReadyness(collectionId)) throw new Exception(string.Format(ErrorMessageCollectionNotReady, collection["Name"].StringValue));
                        using (IResultObject result = collection.ExecuteMethod("AddMembershipRule", inParameters))
                        {
                            if (result["ReturnValue"].IntegerValue != 0) throw new Exception(string.Format(ErrorMessageFailedToModifyMembershipRule, "add", collection["Name"].StringValue, query));
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new Exception(string.Format(ErrorMessageFailedToModifyMemberFromRule, "add", computerName, collectionRuleQuery["RuleName"].StringValue, exception.Message));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the collection membership.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns>A <c>List</c> of all collections the client is member of.</returns>
        public List<SmsCollection> GetCollectionMembership(string resourceName, SmsResource.ResourceTypes resourceType)
        {
            SmsResource smsResource = null;
            switch (resourceType)
            {
                case SmsResource.ResourceTypes.User:
                    smsResource = this.GetUser(resourceName);
                    break;
                case SmsResource.ResourceTypes.System:
                    smsResource = this.GetClient(resourceName);
                    break;
            }

            if (smsResource == null) throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindClient, "Name", resourceName));
            var resourceId = smsResource.ResourceId;
            return this.GetCollectionMembership(resourceId);
        }

        /// <summary>
        /// Gets the collection membership.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>A <c>List</c> of all collections the client is member of.</returns>
        public List<SmsCollection> GetCollectionMembership(int clientId)
        {
            var collectionList = new List<SmsCollection>();
            using (var collectionMemberships = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereInteger, "SMS_CollectionMember", "ResourceID", clientId)))
            {
                foreach (IResultObject collectionMembership in collectionMemberships) using (collectionMembership)
                {
                    var classNameArray = collectionMembership.OverridingObjectClass.Split('_');
                    var collectionId = classNameArray[classNameArray.GetUpperBound(0)];
                    var collection = new SmsCollection(this._managementServer, collectionId);
                    collectionList.Add(collection);
                }
            }

            return collectionList;
        }

        public bool IsMemberOfCollection(int clientId, string collectionId)
        {
            var found = false;
            using (var collectionMembers = this._managementServer.QueryProcessor.ExecuteQuery("SELECT * FROM SMS_CM_RES_COLL_" + collectionId))
            {
                foreach (IResultObject collectionMember in collectionMembers) using (collectionMember)
                {
                    if (collectionMember.Properties["ResourceID"].IntegerValue == clientId)
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Gets all collection variables.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>A <c>List</c> with the collection variables.</returns>
        public List<SmsVariables> GetCollectionVariables(string collectionName)
        {
            var resourceId = this.GetCollectionIdByName(collectionName);
            return this.GetSmsVariables(resourceId, ResourceType.Collection);
        }

        /// <summary>
        /// Adds a collection variable.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="value">The value.</param>
        public void AddCollectionVariable(string collectionName, string value)
        {
            var resourceId = this.GetCollectionIdByName(collectionName);
            this.AddSmsVariable(resourceId, value, ResourceType.Collection);
        }

        /// <summary>
        /// Adds a new collection variable.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="name">The name of the new collection variable.</param>
        /// <param name="value">The value of the new collection variable.</param>
        public void AddCollectionVariable(string collectionName, string name, string value)
        {
            var resourceId = this.GetCollectionIdByName(collectionName);
            this.AddSmsVariable(resourceId, name, value, ResourceType.Collection);
        }

        /// <summary>
        /// Removes a collection variable.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="variable">The name of the collection variable.</param>
        public void RemoveCollectionVariable(string collectionName, string variable)
        {
            var resourceId = this.GetCollectionIdByName(collectionName);
            this.RemoveSmsVariable(resourceId, variable, ResourceType.Collection);
        }

        /// <summary>
        /// Removes all collection variables.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        public void RemoveAllCollectionVariables(string collectionName)
        {
            var resourceId = this.GetCollectionIdByName(collectionName);
            this.RemoveAllSmsVariables(resourceId, ResourceType.Collection);
        }

        /// <summary>
        /// Gets all computer variables.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>A <c>List</c> with the computer variables.</returns>
        public List<SmsVariables> GetComputerVariables(string computerName)
        {
            var resourceId = this.GetComputerResourceIdByName(computerName, false).ToString(CultureInfo.InvariantCulture);
            return this.GetSmsVariables(resourceId, ResourceType.Computer);
        }

        /// <summary>
        /// Adds a computer variable for a new computer.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="value">The value.</param>
        public void AddComputerVariableNewComputer(string computerName, string value)
        {
            var resourceId = this.GetComputerResourceIdByName(computerName, true).ToString(CultureInfo.InvariantCulture);
            this.AddSmsVariable(resourceId, value, ResourceType.Computer);
        }

        /// <summary>
        /// Adds a computer variable for a new computer.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="value">The value.</param>
        public void AddComputerVariableNewComputer(string computerName, string name, string value)
        {
            var resourceId = this.GetComputerResourceIdByName(computerName, true).ToString(CultureInfo.InvariantCulture);
            this.AddSmsVariable(resourceId, name, value, ResourceType.Computer);
        }

        /// <summary>
        /// Adds a computer variable.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="value">The value.</param>
        public void AddComputerVariable(string computerName, string value)
        {
            var resourceId = this.GetComputerResourceIdByName(computerName, false).ToString(CultureInfo.InvariantCulture);
            this.AddSmsVariable(resourceId, value, ResourceType.Computer);
        }

        /// <summary>
        /// Adds a new computer variable.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value.</param>
        public void AddComputerVariable(string computerName, string name, string value)
        {
            var resourceId = this.GetComputerResourceIdByName(computerName, false).ToString(CultureInfo.InvariantCulture);
            this.AddSmsVariable(resourceId, name, value, ResourceType.Computer);
        }

        /// <summary>
        /// Removes all computer variables.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        public void RemoveAllComputerVariables(string computerName)
        {
            var resourceId = this.GetComputerResourceIdByName(computerName, false).ToString(CultureInfo.InvariantCulture);
            this.RemoveAllSmsVariables(resourceId, ResourceType.Computer);
        }

        /// <summary>
        /// Removes a computer variable.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="variable">The name of the collection variable.</param>
        public void RemoveComputerVariable(string computerName, string variable)
        {
            var resourceId = this.GetComputerResourceIdByName(computerName, false).ToString(CultureInfo.InvariantCulture);
            this.RemoveSmsVariable(resourceId, variable, ResourceType.Computer);
        }

        /// <summary>
        /// Gets all resource variables.
        /// </summary>
        /// <param name="resourceId">The resource id.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns> A <c>List</c> with the resource variables. </returns>
        public List<SmsVariables> GetSmsVariables(string resourceId, ResourceType resourceType)
        {
            var smsVariableList = new List<SmsVariables>();
            IResultObject resourceSettings = null;
            string variablesItem = string.Empty;
            switch (resourceType)
            {
                case ResourceType.Collection:
                    resourceSettings = this.GetCollectionSettingById(resourceId);
                    variablesItem = "CollectionVariables";
                    break;
                case ResourceType.Computer:
                    resourceSettings = this.GetComputerSettingById(int.Parse(resourceId));
                    variablesItem = "MachineVariables";
                    break;
            }

            if (resourceSettings != null)
            {
                resourceSettings.Get();
                foreach (var resourceSetting in resourceSettings.GetArrayItems(variablesItem)) using (resourceSetting)
                {
                    var smsVariable = new SmsVariables();
                    smsVariable.Name = resourceSetting["Name"].StringValue;
                    smsVariable.Value = resourceSetting["Value"].StringValue;
                    smsVariable.IsMasked = resourceSetting["IsMasked"].BooleanValue;
                    smsVariable.Id = resourceId;
                    smsVariableList.Add(smsVariable);
                }

                resourceSettings.Dispose();
            }

            return smsVariableList;
        }

        /// <summary>
        /// Removes all resource variables.
        /// </summary>
        /// <param name="resourceId">Name of the resource.</param>
        /// <param name="resourceType">Type of the resource.</param>
        public void RemoveAllSmsVariables(string resourceId, ResourceType resourceType)
        {
            IResultObject resourceSettings = null;
            var arrayItemsName = string.Empty;
            switch (resourceType)
            {
                case ResourceType.Collection:
                    resourceSettings = this.GetCollectionSettingById(resourceId);
                    arrayItemsName = "CollectionVariables";
                    break;

                case ResourceType.Computer:
                    resourceSettings = this.GetComputerSettingById(int.Parse(resourceId));
                    arrayItemsName = "MachineVariables";
                    break;
            }

            if (resourceSettings != null)
            {
                var variableList = new List<IResultObject>();
                resourceSettings.SetArrayItems(arrayItemsName, variableList);
                resourceSettings.Put();
                resourceSettings.Dispose();
            }
        }

        /// <summary>
        /// Removes a Sms variable.
        /// </summary>
        /// <param name="resourceId">The resource id.</param>
        /// <param name="variable">The name of the collection variable.</param>
        /// <param name="resourceType">Type of the resource.</param>
        public void RemoveSmsVariable(string resourceId, string variable, ResourceType resourceType)
        {
            var existingVariables = this.GetSmsVariables(resourceId, resourceType);
            
            // first remove the variable
            foreach (var existingVariable in existingVariables.Where(existingVariable => variable.Equals(existingVariable.Name, StringComparison.CurrentCultureIgnoreCase) || variable.Equals(existingVariable.Value, StringComparison.CurrentCultureIgnoreCase)))
            {
                existingVariables.Remove(existingVariable);
                break;
            }

            this.RemoveAllSmsVariables(resourceId, resourceType);

            // second cycle through all variables
            foreach (var existingVariable in existingVariables)
            {
                this.AddSmsVariable(resourceId, existingVariable.Name, existingVariable.Value, resourceType);
            }
                        
            /*
            var counter = 1;
            this.RemoveAllSmsVariables(resourceId, resourceType);

            // second cycle through all variables and rename them
            foreach (var existingVariable in existingVariables)
            {
                var existingVariableName = existingVariable.Name.Substring(0, existingVariable.Name.Length - 2);
                var variableCounter = counter.ToString(CultureInfo.InvariantCulture).Length == 2 ? counter.ToString(CultureInfo.InvariantCulture) : "0" + counter;
                var newVariableName = existingVariableName + variableCounter;
                counter++;
                this.AddSmsVariable(resourceId, newVariableName, existingVariable.Value, resourceType);
            }
            */


        }

        /// <summary>
        /// Removes an association between two computers by their names. This function first retrieves
        /// the Resource IDs for both source and destination computers based on their names and then
        /// uses these IDs to deletes an association in SCCM.
        /// </summary>
        /// <param name="sourceComputerName">The name of the source computer.</param>
        /// <param name="destinationComputerName">The name of the destination computer.</param>
        /// <returns>A string indicating whether the association was deleted successfully or if there was an error.</returns>
        public bool RemoveUSMTComputerAssociation(string sourceComputerName, string destinationComputerName)
        {
            int sourceResourceId = GetResourceIdByComputerName(sourceComputerName);
            int destinationResourceId = GetResourceIdByComputerName(destinationComputerName);

            var associationParameters = new Dictionary<string, object>
            {
                {"SourceClientResourceID", sourceResourceId},
                {"RestoreClientResourceID", destinationResourceId}
            };

            using (IResultObject outParameter = this._managementServer.ExecuteMethod("SMS_StateMigration", "DeleteAssociation", associationParameters))
            {
                int returnValue = outParameter["ReturnValue"].IntegerValue;

                if (returnValue == 0)
                    return true;
                else
                    throw new Exception($"SCCM returned error code {returnValue} when deleting association.");
            }
        }



        /// <summary>
        /// Adds the collection variable.
        /// </summary>
        /// <param name="resourceId">The resource id.</param>
        /// <param name="value">The value.</param>
        /// <param name="resourceType">Type of the resource.</param>
        public void AddSmsVariable(string resourceId, string value, ResourceType resourceType)
        {
            var resourceVariableList = this.GetSmsVariables(resourceId, resourceType);
            if (resourceVariableList != null)
            {
                // get the last variable to get the naming and number
                var lastCollectionVariableName = resourceVariableList[resourceVariableList.Count - 1].Name;
                var name = lastCollectionVariableName.Substring(0, lastCollectionVariableName.Length - 2);
                var number = lastCollectionVariableName.Substring(lastCollectionVariableName.Length - 2, 2);
                var newNumber = number.StartsWith("0") ? int.Parse(number.Substring(1, 1)) : int.Parse(number);
                newNumber++;
                var newName = name + (newNumber.ToString(CultureInfo.InvariantCulture).Length == 2 ? newNumber.ToString(CultureInfo.InvariantCulture) : "0" + newNumber.ToString(CultureInfo.InvariantCulture));
                this.AddSmsVariable(resourceId, newName, value, resourceType);
            }
        }

        /// <summary>
        /// Adds a new sms variable.
        /// </summary>
        /// <param name="resourceId">The resource id.</param>
        /// <param name="name">The name of the new collection variable.</param>
        /// <param name="value">The value of the new collection variable.</param>
        /// <param name="resourceType">Type of the resource.</param>
        public void AddSmsVariable(string resourceId, string name, string value, ResourceType resourceType)
        {
            IResultObject resourceSettings = null;
            switch (resourceType)
            {
                case ResourceType.Collection:
                    resourceSettings = this.GetCollectionSettingById(resourceId);
                    break;
                case ResourceType.Computer:
                    resourceSettings = this.GetComputerSettingById(int.Parse(resourceId));
                    break;
            }

            if (resourceSettings == null)
            {
                switch (resourceType)
                {
                    case ResourceType.Collection:
                        resourceSettings = this._managementServer.CreateInstance("SMS_CollectionSettings");
                        resourceSettings["CollectionId"].StringValue = resourceId;
                        resourceSettings.Put();
                        break;
                    case ResourceType.Computer:
                        resourceSettings = this._managementServer.CreateInstance("SMS_MachineSettings");
                        resourceSettings["ResourceId"].IntegerValue = int.Parse(resourceId);
                        resourceSettings["SourceSite"].StringValue = this._smsSite.SiteCode;
                        resourceSettings["LocaleID"].IntegerValue = 1033;
                        resourceSettings.Put();
                        break;
                }
            }

            IResultObject resourceVariable = null;
            var arrayItemsName = string.Empty;
            switch (resourceType)
            {
                case ResourceType.Collection:
                    resourceVariable = this._managementServer.CreateEmbeddedObjectInstance("SMS_CollectionVariable");
                    arrayItemsName = "CollectionVariables";
                    break;
                case ResourceType.Computer:
                    resourceVariable = this._managementServer.CreateEmbeddedObjectInstance("SMS_MachineVariable");
                    arrayItemsName = "MachineVariables";
                    break;
            }

            if (resourceVariable != null && resourceSettings != null)
            {
                resourceVariable["IsMasked"].BooleanValue = false;
                resourceVariable["Name"].StringValue = name;
                resourceVariable["Value"].StringValue = value;
                var resourceVariableList = new List<IResultObject>();
                resourceSettings.Get();
                var existingResourceVariables = resourceSettings.GetArrayItems(arrayItemsName);
                foreach (IResultObject existingResourceVariable in existingResourceVariables)
                {
                    if (!existingResourceVariable["Name"].StringValue.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        resourceVariableList.Add(existingResourceVariable);
                    }
                }

                resourceVariableList.Add(resourceVariable);
                resourceSettings.SetArrayItems(arrayItemsName, resourceVariableList);
                resourceSettings.Put();
                resourceVariable.Dispose();
                resourceSettings.Dispose();

                // don't forget to cleanup the IResultObjects in the List.
                foreach (var toDispose in resourceVariableList.Where(toDispose => toDispose != null))
                {
                    toDispose.Dispose();
                }
            }
        }

        /// <summary>
        /// Approves the client.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        public void ApproveClient(string clientName)
        {
            var client = this.GetClient(clientName);
            string[] ids = { client.ResourceId.ToString(CultureInfo.InvariantCulture) };
            var resourceIds = new Dictionary<string, object>();
            resourceIds.Add("ResourceIds", ids);
            using (IResultObject result = this._managementServer.ExecuteMethod("SMS_Collection", "ApproveClients", resourceIds))
            {
                if (result["ReturnValue"].IntegerValue != 0)
                {
                    throw new Exception(string.Format(ErrorMessageFailedToApproveClient, clientName));
                }
            }
        }

        /// <summary>
        /// Updates the collection membership.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        public void UpdateCollectionMembership(string collectionId)
        {
            using (IResultObject collection = this._managementServer.GetInstance(string.Format(WmiDirectReferenceString, "SMS_Collection", "CollectionID", collectionId)))
            {
                // var refreshMembershipParams = new Dictionary<string, object>();
                // refreshMembershipParams.Add("IncludeSubCollections", false);
                collection.ExecuteMethod("RequestRefresh", null);
            }
        }

        /// <summary>
        /// Gets the collections.
        /// </summary>
        /// <returns>A <c>List</c> of <see cref="SmsCollection"/>.</returns>
        public List<SmsCollection> GetCollections()
        {
            var result = new List<SmsCollection>();
            using (var collections = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQuery, "SMS_Collection")))
            {
                foreach (WqlResultObject collection in collections) using (collection)
                {
                    result.Add(new SmsCollection(collection));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an <see cref="ArrayList"/> containing all SCCM collections.
        /// </summary>
        /// <returns><see cref="ArrayList"/> with all collections.</returns>
        public List<string> GetCollectionNames()
        {
            var result = new List<string>();
            using (var collections = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQuery, "SMS_Collection")))
            {
                foreach (IResultObject collection in collections) using (collection)
                {
                    result.Add(collection["Name"].StringValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the id of a collection by its name.
        /// </summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>The id of the collection.</returns>
        public string GetCollectionIdByName(string collectionName)
        {
            using (IResultObject collection = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_Collection", "Name", collectionName)))
            {
                foreach (IResultObject currentCollection in collection) using (currentCollection)
                {
                    return currentCollection["CollectionID"].StringValue;
                }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindCollection, "name", collectionName));
        }

        /// <summary>
        /// Gets an <see cref="ArrayList"/> containing all members of a collection.
        /// </summary>
        /// <param name="collectionId">The id of the collection.</param>
        /// <returns><see cref="ArrayList"/> With all members.</returns>
        public List<string> GetMembersOfCollection(string collectionId)
        {
            var result = new List<string>();
            using (var collection = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_FullCollectionMembership", "CollectionID", collectionId)))
            {
                foreach (WqlResultObject member in collection) using (member)
                {
                    result.Add(member["Name"].StringValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the collection id by deployment id.
        /// </summary>
        /// <param name="deploymentId">The deployment id.</param>
        /// <returns>Returns the collection id.</returns>
        public string GetCollectionIdByDeploymentId(string deploymentId)
        {
            using (IResultObject collection = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_DeploymentSummary", "DeploymentID", deploymentId)))
            {
                foreach (IResultObject item in collection) using (item)
                {
                    return item["CollectionID"].StringValue;
                }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindCollection, "deployment id", deploymentId));
        }

        /// <summary>
        /// Adds a client to a collection.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="clientName">Name of the client.</param>
        public void AddMemberToCollectionDirectByName(string collectionId, string clientName)
        {
            using (var collection = this.GetCollection(collectionId))
            {
                var client = this.GetClient(clientName);
                this.ChangeMemberInCollectionDirect(collection, client, CollectionAction.AddMembershipRule);
            }
        }

        /// <summary>
        /// Removes a client from a collection.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="clientName">The name of the client.</param>
        public void RemoveMemberFromCollectionDirectByName(string collectionId, string clientName)
        {
            using (var collection = this.GetCollection(collectionId))
            {
                var client = this.GetClient(clientName);
                this.ChangeMemberInCollectionDirect(collection, client, CollectionAction.DeleteMembershipRule);
            }
        }

        /// <summary>
        /// Adds a client to a collection.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="clientId">The id of the client.</param>
        public void AddMemberToCollectionDirectById(string collectionId, string clientId)
        {
            using (var collection = this.GetCollection(collectionId))
            {
                var client = this.GetClientByGuid(clientId);
                this.ChangeMemberInCollectionDirect(collection, client, CollectionAction.AddMembershipRule);
            }
        }

        /// <summary>
        /// Removes a client from a collection.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="clientId">The id of the client.</param>
        public void RemoveMemberFromCollectionDirectById(string collectionId, string clientId)
        {
            using (var collection = this.GetCollection(collectionId)) 
            {
                var client = this.GetClientByGuid(clientId);
                this.ChangeMemberInCollectionDirect(collection, client, CollectionAction.DeleteMembershipRule);
            }
        }

        public int AddNewComputerBySmBiosGuid(string computerName, string smBiosGuid)
        {
            return this.AddNewComputer(computerName, smBiosGuid, AddComputerTypes.SmBiosGuid);
        }

        public int AddNewComputerByMacAdress(string computerName, string macAddress)
        {
            return this.AddNewComputer(computerName, macAddress, AddComputerTypes.MacAddress);
        }

        /// <summary>
        /// Adds the new computer.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="macAddressOrBiosGuid">The mac address or Bios GUID.</param>
        /// <returns>The resource id of the newly added computer.</returns>
        public int AddNewComputer(string computerName, string macAddressOrBiosGuid, AddComputerTypes addComputerType)
        {
            if (string.IsNullOrEmpty(macAddressOrBiosGuid)) throw new ArgumentNullException(ErrorMessageNoMacOrBiosGuid);
            var computerParameter = new Dictionary<string, object>();
            computerParameter.Add("NetbiosName", computerName);

            switch (addComputerType)
            {
                case AddComputerTypes.MacAddress:
                    var macAddress = macAddressOrBiosGuid.Replace("-", ":");
                    computerParameter.Add("MACAddress", macAddress);
                    break;
                case AddComputerTypes.SmBiosGuid:
                    computerParameter.Add("SMBIOSGUID", macAddressOrBiosGuid);
                    break;
            }
            
            computerParameter.Add("OverwriteExistingRecord", false);
            using (IResultObject outParameter = this._managementServer.ExecuteMethod("SMS_Site", "ImportMachineEntry", computerParameter))
            {
                return outParameter["ResourceID"].IntegerValue;
            }
        }

        /// <summary>
        /// Deletes the name of the computer by.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>The number of objects that have been deleted.</returns>
        public int DeleteComputerByName(string computerName)
        {
            return this.DeleteComputer(computerName, ComputerIdType.ComputerName);
        }

        /// <summary>
        /// Deletes the computer by sm bios GUID.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>The number of objects that have been deleted.</returns>
        public int DeleteComputerBySmBiosGuid(string computerName)
        {
            return this.DeleteComputer(computerName, ComputerIdType.SmBiosGuid);
        }

        /// <summary>
        /// Clears the last PXE advertisement.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        public void ClearLastPxeAdvertisement(string computerName)
        {
            var computer = this.GetClient(computerName);
            var inParams = new Dictionary<string, object>();
            var resourceIds = new List<int>();
            resourceIds.Add(computer.ResourceId);
            inParams.Add("ResourceIDs", resourceIds.ToArray());
            using (IResultObject outParams = this._managementServer.ExecuteMethod("SMS_Collection", "ClearLastNBSAdvForMachines", inParams))
            {
                if (outParams == null || outParams["StatusCode"].IntegerValue != 0)
                {
                    throw new Exception(string.Format(ErrorMessageFailedToClearPxeAdv, computerName));
                }
            }
        }

        /// <summary>
        /// Gets the SMS GUID.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>The SMS GUID.</returns>
        public string GetSmsGuid(string computerName)
        {
            var computer = this.GetClient(computerName);
            return computer.SmsUniqueIdentifier;
        }

        /// <summary>
        /// Gets the name of the computer resource id by.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>The resource id of the computer.</returns>
        public int GetComputerResourceIdByName(string computerName)
        {
            return this.GetComputerResourceIdByName(computerName, true);
        }

        /// <summary>
        /// Gets the name of the computer resource id by.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="searchAllClients">If set to <c>true</c> [search all clients].</param>
        /// <returns>The resource id of the computer.</returns>
        public int GetComputerResourceIdByName(string computerName, bool searchAllClients)
        {
            var computer = this.GetClient(computerName, searchAllClients);
            return computer.ResourceId;
        }

        /// <summary>
        /// Gets the application id.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>The id of the application.</returns>
        public string GetApplicationId(string applicationName)
        {
            using (var application = this.GetApplicationByName(applicationName))
            {
                return application["CI_ID"].StringValue;
            }
        }

        /// <summary>
        /// Gets the deployments.
        /// </summary>
        /// <param name="scopeName">Name of the application.</param>
        /// <param name="deploymentScope">The deployment scope.</param>
        /// <returns>
        /// A List containing all deployment id's.
        /// </returns>
        public Dictionary<string, string> GetDeploymentsByScopeName(string scopeName, DeploymentScopeTypes deploymentScope)
        {
            var deploymentList = new Dictionary<string, string>();

            var deploymentScopeId = string.Empty;
            var identifier = string.Empty;
            switch (deploymentScope)
            {
                case DeploymentScopeTypes.Application:
                    identifier = "CI_ID";
                    deploymentScopeId = this.GetApplicationId(scopeName);
                    break;
                case DeploymentScopeTypes.Tasksequence:
                    identifier = "PackageID";
                    deploymentScopeId = this.GetTasksequenceIdByName(scopeName);
                    break;
            }

            using (var deployments = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_DeploymentSummary", identifier, deploymentScopeId)))
            {
                foreach (WqlResultObject deployment in deployments) using (deployment)
                {
                    deploymentList.Add(deployment["DeploymentID"].StringValue, deployment["CollectionName"].StringValue);
                }
            }

            return deploymentList;
        }

        /// <summary>
        /// Gets all deployments.
        /// </summary>
        /// <returns>
        /// Gets a <c>List</c> of all deployments.
        /// </returns>
        public List<SmsApplicationAssignment> ListDeployments()
        {
            var deploymentList = new List<SmsApplicationAssignment>();
            using (var deployments = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQuery, "SMS_ApplicationAssignment")))
            {
                foreach (WqlResultObject deployment in deployments) using (deployment)
                {
                    var deploymentObject = new SmsApplicationAssignment(deployment);
                    deploymentList.Add(deploymentObject);
                }
            }

            return deploymentList;
        }

        /// <summary>
        /// Checks if a deployment of a collection exists for an application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>True if the deployment exists, otherwise false.</returns>
        public bool DeploymentExistForApplication(string applicationName, string collectionName)
        {
            var deploymentList = this.GetDeploymentsByScopeName(applicationName, DeploymentScopeTypes.Application);
            return deploymentList.ContainsValue(collectionName);
        }

        /// <summary>
        /// Deployments the exist for tasksequence.
        /// </summary>
        /// <param name="tasksequenceName">Name of the tasksequence.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>True if the deployment exists, otherwise false.</returns>
        public bool DeploymentExistForTasksequence(string tasksequenceName, string collectionName)
        {
            var deploymentList = this.GetDeploymentsByScopeName(tasksequenceName, DeploymentScopeTypes.Tasksequence);
            return deploymentList.ContainsValue(collectionName);
        }

        /// <summary>
        /// Deploys an application.
        /// </summary>
        /// <param name="deploymentName">Name of the deployment.</param>
        /// <param name="deploymentDescription">The deployment description.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationId">The application id.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="assignmentAction">The assignment action.</param>
        /// <param name="desiredConfigType">Type of the desired config.</param>
        /// <param name="offerType">Type of the offer.</param>
        /// <param name="offerFlag">The offer flag.</param>
        /// <param name="distributionPointLocality">The distribution point locality.</param>
        /// <param name="priorityType">The Priority.</param>
        /// <param name="suppressRebootType">Type of the suppress reboot.</param>
        /// <param name="stateMessagePrioritity">The state message prioritity.</param>
        /// <param name="localeId">The locale id.</param>
        /// <param name="userNotificationBehaviour">The user notification behaviour.</param>
        /// <param name="overrideServiceWindow">If set to <c>true</c> [override service window].</param>
        /// <param name="rebootOutsideOfServiceWindow">If set to <c>true</c> [reboot outside of service window].</param>
        /// <param name="applyToSubTargets">If set to <c>true</c> [apply to sub targets].</param>
        /// <param name="containsExpiredUpdates">If set to <c>true</c> [contains expired updates].</param>
        /// <param name="updateSupersedence">If set to <c>true</c> [update supersedence].</param>
        /// <param name="wolEnabled">If set to <c>true</c> [wol enabled].</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="enforcementDeadline">The envorcement deadline.</param>
        /// <returns>
        /// The AssignmentId of the deployment.
        /// </returns>
        public int DeployApplication(
                                        string deploymentName,
                                        string deploymentDescription,
                                        string applicationName,
                                        int applicationId,
                                        string collectionName,
                                        string collectionId,
                                        SmsApplicationAssignment.AssignmentActions assignmentAction,
                                        SmsApplicationAssignment.DesiredConfigTypes desiredConfigType,
                                        SmsAssignmentBase.OfferTypeTypes offerType,
                                        SmsApplicationAssignment.OfferFlags offerFlag,
                                        SmsApplicationAssignment.DpLocalities distributionPointLocality,
                                        SmsApplicationAssignment.PriorityTypes priorityType,
                                        SmsApplicationAssignment.SuppressRebooTypes suppressRebootType,
                                        SmsApplicationAssignment.StateMessagePrioritites stateMessagePrioritity,
                                        SmsApplicationAssignment.LocaleIds localeId,
                                        SmsApplicationAssignment.UserNotificationBehaviourTypes userNotificationBehaviour,
                                        bool overrideServiceWindow,
                                        bool rebootOutsideOfServiceWindow,
                                        bool applyToSubTargets,
                                        bool containsExpiredUpdates,
                                        bool updateSupersedence,
                                        bool wolEnabled,
                                        DateTime startTime,
                                        DateTime enforcementDeadline)
        {
            using (var deplyoment = this.CreateDeployment(
                                                            deploymentName,
                                                            deploymentDescription,
                                                            applicationName,
                                                            applicationId,
                                                            collectionName,
                                                            collectionId,
                                                            assignmentAction,
                                                            desiredConfigType,
                                                            offerType,
                                                            offerFlag,
                                                            distributionPointLocality,
                                                            priorityType,
                                                            suppressRebootType,
                                                            stateMessagePrioritity,
                                                            localeId,
                                                            userNotificationBehaviour,
                                                            overrideServiceWindow,
                                                            rebootOutsideOfServiceWindow,
                                                            applyToSubTargets,
                                                            containsExpiredUpdates,
                                                            updateSupersedence,
                                                            wolEnabled,
                                                            startTime,
                                                            enforcementDeadline))
            {
                return deplyoment["AssignmentID"].IntegerValue;
            }
        }

        /// <summary>
        /// Deploys an application.
        /// </summary>
        /// <param name="deploymentName">Name of the deployment.</param>
        /// <param name="deploymentDescription">The deployment description.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="assignmentAction">The assignment action.</param>
        /// <param name="desiredConfigType">Type of the desired config.</param>
        /// <param name="offerType">Type of the offer.</param>
        /// <param name="offerFlag">The offer flag.</param>
        /// <param name="distributionPointLocality">The distribution point locality.</param>
        /// <param name="priorityType">The Priority.</param>
        /// <param name="suppressRebootType">Type of the suppress reboot.</param>
        /// <param name="stateMessagePrioritity">The state message prioritity.</param>
        /// <param name="localeId">The locale id.</param>
        /// <param name="userNotificationBehaviour">The user notification behaviour.</param>
        /// <param name="overrideServiceWindow">If set to <c>true</c> [override service window].</param>
        /// <param name="rebootOutsideOfServiceWindow">If set to <c>true</c> [reboot outside of service window].</param>
        /// <param name="applyToSubTargets">If set to <c>true</c> [apply to sub targets].</param>
        /// <param name="containsExpiredUpdates">If set to <c>true</c> [contains expired updates].</param>
        /// <param name="updateSupersedence">If set to <c>true</c> [update supersedence].</param>
        /// <param name="wolEnabled">If set to <c>true</c> [wol enabled].</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="enforcementDeadline">The envorcement deadline.</param>
        /// <returns>
        /// The AssignmentId of the deployment.
        /// </returns>
        public int DeployApplication(
                                string deploymentName,
                                string deploymentDescription,
                                string applicationName,
                                string collectionName,
                                SmsApplicationAssignment.AssignmentActions assignmentAction,
                                SmsApplicationAssignment.DesiredConfigTypes desiredConfigType,
                                SmsAssignmentBase.OfferTypeTypes offerType,
                                SmsApplicationAssignment.OfferFlags offerFlag,
                                SmsApplicationAssignment.DpLocalities distributionPointLocality,
                                SmsApplicationAssignment.PriorityTypes priorityType,
                                SmsApplicationAssignment.SuppressRebooTypes suppressRebootType,
                                SmsApplicationAssignment.StateMessagePrioritites stateMessagePrioritity,
                                SmsApplicationAssignment.LocaleIds localeId,
                                SmsApplicationAssignment.UserNotificationBehaviourTypes userNotificationBehaviour,
                                bool overrideServiceWindow,
                                bool rebootOutsideOfServiceWindow,
                                bool applyToSubTargets,
                                bool containsExpiredUpdates,
                                bool updateSupersedence,
                                bool wolEnabled,
                                DateTime startTime,
                                DateTime enforcementDeadline)
        {
            var applicationId = int.Parse(this.GetApplicationId(applicationName));
            var collectionId = this.GetCollectionIdByName(collectionName);
            return this.DeployApplication(
                                            deploymentName,
                                            deploymentDescription,
                                            applicationName,
                                            applicationId,
                                            collectionName,
                                            collectionId,
                                            assignmentAction,
                                            desiredConfigType,
                                            offerType,
                                            offerFlag,
                                            distributionPointLocality,
                                            priorityType,
                                            suppressRebootType,
                                            stateMessagePrioritity,
                                            localeId,
                                            userNotificationBehaviour,
                                            overrideServiceWindow,
                                            rebootOutsideOfServiceWindow,
                                            applyToSubTargets,
                                            containsExpiredUpdates,
                                            updateSupersedence,
                                            wolEnabled,
                                            startTime,
                                            enforcementDeadline);
        }

        /// <summary>
        /// Deploys an application.
        /// </summary>
        /// <param name="deploymentName">Name of the deployment.</param>
        /// <param name="deploymentDescription">The deployment description.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationId">The application id.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="assignmentAction">The assignment action.</param>
        /// <param name="desiredConfigType">Type of the desired config.</param>
        /// <param name="offerType">Type of the offer.</param>
        /// <param name="offerFlag">The offer flag.</param>
        /// <param name="distributionPointLocality">The dp locality.</param>
        /// <param name="priorityType">The Priority.</param>
        /// <param name="suppressRebootType">Type of the suppress reboot.</param>
        /// <param name="stateMessagePrioritity">The state message prioritity.</param>
        /// <param name="localeId">The locale id.</param>
        /// <param name="userNotificationBehaviour">The user notification behaviour.</param>
        /// <param name="overrideServiceWindow">If set to <c>true</c> [override service window].</param>
        /// <param name="rebootOutsideOfServiceWindow">If set to <c>true</c> [reboot outside of service window].</param>
        /// <param name="applyToSubTargets">If set to <c>true</c> [apply to sub targets].</param>
        /// <param name="containsExpiredUpdates">If set to <c>true</c> [contains expired updates].</param>
        /// <param name="updateSupersedence">If set to <c>true</c> [update supersedence].</param>
        /// <param name="wolEnabled">If set to <c>true</c> [wol enabled].</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="enforcementDeadline">The envorcement deadline.</param>
        /// <returns>
        /// The new created <c>IResultObject</c>.
        /// </returns>
        public IResultObject CreateDeployment(
                                                string deploymentName,
                                                string deploymentDescription,
                                                string applicationName,
                                                int applicationId,
                                                string collectionName,
                                                string collectionId,
                                                SmsApplicationAssignment.AssignmentActions assignmentAction,
                                                SmsApplicationAssignment.DesiredConfigTypes desiredConfigType,
                                                SmsAssignmentBase.OfferTypeTypes offerType,
                                                SmsApplicationAssignment.OfferFlags offerFlag,
                                                SmsApplicationAssignment.DpLocalities distributionPointLocality,
                                                SmsApplicationAssignment.PriorityTypes priorityType,
                                                SmsApplicationAssignment.SuppressRebooTypes suppressRebootType,
                                                SmsApplicationAssignment.StateMessagePrioritites stateMessagePrioritity,
                                                SmsApplicationAssignment.LocaleIds localeId,
                                                SmsApplicationAssignment.UserNotificationBehaviourTypes userNotificationBehaviour,
                                                bool overrideServiceWindow,
                                                bool rebootOutsideOfServiceWindow,
                                                bool applyToSubTargets,
                                                bool containsExpiredUpdates,
                                                bool updateSupersedence,
                                                bool wolEnabled,
                                                DateTime startTime,
                                                DateTime enforcementDeadline)
        {
            var deployment = new SmsApplicationAssignment();
            deployment.AssignmentName = deploymentName;
            deployment.AssignmentDescription = deploymentDescription;
            deployment.ApplicationName = applicationName;
            deployment.CollectionName = collectionName;
            deployment.TargetCollectionId = collectionId;
            deployment.AssignedCis = new[] { applicationId };
            deployment.AssignmentAction = assignmentAction;
            deployment.DesiredConfigType = desiredConfigType;
            deployment.OfferType = offerType;
            deployment.OfferFlag = offerFlag;
            deployment.DpLocality = distributionPointLocality;
            deployment.Priority = priorityType;
            deployment.SuppressReboot = suppressRebootType;
            deployment.LocalId = localeId;
            deployment.OverrideServiceWindows = overrideServiceWindow;
            deployment.RebootOutsideOfServiceWindows = rebootOutsideOfServiceWindow;
            deployment.ApplyToSubTargets = applyToSubTargets;
            deployment.ContainsExpiredUpdates = containsExpiredUpdates;
            deployment.UpdateSupersedence = updateSupersedence;
            deployment.WoLEnabled = wolEnabled;
            deployment.StateMessagePriority = stateMessagePrioritity;
            deployment.StartTime = startTime;
            deployment.EnforcementDeadline = enforcementDeadline;
            //// the next few values will mostly not change
            deployment.AssignmentType = SmsApplicationAssignment.AssignmentTypes.CiaTypeApplication;
            deployment.CreationTime = DateTime.Now;
            deployment.DisableMomAlerts = false;
            deployment.Enabled = true;
            deployment.LogComplianceToWinEvent = false;
            deployment.RaiseMomAlertsOnFailure = false;
            deployment.RequireApproval = false;
            deployment.SendDetailedNonComplianceStatus = false;
            deployment.SourceSite = this._smsSite.SiteCode;
            deployment.UseGmtTimes = false;
            //// user notification behaviour
            switch (userNotificationBehaviour)
            {
                case SmsApplicationAssignment.UserNotificationBehaviourTypes.UserNotificationHideInSoftwareCenter:
                    deployment.UserUiExperience = false;
                    deployment.NotifyUser = false;
                    break;
                case SmsApplicationAssignment.UserNotificationBehaviourTypes.UserNotificationDisplayInSoftwareCenterAllNotifications:
                    deployment.UserUiExperience = true;
                    deployment.NotifyUser = true;
                    break;
                case SmsApplicationAssignment.UserNotificationBehaviourTypes.UserNotificationDisplayInSoftwareCenterAndRebootsOnly:
                    deployment.UserUiExperience = true;
                    deployment.NotifyUser = false;
                    break;
            }

            var deploymentObject = deployment.CreateInstance(this._managementServer);
            return deploymentObject;
        }

        /// <summary>
        /// Deploys the tasksequence.
        /// </summary>
        /// <param name="advertisementName">Name of the advertisement.</param>
        /// <param name="advertisementComment">The advertisement comment.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tasksequenceName">Name of the tasksequence.</param>
        /// <param name="offerType">Type of the offer.</param>
        /// <param name="showTasksequenceProgress">If set to <c>true</c> [show tasksequence progress].</param>
        /// <param name="forceReboot">If set to <c>true</c> [force reboot].</param>
        /// <param name="forceInstallation">If set to <c>true</c> [force installation].</param>
        /// <param name="showInAdvertList">If set to <c>true</c> [show in advert list].</param>
        /// <param name="wolEnabled">If set to <c>true</c> [wol enabled].</param>
        /// <param name="networkBehaviour">The network behaviour.</param>
        /// <param name="rerunBehaviour">The rerun behaviour.</param>
        /// <param name="downloadBehaviour">The download behaviour.</param>
        /// <param name="availableTime">The available time.</param>
        /// <param name="assignmentTime">The assignment time.</param>
        /// <param name="expirationTime">The expiration time.</param>
        /// <param name="mandatoryCountdown">The mandatory countdown.</param>
        /// <returns>
        /// The advertisement id.
        /// </returns>
        public string DeployTasksequence(
                                        string advertisementName,
                                        string advertisementComment,
                                        string collectionName,
                                        string tasksequenceName,
                                        SmsAssignmentBase.OfferTypeTypes offerType,
                                        bool showTasksequenceProgress,
                                        bool forceReboot,
                                        bool forceInstallation,
                                        bool showInAdvertList,
                                        bool wolEnabled,
                                        NetworkBehaviourTypes networkBehaviour,
                                        RerunBehaviourTypes rerunBehaviour,
                                        DownloadBehaviourTypes downloadBehaviour,
                                        DateTime availableTime,
                                        DateTime assignmentTime,
                                        DateTime expirationTime,
                                        int mandatoryCountdown)
        {
            var tasksequenceId = this.GetTasksequenceIdByName(tasksequenceName);
            var collectionId = this.GetCollectionIdByName(collectionName);

            var deviceFlags = new DeviceFlags();
            switch (networkBehaviour)
            {
                case NetworkBehaviourTypes.DownloadOnAllNetworks:
                    deviceFlags = DeviceFlags.AllNetwork;
                    break;
            }

            using (var advertisement = this.CreateAdvertisement(
                                                                advertisementName,
                                                                advertisementComment, 
                                                                collectionId, 
                                                                tasksequenceId, 
                                                                offerType, 
                                                                showTasksequenceProgress, 
                                                                forceReboot, 
                                                                forceInstallation, 
                                                                showInAdvertList, 
                                                                true, 
                                                                false, 
                                                                wolEnabled, 
                                                                deviceFlags, 
                                                                rerunBehaviour, 
                                                                downloadBehaviour, 
                                                                availableTime, 
                                                                assignmentTime, 
                                                                expirationTime, 
                                                                mandatoryCountdown))
            {
                return advertisement["AdvertisementID"].StringValue;
            }
        }

        /// <summary>
        /// Creates the advertisement.
        /// </summary>
        /// <param name="advertisementName">Name of the advertisement.</param>
        /// <param name="advertisementComment">The advertisement comment.</param>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="packageId">The package id.</param>
        /// <param name="offerType">Type of the offer.</param>
        /// <param name="showTasksequenceProgress">If set to <c>true</c> [show tasksequence progress].</param>
        /// <param name="forceReboot">If set to <c>true</c> [force reboot].</param>
        /// <param name="forceInstallation">If set to <c>true</c> [force installation].</param>
        /// <param name="showInAdvertList">If set to <c>true</c> [show in advert list].</param>
        /// <param name="alsoInternet">If set to <c>true</c> [also internet].</param>
        /// <param name="applyToSubTargets">If set to <c>true</c> [apply to sub targets].</param>
        /// <param name="wolEnabled">If set to <c>true</c> [wol enabled].</param>
        /// <param name="deviceFlags">The device flags.</param>
        /// <param name="rerunBehaviour">The rerun behaviour.</param>
        /// <param name="downloadBehaviour">The download behaviour.</param>
        /// <param name="availableTime">The available time.</param>
        /// <param name="assignmentTime">The assignment time.</param>
        /// <param name="expirationTime">The expiration time.</param>
        /// <param name="mandatoryCountdown">The mandatory countdown.</param>
        /// <returns>
        /// The new created <c>IResultObject</c>.
        /// </returns>
        public IResultObject CreateAdvertisement(
                                                        string advertisementName, 
                                                        string advertisementComment, 
                                                        string collectionId, 
                                                        string packageId, 
                                                        SmsAssignmentBase.OfferTypeTypes offerType, 
                                                        bool showTasksequenceProgress, 
                                                        bool forceReboot, 
                                                        bool forceInstallation,
                                                        bool showInAdvertList,
                                                        bool alsoInternet,
                                                        bool applyToSubTargets,
                                                        bool wolEnabled,
                                                        DeviceFlags deviceFlags,
                                                        RerunBehaviourTypes rerunBehaviour,
                                                        DownloadBehaviourTypes downloadBehaviour,
                                                        DateTime availableTime,
                                                        DateTime assignmentTime,
                                                        DateTime expirationTime,
                                                        int mandatoryCountdown)
        {
            var advertisement = new SmsAdvertisement();
            advertisement.CollectionId = collectionId;
            advertisement.AdvertisementName = advertisementName;
            advertisement.Comment = advertisementComment;
            advertisement.PackageId = packageId;
            advertisement.OfferType = offerType;
            advertisement.Priority = AdvertisementPriority.Normal;

            var advertisementFlags = new AdvertisementFlags();
            if (showTasksequenceProgress) advertisementFlags |= AdvertisementFlags.ShowTaskSequenceProgress;
            if (forceReboot) advertisementFlags |= AdvertisementFlags.RebootOutsideOfServiceWindows;
            if (forceInstallation) advertisementFlags |= AdvertisementFlags.OverrideServiceWindows;
            if (showInAdvertList) advertisementFlags |= AdvertisementFlags.NoDisplay;
            if (!alsoInternet) advertisementFlags |= AdvertisementFlags.APTSIntranetOnly;
            if (wolEnabled) advertisementFlags |= AdvertisementFlags.WakeOnLan;

            // don't know what virtual offer is... but is set in the template
            advertisementFlags |= AdvertisementFlags.VirtualOffer;

            advertisement.AdvertFlags = advertisementFlags;

            var advertisementRemoteClientFlags = new AdvertisementRemoteClientFlags();
            switch (rerunBehaviour)
            {
                case RerunBehaviourTypes.RerunNever:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.RerunNever;
                    break;
                case RerunBehaviourTypes.RerunAlways:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.RerunAlways;
                    break;
                case RerunBehaviourTypes.RerunIfSucceeded:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.RerunIfSucceeded;
                    break;
                case RerunBehaviourTypes.RerunIfFailed:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.RerunIfFailed;
                    break;
            }

            switch (downloadBehaviour)
            {
                case DownloadBehaviourTypes.DownloadLocalAndRemoteDp:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.DownloadFromLocalDisppoint | AdvertisementRemoteClientFlags.DownloadFromRemoteDisppoint;
                    break;
                case DownloadBehaviourTypes.DownloadOnlyFromLocalDp:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.DownloadFromLocalDisppoint;
                    break;
                case DownloadBehaviourTypes.DownloadOnDemandLocalAndRemoteDp:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.DownloadOnDemandFromLocalDP | AdvertisementRemoteClientFlags.DownloadOnDemandFromRemoteDP;
                    break;
                case DownloadBehaviourTypes.DownloadOnDemandOnlyFromLocalDp:
                    advertisementRemoteClientFlags |= AdvertisementRemoteClientFlags.DownloadOnDemandFromLocalDP;
                    break;
            }

            advertisement.RemoteClientFlags = advertisementRemoteClientFlags;

            if (!assignmentTime.Equals(DateTime.MinValue))
            {
                advertisement.AssignmentScheduleEnabled = true;
                advertisement.AssignmentScheduleIsGmt = false;
                var scheduleToken = new SmsScheduleToken();
                scheduleToken.StartTime = assignmentTime;
                scheduleToken.ScheduleToken = SmsScheduleToken.ScheduleTokenTypes.SmsStNoRecurring;
                advertisement.AssigmentSchedule = scheduleToken;
            }

            if (!expirationTime.Equals(DateTime.MinValue))
            {
                advertisement.ExpirationTimeEnabled = true;
                advertisement.ExpirationTimeIsGmt = false;
                advertisement.ExpirationTime = expirationTime;
            }

            if (!availableTime.Equals(DateTime.MinValue))
            {
                advertisement.PresentTimeEnabled = true;
                advertisement.PresentTimeIsGmt = false;
                advertisement.PresentTime = availableTime;
            }

            advertisement.MandatoryCountdown = mandatoryCountdown;

            advertisement.DeviceFlag = deviceFlags;
            advertisement.ProgramName = "*";
            var advertisementObject = advertisement.CreateInstance(this._managementServer);

            return advertisementObject;
        }

        /// <summary>
        /// Gets the advertisement.
        /// </summary>
        /// <param name="advertisementId">The advertisement id.</param>
        /// <returns>The <c>SmsAdvertisement.</c></returns>
        public SmsAdvertisement GetAdvertisement(string advertisementId)
        {
            return new SmsAdvertisement(this._managementServer, advertisementId);
        }

        /// <summary>
        /// Gets the advertisements.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <returns>A <c>List</c> with all advertisements.</returns>
        public List<SmsAdvertisement> GetAdvertisementsByClient(string clientName)
        {
            var client = this.GetClient(clientName);
            return this.GetAdvertisementsByClient(client.ResourceId);
        }

        /// <summary>
        /// Gets the advertisements.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>A <c>List</c> with all advertisements.</returns>
        public List<SmsAdvertisement> GetAdvertisementsByClient(int clientId)
        {
            var advertisementList = new List<SmsAdvertisement>();
            using (var advertisements = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereInteger, "SMS_ClientAdvertisementStatus", "ResourceId", clientId)))
            {
                foreach (IResultObject advertisement in advertisements) using (advertisement)
                {
                    var found = false;

                    // perform a check if the advertisement still exists
                    using (IResultObject advertisementsToCheck = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_Advertisement", "AdvertisementID", advertisement["AdvertisementID"].StringValue)))
                    {
                        // Tried with advertisementsToCheck.Count, but it does not work with IResultObject -> everytime NotImplementedException
                        // the foreach loop must be done... a Count(*) has the same issue...
                        foreach (IResultObject advertisementToCheck in advertisementsToCheck) using (advertisementToCheck)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found) continue;
                    var smsAdvertisement = new SmsAdvertisement(this._managementServer, advertisement["AdvertisementID"].StringValue);
                    advertisementList.Add(smsAdvertisement);
                }
            }

            return advertisementList;
        }

        /// <summary>
        /// Gets the advertisements by collection.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <returns>A <c>List</c> of all <c>SmsAdvertisement</c> for this collection.</returns>
        public List<SmsAdvertisement> GetAdvertisementsByCollection(string collectionId)
        {
            var advertisementList = new List<SmsAdvertisement>();
            using (var advertisements = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_Advertisement", "CollectionID", collectionId)))
            {
                foreach (IResultObject advertisement in advertisements) using (advertisement)
                {
                    var smsAdvertisement = new SmsAdvertisement(advertisement);
                    advertisementList.Add(smsAdvertisement);
                }
            }

            return advertisementList;
        }

        /// <summary>
        /// Advertisements the exists for client.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="advertisementId">The advertisement id.</param>
        /// <returns><c>True</c> if the client has the specific advertisement, otherwise <c>false</c>.</returns>
        public bool AdvertisementExistsForClient(string clientName, string advertisementId)
        {
            var advertisementList = this.GetAdvertisementsByClient(clientName);
            return advertisementList.Any(advertisement => advertisement.AdvertisementId.Equals(advertisementId, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Stops the deployment of an application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="collectionName">Name of the collection.</param>
        public void StopDeploymentForApplication(string applicationName, string collectionName)
        {
            var deploymentList = this.GetDeploymentsByScopeName(applicationName, DeploymentScopeTypes.Application);
            foreach (var deployment in deploymentList)
            {
                if (deployment.Value.Equals(collectionName, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.RemoveDeployment(deployment.Key, collectionName, applicationName);
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the approval requests.
        /// </summary>
        /// <returns>A list containing the approval requests.</returns>
        public List<SmsApprovalRequest> GetOpenApprovalRequests()
        {
            var approvalRequests = new List<SmsApprovalRequest>();
            using (IResultObject userApplicationRequests = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereInteger, "SMS_UserApplicationRequest", "CurrentState", 1)))
            {
                foreach (WqlResultObject userApplicationRequest in userApplicationRequests) using (userApplicationRequest)
                {
                    var approvalRequest = new SmsApprovalRequest(userApplicationRequest);
                    approvalRequests.Add(approvalRequest);
                }
            }

            return approvalRequests;
        }

        /// <summary>
        /// Gets the application inventory.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>A <c>List</c> with all installed applications.</returns>
        public List<string> GetApplicationInventory(string computerName)
        {
            var applicationList = new List<string>();
            var computerId = this.GetComputerResourceIdByName(computerName);
            using (IResultObject installedApplications = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereInteger, "SMS_G_SYSTEM_ADD_REMOVE_PROGRAMS_64", "ResourceID", computerId)))
            {
                foreach (WqlResultObject installedApplication in installedApplications) using (installedApplication)
                {
                    applicationList.Add(installedApplication["DisplayName"].StringValue);
                }
            }

            return applicationList;
        }

        /// <summary>
        /// Gets the installed applications by inventory.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>A <c>List</c> containing all installed applications.</returns>
        public List<SmsApplication> GetInstalledApplicationsByInventory(string computerName)
        {
            var installedApplicationList = new List<SmsApplication>();
            var applicationList = this.GetApplicationInventory(computerName);
            foreach (var applicationName in applicationList)
            {
                try
                {
                    using (var applicationObject = this.GetApplicationByName(applicationName))
                    {
                        var application = new SmsApplication(applicationObject);
                        installedApplicationList.Add(application);
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // doesn't matter if the application does not exist
                }
            }

            return installedApplicationList;
        }

        /// <summary>
        /// Gets the installed applications by collection list.
        /// </summary>
        /// <param name="collectionList">The collection list.</param>
        /// <returns>A <c>List</c> containing all installed applications.</returns>
        public List<SmsApplication> GetInstalledApplicationsByCollectionList(List<SmsCollection> collectionList)
        {
            var installedApplicationList = new List<SmsApplication>();
            foreach (var collection in collectionList)
            {
                try
                {
                    using (var applicationObject = this.GetApplicationByName(collection.Name))
                    {
                        var application = new SmsApplication(applicationObject);
                        installedApplicationList.Add(application);
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // doesn't matter if the application does not exist
                }
            }

            return installedApplicationList;
        }

        /// <summary>
        /// Gets the installed applications from taskseequences by collection list.
        /// </summary>
        /// <param name="tasksequenceNames">The tasksequence names.</param>
        /// <returns>A <c>Dictionary</c> containing all applications and packages from the tasksequences.</returns>
        public Dictionary<string, SmsApplicationBase> GetInstalledApplicationsAndPackagesOfTasksequence(string[] tasksequenceNames)
        {
            var installedApplicationList = new Dictionary<string, SmsApplicationBase>();
            foreach (var tasksequenceName in tasksequenceNames)
            {
                if (string.IsNullOrEmpty(tasksequenceName)) continue;
                var smsTasksequence = new SmsTaskSequence(this._managementServer, tasksequenceName);
                var smsTasksequenceSteps = smsTasksequence.SmsTaskSequenceSteps;
                foreach (var smsTasksequenceStep in smsTasksequenceSteps)
                {
                    if (smsTasksequenceStep.TaskSequenceStepType != SmsTaskSequenceStep.TaskSequenceStepTypes.Action)
                    {
                        continue;
                    }

                    if (smsTasksequenceStep is SmsTaskSequenceInstallApplication)
                    {
                        var smsApplication = new SmsApplication(this._managementServer, smsTasksequenceStep.Name);
                        if (!installedApplicationList.ContainsKey(smsApplication.ApplicationName)) installedApplicationList.Add(smsApplication.ApplicationName, smsApplication);
                    }
                    else if (smsTasksequenceStep is SmsTaskSequenceInstallSoftware)
                    {
                        var smsPackage = new SmsPackage(this._managementServer, ((SmsTaskSequenceInstallSoftware)smsTasksequenceStep).PackageId, SmsPackage.InstanceType.PackageId);
                        if (!installedApplicationList.ContainsKey(smsPackage.Name)) installedApplicationList.Add(smsPackage.Name, smsPackage);
                    }
                }
            }

            return installedApplicationList;
        }
          
        /// <summary>
        /// Approves the request.
        /// </summary>
        /// <param name="requestGuid">The request GUID.</param>
        /// <param name="comment">The comment.</param>
        public void ApproveRequest(string requestGuid, string comment)
        {
            this.HandleRequest(requestGuid, comment, true);
        }

        /// <summary>
        /// Denys the request.
        /// </summary>
        /// <param name="requestGuid">The request GUID.</param>
        /// <param name="comment">The comment.</param>
        public void DenyRequest(string requestGuid, string comment)
        {
            this.HandleRequest(requestGuid, comment, false);
        }

        /// <summary>
        /// Gets a client.
        /// </summary>
        /// <param name="clientGuid">The <see cref="Guid"/> of the client.</param>
        /// <returns>The client object.</returns>
        public SmsClient GetClientByGuid(string clientGuid)
        {
            using (var resultObject = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_R_System", "SMSUniqueIdentifier", "GUID:" + clientGuid)))
            {
                foreach (WqlResultObject wqlResultObject in resultObject) using (wqlResultObject)
                    {
                        return new SmsClient(wqlResultObject);
                    }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindClient, "GUID", clientGuid));
        }

        public SmsClient GetClientByResourceId(string resourceId)
        {
            SmsClient smsClient = null;
            using (var resultObject = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_R_System", "ResourceID", resourceId)))
            {
                foreach (WqlResultObject wqlResultObject in resultObject) using (wqlResultObject)
                    {
                        smsClient = new SmsClient(wqlResultObject);
                    }
            }

            if (smsClient == null) throw new Exception("The client with ResourceID \"" + resourceId + "\" does not exist");

            return smsClient;
        }

        public int GetResourceIdByComputerName(string computerName)
        {
            if (string.IsNullOrEmpty(computerName)) throw new ArgumentNullException(nameof(computerName), "Computer Name cannot be null or empty.");

            using (var resultObject = this._managementServer.QueryProcessor.ExecuteQuery($"SELECT ResourceID FROM SMS_R_System WHERE Name = '{computerName}'"))
            {
                foreach (WqlResultObject wqlResultObject in resultObject) using (wqlResultObject)
                    {
                        return wqlResultObject["ResourceID"].IntegerValue;
                    }
            }

            throw new Exception($"No computer found with the name {computerName}.");
        }


        public bool ClientExistsByName(string clientName)
        {
            var exists = false;
            try
            {
                var smsClient = this.GetClient(clientName);
                if (smsClient != null)
                {
                    exists = true;
                }
            }
            catch
            {
                // empty catch because an error is thrown when the client does not exist and exists is already false...
            }

            return exists;
        }

        public bool ClientExistsByResourceId(string resourceId)
        {
            var exists = false;
            try
            {
                var smsClient = this.GetClientByResourceId(resourceId);
                if (smsClient != null)
                {
                    exists = true;
                }
            }
            catch
            {
                // empty catch because an error is thrown when the client does not exist and exists is already false...
            }

            return exists;
        }

        /// <summary>
        /// Gets a client.
        /// </summary>
        /// <param name="clientName">The name of the client.</param>
        /// <returns>The client object.</returns>
        public SmsClient GetClient(string clientName)
        {
            return this.GetClient(clientName, true);
        }

        public SmsClient GetClientByMacAddress(string macAddress)
        {
            return this.GetClientByMacAddress(macAddress, true);
        }

        public SmsClient GetClientByMacAddress(string macAddress, bool noObsolete)
        {
            var wmiQuery = string.Empty;
            if (noObsolete)
            {
                wmiQuery = string.Format(WmiSelectQueryWhereStringAndCustom, "SMS_R_System", "MACAddresses", macAddress, "(Obsolete = 0 OR Obsolete IS NULL)");
            }
            else
            {
                wmiQuery = string.Format(WmiSelectQueryWhereString, "SMS_R_System", "MACAddresses", macAddress);
            }
            using (var resultObject = this._managementServer.QueryProcessor.ExecuteQuery(wmiQuery))
            {
                foreach (WqlResultObject wqlResultObject in resultObject) using (wqlResultObject)
                    {
                        return new SmsClient(wqlResultObject);
                    }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindClient, "MACAddress", macAddress));
        }

        /// <summary>
        /// Gets a client.
        /// </summary>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="searchAllClients">If set to <c>true</c> [only active].</param>
        /// <returns> The <c>SmsClient</c> object. </returns>
        public SmsClient GetClient(string clientName, bool searchAllClients)
        {
            var wmiQuery = string.Empty;
            switch (searchAllClients)
            {
                case false:
                    wmiQuery = string.Format(WmiSelectQueryWhereStringAndCustom, "SMS_R_System", "Name", clientName, "Obsolete = 0");
                    break;
                case true:
                    wmiQuery = string.Format(WmiSelectQueryWhereStringAndCustom, "SMS_R_System", "Name", clientName, "(Obsolete = 0 OR Obsolete IS NULL)");
                    break;
            }

            using (var resultObject = this._managementServer.QueryProcessor.ExecuteQuery(wmiQuery))
            {
                foreach (WqlResultObject wqlResultObject in resultObject) using (wqlResultObject)
                    {
                        return new SmsClient(wqlResultObject);
                    }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindClient, "name", clientName));
        }

        public bool ClientWithUUIDExists(string smBiosGuid)
        {
            try
            {
                var client = this.GetClientByUUID(smBiosGuid);
                if (client != null) return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }

        public SmsClient GetClientByUUID(string smBiosGuid)
        {
            var wmiQuery = string.Format(WmiSelectQueryWhereString, "SMS_R_System", "SMBIOSGUID", smBiosGuid);
            using (var resultObject = this._managementServer.QueryProcessor.ExecuteQuery(wmiQuery))
            {
                foreach (WqlResultObject wqlResultObject in resultObject) using (wqlResultObject)
                {
                    return new SmsClient(wqlResultObject);
                }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindClient, "UUID", smBiosGuid));

        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>The <c>SmsUser</c> object.</returns>
        public SmsUser GetUser(string userName)
        {
            using (var users = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_R_User", "UniqueUserName", userName)))
            {
                foreach (WqlResultObject user in users) using (user)
                {
                    return new SmsUser(user);
                }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindUser, "UserName", userName));
        }


        /// <summary>
        /// Retrieves the migration status of an association between two computers by their names. This function first retrieves
        /// the Resource IDs for both source and destination computers based on their names and then uses these IDs to query
        /// the migration status in SCCM.
        /// </summary>
        /// <param name="sourceComputerName">The name of the source computer.</param>
        /// <param name="destinationComputerName">The name of the destination computer.</param>
        /// <returns>A string indicating the migration status (e.g., NOTSTARTED, INPROGRESS, COMPLETED) or an error message if the query fails.</returns>
        public string GetUSMTMigrationStatus(string sourceComputerName, string destinationComputerName)
        {
            try
            {
                string query = $"SELECT MigrationStatus FROM SMS_StateMigration WHERE SourceName = '{sourceComputerName}' AND RestoreName = '{destinationComputerName}'";

                using (IResultObject queryResults = this._managementServer.QueryProcessor.ExecuteQuery(query))
                {
                    foreach (IResultObject result in queryResults)
                    {
                        int status = result["MigrationStatus"].IntegerValue;

                        switch (status)
                        {
                            case 0:
                                return "NOTSTARTED";
                            case 1:
                                return "INPROGRESS";
                            case 2:
                                return "COMPLETED";
                            default:
                                return "Unknown status value.";
                        }
                    }
                }

                return "No matching records found.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public List<SmsUser> GetAllUsers()
        {
            var userList = new List<SmsUser>();
            using (var users = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQuery, "SMS_R_User")))
            {
                foreach (WqlResultObject user in users) using (user)
                    {
                        userList.Add(new SmsUser(user));
                    }
            }

            return userList;
        }

        public List<string> GetAllUserUniqueNames()
        {
            var userList = new List<string>();
            using (var users = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQuery, "SMS_R_User")))
            {
                foreach (WqlResultObject user in users) using (user)
                    {
                        var uniqueUserName = user.Properties["UniqueUserName"].StringValue.Split('\\');
                        userList.Add(uniqueUserName[1]);
                    }
            }

            return userList;
        }

        public void SendSccmStatusMessage(string computerName, int statusMessageType, SeverityTypes severityType, string component, string description)
        {
            this.SendSccmStatusMessage(computerName, ((StatusMessageType)statusMessageType), severityType, component, description);
        }

        public void SendSccmStatusMessage(string computerName, StatusMessageType statusMessageType, SeverityTypes severityType, string component, string description)
        {

            var inParameters = new Dictionary<string, object>();
            inParameters.Add("MessageText", description);
            inParameters.Add("MessageType", statusMessageType);

            var methodName = string.Empty;
            switch (severityType)
            {
                case SeverityTypes.Error:
                    methodName = "RaiseErrorStatusMsg";
                    break;
                case SeverityTypes.Warning:
                    methodName = "RaiseWarningStatusMsg";
                    break;
                default:
                    methodName = "RaiseInformationalStatusMsg";
                    break;
            };

            this._managementServer.Context.Add("Local", "MS\\1033");
            this._managementServer.Context.Add("MachineName", computerName);
            this._managementServer.Context.Add("ApplicationName", component);

            using (IResultObject outParams = this._managementServer.ExecuteMethod("SMS_StatusMessage", methodName, inParameters))
            {
                this._managementServer.Context.Remove("Local");
                this._managementServer.Context.Remove("MachineName");
                this._managementServer.Context.Remove("ApplicationName");

                if (outParams == null || outParams["ReturnValue"].IntegerValue != 0)
                {
                    throw new Exception("Failed to send Status message for \"" + computerName + "\"");
                }
            }

        }

        /// <summary>
        /// Sends a DDR (Data Discovery Record) with SID, OU, and Group information.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="domainName">The domain name.</param>
        /// <param name="computerSid">The computer SID.</param>
        /// <param name="systemOuName">Array of system OU names.</param>
        /// <param name="groupNames">Array of group names.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool SendDDRRecord(string computerName, string domainName, string computerSid, string[] systemOuName, string[] groupNames)
        {
            var newRecord = new Microsoft.ConfigurationManagement.Messaging.Messages.Server.DiscoveryDataRecordFile("Swisscom Discovery Agent");
            newRecord.Architecture = "System";
            newRecord.SiteCode = this._smsSite.SiteCode;

            newRecord.AddStringProperty("SID", Microsoft.ConfigurationManagement.Messaging.Messages.Server.DdmDiscoveryFlags.Key, 64, computerSid);

            if (systemOuName != null && systemOuName.Length > 0)
            {
                newRecord.AddStringPropertyArray("System OU Name", Microsoft.ConfigurationManagement.Messaging.Messages.Server.DdmDiscoveryFlags.Array, 256, systemOuName);
            }

            if (groupNames != null && groupNames.Length > 0)
            {
                newRecord.AddStringPropertyArray("System Group Name", Microsoft.ConfigurationManagement.Messaging.Messages.Server.DdmDiscoveryFlags.Array, 256, groupNames);
                newRecord.AddStringPropertyArray("Security Group Name", Microsoft.ConfigurationManagement.Messaging.Messages.Server.DdmDiscoveryFlags.Array, 256, groupNames);
            }

            newRecord.AddStringProperty("Name", Microsoft.ConfigurationManagement.Messaging.Messages.Server.DdmDiscoveryFlags.Key | Microsoft.ConfigurationManagement.Messaging.Messages.Server.DdmDiscoveryFlags.Name, 64, computerName);
            newRecord.AddStringProperty("Resource Domain Or Workgroup", Microsoft.ConfigurationManagement.Messaging.Messages.Server.DdmDiscoveryFlags.None, 255, domainName);

            newRecord.Validate();
            newRecord.SerializeToInbox(this.SccmServer);

            return true;
        }

        /// <summary>
        /// Gets the SMS Unique Identifier for a computer by ResourceID.
        /// </summary>
        /// <param name="resourceId">The ResourceID of the computer.</param>
        /// <returns>The SMS Unique Identifier or empty string if not found.</returns>
        public string GetSmsUniqueIdentifierByResourceId(int resourceId)
        {
            try
            {
                var query = string.Format(WmiSelectQueryWhereInteger, "SMS_R_System", "ResourceID", resourceId);
                using (var devices = this._managementServer.QueryProcessor.ExecuteQuery(query))
                {
                    foreach (IResultObject device in devices)
                    {
                        var isClient = device["Client"].IntegerValue == 1;

                        if (isClient)
                        {
                            return device["SMSUniqueIdentifier"].StringValue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log exception if needed
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the SMS Unique Identifier for a computer.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>The SMS Unique Identifier or empty string if not found.</returns>
        public string GetSmsUniqueIdentifier(string computerName)
        {
            try
            {
                var query = string.Format(WmiSelectQueryWhereString, "SMS_R_System", "Name", computerName);
                using (var devices = this._managementServer.QueryProcessor.ExecuteQuery(query))
                {
                    foreach (IResultObject device in devices)
                    {
                        var resourceId = device["ResourceID"].IntegerValue;
                        var isClient = device["Client"].IntegerValue == 1;

                        if (!resourceId.ToString().StartsWith("2") && isClient)
                        {
                            return device["SMSUniqueIdentifier"].StringValue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log exception if needed
            }

            return string.Empty;
        }

        /// <summary>
        /// Fisposes the instantiated object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the instantiated object.
        /// </summary>
        /// <param name="disposing">If the object should be disposed or not.</param>
        protected void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                if (disposing)
                {
                    // free managed ressources
                    if (this._managementServer != null) this._managementServer.Dispose();
                }

                // free unmanaged ressources
            }

            this._isDisposed = true;
        }

        /// <summary>
        /// Deletes the computer.
        /// </summary>
        /// <param name="computerNameOrId">Name of the computer.</param>
        /// <param name="computerIdType">Type of the computer id.</param>
        /// <returns>
        /// The number of objects that have been deleted.
        /// </returns>
        private int DeleteComputer(string computerNameOrId, ComputerIdType computerIdType)
        {
            var deletedComputers = 0;
            var fieldName = string.Empty;

            fieldName = computerIdType == ComputerIdType.ComputerName ? "Name" : "SMBIOSGUID";

            using (var computers = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_R_SYSTEM", fieldName, computerNameOrId)))
            {
                foreach (IResultObject computer in computers) using (computer)
                {
                    computer.Delete();
                    deletedComputers++;
                }
            }

            return deletedComputers;
        }

        /// <summary>
        /// Gets the collection setting by id.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <returns>The <c>IResultObject</c> of the SMS_CollectionSetting.</returns>
        private IResultObject GetCollectionSettingById(string collectionId)
        {
            using (var collectionSettings = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_CollectionSettings", "CollectionID", collectionId)))
            {
                foreach (IResultObject collectionSetting in collectionSettings)
                {
                    return collectionSetting;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the computer setting by id.
        /// </summary>
        /// <param name="resourceId">The resource id.</param>
        /// <returns>The <c>IResultObject</c> of the SMS_MachineSettings.</returns>
        private IResultObject GetComputerSettingById(int resourceId)
        {
            using (var collectionSettings = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_MachineSettings", "ResourceID", resourceId)))
            {
                foreach (IResultObject collectionSetting in collectionSettings)
                {
                    return collectionSetting;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes a deployment.
        /// </summary>
        /// <param name="assignmentUniqueId">The assignment unique id.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="applicationName">Name of the application.</param>
        private void RemoveDeployment(string assignmentUniqueId, string collectionName, string applicationName)
        {
            using (var deployments = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_ApplicationAssignment", "AssignmentUniqueId", assignmentUniqueId)))
            {
                foreach (WqlResultObject deployment in deployments) using (deployment)
                {
                    if (deployment["CollectionName"].StringValue.Equals(collectionName, StringComparison.CurrentCultureIgnoreCase) && deployment["ApplicationName"].StringValue.Equals(applicationName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        deployment.Delete();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Approves the request.
        /// </summary>
        /// <param name="requestGuid">The request GUID.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="approve">Ff set to <c>true</c> [approve].</param>
        private void HandleRequest(string requestGuid, string comment, bool approve)
        {
            using (IResultObject userApplicationRequests = this._managementServer.GetInstance(string.Format(WmiDirectReferenceString, "SMS_UserApplicationRequest", "RequestGuid", requestGuid)))
            {
                var parameters = new Dictionary<string, object>();
                parameters.Add("Comments", comment);
                var action = approve ? "Approve" : "Deny";
                userApplicationRequests.ExecuteMethod(action, parameters);
            }
        }

        /// <summary>
        /// Changes a clients membership in a collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="client">The client.</param>
        /// <param name="action">The action, if to add or remove.</param>
        private void ChangeMemberInCollectionDirect(WqlResultObject collection, SmsClient client, CollectionAction action)
        {
            var found = this.IsMemberOfCollection(client.ResourceId, collection.Properties["CollectionID"].StringValue);
            if (action == CollectionAction.AddMembershipRule)
            {
                // is already member
                if (found) return;
            }
            else
            {
                // is not member
                if (!found) return;
            }

            using (var rule = this._managementServer.CreateInstance("SMS_CollectionRuleDirect"))
            {
                rule["ResourceID"].StringValue = client.ResourceId.ToString(CultureInfo.InvariantCulture);
                rule["ResourceClassName"].StringValue = "SMS_R_System";
                rule["RuleName"].StringValue = client.Name;
                var parameters = new Dictionary<string, object>();
                parameters.Add("collectionRule", rule);
                if (!this.CheckCollectionReadyness(collection["CollectionId"].StringValue)) throw new Exception(string.Format(ErrorMessageCollectionNotReady, collection["Name"].StringValue));
                using (var id = collection.ExecuteMethod(Enum.GetName(typeof(CollectionAction), action), parameters))
                {
                    if (id == null) throw new Exception(string.Format(ErrorMessageFailedToModifyMemberInCollection, Enum.GetName(typeof(CollectionAction), action), client.Name, collection["Name"].StringValue));
                }
            }
        }

        /// <summary>
        /// Gets the name of the application by.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>The application.</returns>
        private WqlResultObject GetApplicationByName(string applicationName)
        {
            using (var applications = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereStringAndString, "SMS_Application", "LocalizedDisplayName", applicationName, "IsLatest", "TRUE")))
            {
                foreach (WqlResultObject application in applications)
                {
                    application.Get();
                    return application;
                }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindApplicationWithName, applicationName));
        }

        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <param name="collectionId">The id of the collection.</param>
        /// <returns>The collection object.</returns>
        private WqlResultObject GetCollection(string collectionId)
        {
            using (var collections = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_Collection", "CollectionID", collectionId)))
            {
                foreach (WqlResultObject collection in collections)
                {
                    collection.Get();
                    return collection;
                }
            }

            throw new InstanceNotFoundException(string.Format(ErrorMessageUnableToFindCollection, "collection id", collectionId));
        }

        /// <summary>
        /// Checks the collection readyness.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <returns><c>True</c> if the collection is ready, otherwise <c>false</c>.</returns>
        private bool CheckCollectionReadyness(string collectionId)
        {
            //// performs a check every half seconds for about two minutes (0.5 * 240 = 120 / 60 = 2 minutes)
            for (var counter = 0; counter < 240; counter++)
            {
                var collection = new SmsCollection(this._managementServer, collectionId);
                if (collection.CurrentStatus == CollectionCurrentStatus.Ready || collection.CurrentStatus == CollectionCurrentStatus.AwaitingRefresh) return true;
                Thread.Sleep(500);
            }

            return false;
        }


        /// <summary>
        /// Validates the query.
        /// </summary>
        /// <param name="queryValidationParameters">The query validation parameters.</param>
        /// <returns>true if the query is valid otherwise false.</returns>
        private bool ValidateQuery(Dictionary<string, object> queryValidationParameters)
        {
            using (var result = this._managementServer.ExecuteMethod("SMS_CollectionRuleQuery", "ValidateQuery", queryValidationParameters))
            {
                return result["ReturnValue"].BooleanValue;
            }
        }

        /// <summary>
        /// Gets the task sequence name by its package ID.
        /// </summary>
        /// <param name="packageId">The package ID of the task sequence.</param>
        /// <returns>The name of the task sequence, or null if not found.</returns>
        public string GetTaskSequenceNameByPackageId(string packageId)
        {
            using (var tasksequences = this._managementServer.QueryProcessor.ExecuteQuery(string.Format(WmiSelectQueryWhereString, "SMS_TasksequencePackage", "PackageID", packageId)))
            {
                foreach (IResultObject tasksequence in tasksequences) using (tasksequence)
                    {
                        return tasksequence["Name"].StringValue;
                    }
            }

            return null;
        }
    }
}
