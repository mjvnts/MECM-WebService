// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsPackage.cs" company="LANexpert S.A.">
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
    /// The SMS_Package Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class that contains information about Configuration Manager packages.
    /// </summary>
    [Serializable]
    public class SmsPackage : SmsApplicationBase, ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsPackage"/> class.
        /// </summary>
        public SmsPackage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsPackage"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        public SmsPackage(WqlResultObject package)
        {
            this.GetPackage(package);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsPackage"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="packageNameOrId">The package name or id.</param>
        /// <param name="instanceType">Type of the instance.</param>
        public SmsPackage(WqlConnectionManager connection, string packageNameOrId, InstanceType instanceType)
        {
            if (instanceType == InstanceType.PackageId)
            {
                using (var packageObject = connection.GetInstance("SMS_Package.PackageID='" + packageNameOrId + "'"))
                {
                    this.GetPackage(packageObject as WqlResultObject);
                }
            }
            else
            {
                using (var packageObject = connection.QueryProcessor.ExecuteQuery("SELECT * FROM SMS_Package WHERE Name = '" + packageNameOrId + "'"))
                {
                    foreach (WqlResultObject package in packageObject)
                    {
                        this.GetPackage(package);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Type of the instance.
        /// </summary>
        [Serializable]
        public enum InstanceType
        {
            /// <summary>
            /// Search after package id.
            /// </summary>
            PackageId,

            /// <summary>
            /// Search after package name.
            /// </summary>
            PackageName
        }

        /// <summary>
        /// Gets or sets the action in progress.
        /// </summary>
        /// <value>
        /// The action in progress.
        /// </value>
        public PackageActionInProgress ActionInProgress { get; set; }

        /// <summary>
        /// Gets or sets the alternate content providers.
        /// </summary>
        /// <value>
        /// The alternate content providers.
        /// </value>
        public string AlternateContentProviders { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the extended data.
        /// </summary>
        /// <value>
        /// The extended data.
        /// </value>
        public int ExtendedData { get; set; }

        /// <summary>
        /// Gets or sets the size of the extended data.
        /// </summary>
        /// <value>
        /// The size of the extended data.
        /// </value>
        public int ExtendedDataSize { get; set; }

        /// <summary>
        /// Gets or sets the forced disconnect delay.
        /// </summary>
        /// <value>
        /// The forced disconnect delay.
        /// </value>
        public int ForcedDisconnectDelay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [forced disconnect enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [forced disconnect enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ForcedDisconnectEnabled { get; set; }

        /// <summary>
        /// Gets or sets the forced disconnect num retries.
        /// </summary>
        /// <value>
        /// The forced disconnect num retries.
        /// </value>
        public int ForcedDisconnectNumRetries { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>
        /// The used icon.
        /// </value>
        public int[] Icon { get; set; }

        /// <summary>
        /// Gets or sets the size of the icon.
        /// </summary>
        /// <value>
        /// The size of the icon.
        /// </value>
        public int IconSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [ignore address schedule].
        /// </summary>
        /// <value>
        /// <c>true</c> if [ignore address schedule]; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreAddressSchedule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is predefined package.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is predefined package; otherwise, <c>false</c>.
        /// </value>
        public bool IsPredefinedPackage { get; set; }

        /// <summary>
        /// Gets or sets the isv data.
        /// </summary>
        /// <value>
        /// The isv data.
        /// </value>
        public int[] IsvData { get; set; }

        /// <summary>
        /// Gets or sets the size of the isv data.
        /// </summary>
        /// <value>
        /// The size of the isv data.
        /// </value>
        public int IsvDataSize { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the last refresh time.
        /// </summary>
        /// <value>
        /// The last refresh time.
        /// </value>
        public DateTime LastRefreshTime { get; set; }

        /// <summary>
        /// Gets or sets the localized category instance names.
        /// </summary>
        /// <value>
        /// The localized category instance names.
        /// </value>
        public string[] LocalizedCategoryInstanceNames { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        /// <value>
        /// The manufacturer.
        /// </value>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The used name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the num of programs.
        /// </summary>
        /// <value>
        /// The num of programs.
        /// </value>
        public int NumOfPrograms { get; set; }

        /// <summary>
        /// Gets or sets the package id.
        /// </summary>
        /// <value>
        /// The package id.
        /// </value>
        public string PackageId { get; set; }

        /// <summary>
        /// Gets or sets the package type.
        /// </summary>
        /// <value>
        /// The package type.
        /// </value>
        public PackageType PkgType { get; set; }

        /// <summary>
        /// Gets or sets the PKG flags.
        /// </summary>
        /// <value>
        /// The PKG flags.
        /// </value>
        public PackageFlags PkgFlags { get; set; }

        /// <summary>
        /// Gets or sets the PKG source flag.
        /// </summary>
        /// <value>
        /// The PKG source flag.
        /// </value>
        public PackageSourceFlag PkgSourceFlag { get; set; }

        /// <summary>
        /// Gets or sets the PKG source path.
        /// </summary>
        /// <value>
        /// The PKG source path.
        /// </value>
        public string PkgSourcePath { get; set; }

        /// <summary>
        /// Gets or sets the type of the preferred address.
        /// </summary>
        /// <value>
        /// The type of the preferred address.
        /// </value>
        public string PreferredAddressType { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [refresh PKG source flag].
        /// </summary>
        /// <value>
        /// <c>true</c> if [refresh PKG source flag]; otherwise, <c>false</c>.
        /// </value>
        public bool RefreshPkgSourceFlag { get; set; }

        /// <summary>
        /// Gets or sets the refresh schedule.
        /// </summary>
        /// <value>
        /// The refresh schedule.
        /// </value>
        public SmsScheduleToken RefreshSchedule { get; set; }

        /// <summary>
        /// Gets or sets the secured scope names.
        /// </summary>
        /// <value>
        /// The secured scope names.
        /// </value>
        public string[] SecuredScopeNames { get; set; }

        /// <summary>
        /// Gets or sets the sedo object version.
        /// </summary>
        /// <value>
        /// The sedo object version.
        /// </value>
        public string SedoObjectVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the share.
        /// </summary>
        /// <value>
        /// The name of the share.
        /// </value>
        public string ShareName { get; set; }

        /// <summary>
        /// Gets or sets the type of the share.
        /// </summary>
        /// <value>
        /// The type of the share.
        /// </value>
        public PackageShareType ShareType { get; set; }

        /// <summary>
        /// Gets or sets the source date.
        /// </summary>
        /// <value>
        /// The source date.
        /// </value>
        public DateTime SourceDate { get; set; }

        /// <summary>
        /// Gets or sets the source site.
        /// </summary>
        /// <value>
        /// The source site.
        /// </value>
        public string SourceSite { get; set; }

        /// <summary>
        /// Gets or sets the source version.
        /// </summary>
        /// <value>
        /// The source version.
        /// </value>
        public int SourceVersion { get; set; }

        /// <summary>
        /// Gets or sets the stored PKG path.
        /// </summary>
        /// <value>
        /// The stored PKG path.
        /// </value>
        public string StoredPkgPath { get; set; }

        /// <summary>
        /// Gets or sets the stored PKG version.
        /// </summary>
        /// <value>
        /// The stored PKG version.
        /// </value>
        public int StoredPkgVersion { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Creates a new instance of the SMS object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        /// <exception cref="NotImplementedException">The function is currently not implemented.</exception>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            throw new NotImplementedException("Not yet implemented.");
        }

        /// <summary>
        /// Gets the package.
        /// </summary>
        /// <param name="package">The package.</param>
        private void GetPackage(WqlResultObject package)
        {
            package.Get();
            if (package["ActionInProgress"].ObjectValue != null) this.ActionInProgress = (PackageActionInProgress)package["ActionInProgress"].IntegerValue;
            if (package["AlternateContentProviders"].ObjectValue != null) this.AlternateContentProviders = package["AlternateContentProviders"].StringValue;
            if (package["Description"].ObjectValue != null) this.Description = package["Description"].StringValue;
            if (package["ExtendedData"].ObjectValue != null) this.ExtendedData = package["ExtendedData"].IntegerValue;
            if (package["ExtendedDataSize"].ObjectValue != null) this.ExtendedDataSize = package["ExtendedDataSize"].IntegerValue;
            if (package["ForcedDisconnectDelay"].ObjectValue != null) this.ForcedDisconnectDelay = package["ForcedDisconnectDelay"].IntegerValue;
            if (package["ForcedDisconnectEnabled"].ObjectValue != null) this.ForcedDisconnectEnabled = package["ForcedDisconnectEnabled"].BooleanValue;
            if (package["ForcedDisconnectNumRetries"].ObjectValue != null) this.ForcedDisconnectNumRetries = package["ForcedDisconnectNumRetries"].IntegerValue;
            if (package["Icon"].ObjectValue != null) this.Icon = package["Icon"].IntegerArrayValue;
            if (package["IconSize"].ObjectValue != null) this.IconSize = package["IconSize"].IntegerValue;
            if (package["IgnoreAddressSchedule"].ObjectValue != null) this.IgnoreAddressSchedule = package["IgnoreAddressSchedule"].BooleanValue;
            if (package["IsPredefinedPackage"].ObjectValue != null) this.IsPredefinedPackage = package["IsPredefinedPackage"].BooleanValue;
            if (package["IsvData"].ObjectValue != null) this.IsvData = package["IsvData"].IntegerArrayValue;
            if (package["IsvDataSize"].ObjectValue != null) this.IsvDataSize = package["IsvDataSize"].IntegerValue;
            if (package["Language"].ObjectValue != null) this.Language = package["Language"].StringValue;
            if (package["LastRefreshTime"].ObjectValue != null) this.LastRefreshTime = package["LastRefreshTime"].DateTimeValue;
            if (package["LocalizedCategoryInstanceNames"].ObjectValue != null) this.LocalizedCategoryInstanceNames = package["LocalizedCategoryInstanceNames"].StringArrayValue;
            if (package["Manufacturer"].ObjectValue != null) this.Manufacturer = package["Manufacturer"].StringValue;
            if (package["Name"].ObjectValue != null) this.Name = package["Name"].StringValue;
            if (package["NumOfPrograms"].ObjectValue != null) this.NumOfPrograms = package["NumOfPrograms"].IntegerValue;
            if (package["PackageId"].ObjectValue != null) this.PackageId = package["PackageId"].StringValue;
            if (package["PackageType"].ObjectValue != null) this.PkgType = (PackageType)package["PackageType"].IntegerValue;
            if (package["PkgFlags"].ObjectValue != null) this.PkgFlags = (PackageFlags)package["PkgFlags"].IntegerValue;
            if (package["PkgSourceFlag"].ObjectValue != null) this.PkgSourceFlag = (PackageSourceFlag)package["PkgSourceFlag"].IntegerValue;
            if (package["PkgSourcePath"].ObjectValue != null) this.PkgSourcePath = package["PkgSourcePath"].StringValue;
            if (package["PreferredAddressType"].ObjectValue != null) this.PreferredAddressType = package["PreferredAddressType"].StringValue;
            if (package["Priority"].ObjectValue != null) this.Priority = package["Priority"].IntegerValue;
            if (package["RefreshPkgSourceFlag"].ObjectValue != null) this.RefreshPkgSourceFlag = package["RefreshPkgSourceFlag"].BooleanValue;

            // currently only implemented the first occurence
            foreach (var resultObject in package.GetArrayItems("RefreshSchedule"))
            {
                if (resultObject != null)
                {
                    this.RefreshSchedule = new SmsScheduleToken(resultObject);
                    break;
                }
            }

            if (package["SecuredScopeNames"].ObjectValue != null) this.SecuredScopeNames = package["SecuredScopeNames"].StringArrayValue;
            if (package["SedoObjectVersion"].ObjectValue != null) this.SedoObjectVersion = package["SedoObjectVersion"].StringValue;
            if (package["ShareName"].ObjectValue != null) this.ShareName = package["ShareName"].StringValue;
            if (package["ShareType"].ObjectValue != null) this.ShareType = (PackageShareType)package["ShareType"].IntegerValue;
            if (package["SourceDate"].ObjectValue != null) this.SourceDate = package["SourceDate"].DateTimeValue;
            if (package["SourceSite"].ObjectValue != null) this.SourceSite = package["SourceSite"].StringValue;
            if (package["SourceVersion"].ObjectValue != null) this.SourceVersion = package["SourceVersion"].IntegerValue;
            if (package["StoredPkgPath"].ObjectValue != null) this.StoredPkgPath = package["StoredPkgPath"].StringValue;
            if (package["StoredPkgVersion"].ObjectValue != null) this.StoredPkgVersion = package["StoredPkgVersion"].IntegerValue;
            if (package["Version"].ObjectValue != null) this.Version = package["Version"].StringValue;
        }
    }
}
