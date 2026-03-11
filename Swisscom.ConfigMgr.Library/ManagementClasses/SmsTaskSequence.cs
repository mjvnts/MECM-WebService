// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsTaskSequence.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Collections.Generic;

    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

    /// <summary>
    /// The SMS_TaskSequencePackage Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in Configuration Manager,
    /// that represents a task sequence package that defines the steps to run for the task sequence. 
    /// </summary>
    [Serializable]
    public class SmsTaskSequence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequence"/> class.
        /// </summary>
        public SmsTaskSequence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsTaskSequence"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="taskSequenceName">Name of the task sequence.</param>
        public SmsTaskSequence(WqlConnectionManager connection, string taskSequenceName)
        {
            using (var taskSequencePackages = connection.QueryProcessor.ExecuteQuery("SELECT * FROM SMS_TaskSequencePackage WHERE Name ='" + taskSequenceName + "'"))
            {
                foreach (IResultObject taskSequencePackage in taskSequencePackages) using (taskSequencePackage)
                {
                    this.Name = taskSequencePackage["Name"].StringValue;
                    this.PackageId = taskSequencePackage["PackageID"].StringValue;

                    var inputParameters = new Dictionary<string, object>();
                    inputParameters.Add("TaskSequencePackage", taskSequencePackage);
                    using (IResultObject outputParameters = connection.ExecuteMethod("SMS_TaskSequencePackage", "GetSequence", inputParameters))
                    {
                        using (IResultObject taskSequence = outputParameters.GetSingleItem("TaskSequence"))
                        {
                            this.GetSteps(taskSequence);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The type of the package. 
        /// </summary>
        [Serializable]
        public enum PackageTypeTypes
        {
            /// <summary>
            /// Regular software distribution package.
            /// </summary>
            RegularPackage = 0,

            /// <summary>
            /// Driver package.
            /// </summary>
            Driver = 3,

            /// <summary>
            /// Task sequence package.
            /// </summary>
            TaskSequence = 4,

            /// <summary>
            /// Software update package.
            /// </summary>
            SoftwareUpdate = 5,

            /// <summary>
            /// Virtual application package.
            /// </summary>
            VirtualAppPackage = 7,

            /// <summary>
            /// Content package.
            /// </summary>
            ContentPackage = 8,

            /// <summary>
            /// Image package.
            /// </summary>
            ImageDeployment = 257,

            /// <summary>
            /// Boot image package.
            /// </summary>
            BootImage = 258,

            /// <summary>
            /// Operating system install package.
            /// </summary>
            OsInstallPackage = 259,
        }

        /// <summary>
        /// Flags specifying special properties of the package.
        /// </summary>
        [Flags]
        [Serializable]
        public enum PkgFlagsTypes : long
        {
            /// <summary>
            /// Not defined.
            /// </summary>
            NotDefined = 0L,

            /// <summary>
            /// Do not encrypt content on the cloud.
            /// </summary>
            DoNotEncryptContentOnCloud = 23L,

            /// <summary>
            /// Do not download the package to branch distribution points, as it will be pre-staged.
            /// </summary>
            DoNotDownload = 24L,

            /// <summary>
            /// Persist the package in the cache.
            /// </summary>
            PersistInCache = 25L,

            /// <summary>
            /// Marks the package to be replicated by distribution manager using binary delta replication.
            /// </summary>
            UseBinaryDeltaRep = 26L,

            /// <summary>
            /// The package does not require distribution points.
            /// </summary>
            NoPackage = 28L,

            /// <summary>
            /// This value determines if Configuration Manager uses MIFName, MIFPublisher, and MIFVersion
            /// for MIF file status matching. Otherwise, Configuration Manager uses Name, Manufacturer, and Version 
            /// for status matching. For more information, see the Remarks section later in this topic.
            /// </summary>
            UseSpecialMif = 29L,

            /// <summary>
            /// The package is allowed to be distributed on demand to branch distribution points.
            /// </summary>
            DistributeOnDemand = 30L
        }

        /// <summary>
        /// The type of task sequence represented by the package. 
        /// </summary>
        public enum TaskSequenceTypeTypes
        {
            /// <summary>
            /// Generic task sequence.
            /// </summary>
            Generic = 1,

            /// <summary>
            /// Operating system deployment task sequence.
            /// </summary>
            OperatingSystemDeployment = 2
        }

        /// <summary>
        /// Gets or sets the Tasksequence name.
        /// </summary>
        /// <value>
        /// The Tasksequence name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the package id.
        /// </summary>
        /// <value>
        /// The package id.
        /// </value>
        public string PackageId { get; set; }

        /// <summary>
        /// Gets or sets the SMS task sequence steps.
        /// </summary>
        /// <value>
        /// The SMS task sequence steps.
        /// </value>
        public List<SmsTaskSequenceStep> SmsTaskSequenceSteps { get; set; }

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
        /// Gets or sets the boot image id.
        /// </summary>
        /// <value>
        /// The boot image id.
        /// </value>
        public string BootImageId { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the custom progress MSG.
        /// </summary>
        /// <value>
        /// The custom progress MSG.
        /// </value>
        public string CustomProgressMsg { get; set; }

        /// <summary>
        /// Gets or sets the dependent program.
        /// </summary>
        /// <value>
        /// The dependent program.
        /// </value>
        public string DependentProgram { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public int Duration { get; set; }

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
        /// Gets or sets a value indicating whether [ignore address schedule].
        /// </summary>
        /// <value>
        /// <c>true</c> if [ignore address schedule]; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreAddressSchedule { get; set; }

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
        /// Gets or sets the name of the mif file.
        /// </summary>
        /// <value>
        /// The name of the mif file.
        /// </value>
        public string MifFileName { get; set; }

        /// <summary>
        /// Gets or sets the name of the mif.
        /// </summary>
        /// <value>
        /// The name of the mif.
        /// </value>
        public string MifName { get; set; }

        /// <summary>
        /// Gets or sets the mif publisher.
        /// </summary>
        /// <value>
        /// The mif publisher.
        /// </value>
        public string MifPublisher { get; set; }

        /// <summary>
        /// Gets or sets the mif version.
        /// </summary>
        /// <value>
        /// The mif version.
        /// </value>
        public string MifVersion { get; set; }

        /// <summary>
        /// Gets or sets the type of the package.
        /// </summary>
        /// <value>
        /// The type of the package.
        /// </value>
        public PackageTypeTypes PackageType { get; set; }

        /// <summary>
        /// Gets or sets the PKG flags.
        /// </summary>
        /// <value>
        /// The PKG flags.
        /// </value>
        public PkgFlagsTypes PkgFlags { get; set; }

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
        public AdvertisementPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the current program flags.
        /// </summary>
        /// <value>
        /// The current program flags.
        /// </value>
        public ProgramFlags CurrentProgramFlags { get; set; }

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
        /// Gets or sets the supported operating systems.
        /// </summary>
        /// <value>
        /// The supported operating systems.
        /// </value>
        public string[] SupportedOperatingSystems { get; set; }

        /// <summary>
        /// Gets or sets the type of the task sequence.
        /// </summary>
        /// <value>
        /// The type of the task sequence.
        /// </value>
        public TaskSequenceTypeTypes TaskSequenceType { get; set; }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <param name="currentStep">The current step.</param>
        private void GetSteps(IResultObject currentStep)
        {
            if (this.SmsTaskSequenceSteps == null) this.SmsTaskSequenceSteps = new List<SmsTaskSequenceStep>();
            foreach (IResultObject taskSequenceStep in currentStep.GetArrayItems("Steps")) using (taskSequenceStep)
            {
                SmsTaskSequenceStep step = null;
                if (taskSequenceStep["__CLASS"].StringValue.Equals("SMS_TaskSequence_Group", StringComparison.InvariantCultureIgnoreCase))
                {
                    step = new SmsTaskSequenceGroup(taskSequenceStep);
                    this.GetSteps(taskSequenceStep);
                }

                if (taskSequenceStep["__CLASS"].StringValue.Equals("SMS_TaskSequence_InstallApplicationAction", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(taskSequenceStep["ApplicationName"].StringValue)) step = new SmsTaskSequenceInstallApplication(taskSequenceStep);
                }

                if (taskSequenceStep["__CLASS"].StringValue.Equals("SMS_TaskSequence_InstallSoftwareAction", StringComparison.InvariantCultureIgnoreCase))
                {
                    step = new SmsTaskSequenceInstallSoftware(taskSequenceStep);
                }

                if (step != null) this.SmsTaskSequenceSteps.Add(step);
            }
        }
    }
}
