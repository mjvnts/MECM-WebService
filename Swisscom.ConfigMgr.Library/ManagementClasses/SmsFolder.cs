// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsFolder.cs" company="Stadt Zürich Organisation und Informatik">
//   Copyright (c) 2013
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
    public class SmsFolder : ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsFolder"/> class.
        /// </summary>
        public SmsFolder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsFolder"/> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public SmsFolder(IResultObject folder)
        {
            this.FolderId = folder["ContainerNodeID"].IntegerValue;
            this.FolderName = folder["Name"].StringValue;
            this.FolderType = (FolderObjectType)folder["ObjectType"].IntegerValue;
            this.ParentFolderId = folder["ParentContainerNodeID"].IntegerValue;
            if (this.ParentFolderId != 0)
            {
                using (var parentFolder = folder.ConnectionManager.GetInstance("SMS_ObjectContainerNode.ContainerNodeID=" + this.ParentFolderId))
                {
                    this.ParentFolderName = parentFolder["Name"].StringValue;
                }
            }
        }

        /// <summary>
        /// The folder object type.
        /// </summary>
        [Serializable]
        public enum FolderObjectType
        {
            /// <summary>
            /// FolderType is SmsPackage.
            /// </summary>
            SmsPackage = 2,

            /// <summary>
            /// FolderType is SmsAdvertisement.
            /// </summary>
            SmsAdvertisement = 3,

            /// <summary>
            /// FolderType is SmsQuery.
            /// </summary>
            SmsQuery = 7,

            /// <summary>
            /// FolderType is SmsReport.
            /// </summary>
            SmsReport = 8,

            /// <summary>
            /// FolderType is SmsMeteredProductRule.
            /// </summary>
            SmsMeteredProductRule = 9,

            /// <summary>
            /// FolderType is SmsConfigurationItem.
            /// </summary>
            SmsConfigurationItem = 11,

            /// <summary>
            /// FolderType is SmsOperatingSystemInstallPackage.
            /// </summary>
            SmsOperatingSystemInstallPackage = 14,

            /// <summary>
            /// FolderType is SmsStateMigration.
            /// </summary>
            SmsStateMigration = 17,

            /// <summary>
            /// FolderType is SmsImagePackage.
            /// </summary>
            SmsImagePackage = 18,

            /// <summary>
            /// FolderType is SmsBootImagePackage.
            /// </summary>
            SmsBootImagePackage = 19,

            /// <summary>
            /// FolderType is SmsTasksequencePackage.
            /// </summary>
            SmsTasksequencePackage = 20,

            /// <summary>
            /// FolderType is SmsDeviceSettingPackage.
            /// </summary>
            SmsDeviceSettingPackage = 21,

            /// <summary>
            /// FolderType is SmsDriverPackage.
            /// </summary>
            SmsDriverPackage = 23,

            /// <summary>
            /// FolderType is SmsDriver.
            /// </summary>
            SmsDriver = 25,

            /// <summary>
            /// FolderType is SmsSoftwareUpdate.
            /// </summary>
            SmsSoftwareUpdate = 1011,

            /// <summary>
            /// FolderType is SmsConfigurationBaseline.
            /// </summary>
            SmsConfigurationBaseline = 2011,

            /// <summary>
            /// FolderType is SmsCollectionDevice.
            /// </summary>
            SmsCollectionDevice = 5000,

            /// <summary>
            /// FolderType is SmsCollectionUser.
            /// </summary>
            SmsCollectionUser = 5001,

            /// <summary>
            /// FolderType is SmsApplicationLatest.
            /// </summary>
            SmsApplicationLatest = 6000,
        }

        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        /// <value>
        /// The name of the folder.
        /// </value>
        public string FolderName { get; set; }

        /// <summary>
        /// Gets or sets the folder id.
        /// </summary>
        /// <value>
        /// The folder id.
        /// </value>
        public int FolderId { get; set; }

        /// <summary>
        /// Gets or sets the parent folder id.
        /// </summary>
        /// <value>
        /// The parent folder id.
        /// </value>
        public int ParentFolderId { get; set; }

        /// <summary>
        /// Gets or sets the name of the parent folder.
        /// </summary>
        /// <value>
        /// The name of the parent folder.
        /// </value>
        public string ParentFolderName { get; set; }

        /// <summary>
        /// Gets or sets the type of the folder.
        /// </summary>
        /// <value>
        /// The type of the folder.
        /// </value>
        public FolderObjectType FolderType { get; set; }

        /// <summary>
        /// Creates a new instance of the SMS object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created folder.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            var folder = connection.CreateInstance("SMS_ObjectContainerNode");
            folder["Name"].StringValue = this.FolderName;
            folder["ObjectType"].IntegerValue = (int)this.FolderType;
            folder["ParentContainerNodeID"].IntegerValue = this.ParentFolderId;
            try
            {
                folder.Put();
                return folder;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to create the new folder " + this.ParentFolderName + "\\" + this.FolderName + ": " + exception.Message);
            }
        }
    }
}
