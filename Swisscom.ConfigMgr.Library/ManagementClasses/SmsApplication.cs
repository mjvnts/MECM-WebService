// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsApplication.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Globalization;

    using Microsoft.ConfigurationManagement.ManagementProvider;
    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
    using Swisscom.ConfigMgr.Library.Interfaces;

    /// <summary>
    /// The application class does not implement ISmsBaseClass and SmsBaseClass
    /// because creating an instance is not intended.
    /// </summary>
    [Serializable]
    public class SmsApplication : SmsApplicationBase, ISmsBaseClass
    {
        /// <summary>
        /// The application types.
        /// </summary>
        [Serializable]
        public enum ApplicationTypes
        {
            /// <summary>
            /// The type is a software update.
            /// </summary>
            SoftwareUpdate = 1,

            /// <summary>
            /// The type is a baseline.
            /// </summary>
            Baseline = 2,

            /// <summary>
            /// The type is an operating system.
            /// </summary>
            OperatingSystem = 3,

            /// <summary>
            /// The type is a business policy.
            /// </summary>
            BusinessPolicy = 4,

            /// <summary>
            /// The type is an application.
            /// </summary>
            Application = 5,

            /// <summary>
            /// The type is a driver.
            /// </summary>
            Driver = 6,

            /// <summary>
            /// The type is an other configuration item.
            /// </summary>
            OtherConfigurationItem = 7,

            /// <summary>
            /// The type is a software bundle.
            /// </summary>
            SoftwareUpdateBundle = 8,

            /// <summary>
            /// The type is a software update authorization list.
            /// </summary>
            SoftwareUpdateAuthorizationList = 9,

            /// <summary>
            /// The type is an application model.
            /// </summary>
            AppModel = 10,

            /// <summary>
            /// The type is a global setting.
            /// </summary>
            GlobalSettings = 11,

            /// <summary>
            /// The type is a global expression.
            /// </summary>
            GlobalExpression = 13,

            /// <summary>
            /// The type is a platform.
            /// </summary>
            Platform = 14,

            /// <summary>
            /// The type is a deployment type.
            /// </summary>
            DeploymentType = 21,

            /// <summary>
            /// The type is a deployment technology.
            /// </summary>
            DeploymentTechnology = 25,

            /// <summary>
            /// The type is a hosting technology.
            /// </summary>
            HostingTechnology = 26,

            /// <summary>
            /// The type is an installer technology.
            /// </summary>
            InstallerTechnology = 27,
        }

        /// <summary>
        /// SmsApplication type.
        /// </summary>
        private int _applicationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplication"/> class.
        /// </summary>
        public SmsApplication()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplication"/> class.
        /// </summary>
        /// <param name="application">The application.</param>
        public SmsApplication(WqlResultObject application)
        {
            application.Get();
            this.ApplicationDescription = application["LocalizedDescription"].StringValue;
            this.ApplicationId = application["CI_ID"].IntegerValue;
            this.ApplicationName = application["LocalizedDisplayName"].StringValue;
            this.ApplicationType = application["CIType_ID"].IntegerValue.ToString(CultureInfo.InvariantCulture);
            this.ApplicationUniqueId = application["CI_UniqueID"].StringValue;
            this.ApplicationVersion = application["CIVersion"].IntegerValue;
            this.CreatedBy = application["CreatedBy"].StringValue;
            this.HasContent = application["HasContent"].BooleanValue;
            this.IsBundle = application["IsBundle"].BooleanValue;
            this.IsDeployable = application["IsDeployable"].BooleanValue;
            this.IsDeployed = application["IsDeployed"].BooleanValue;
            this.IsDigest = application["IsDigest"].BooleanValue;
            this.IsEnabled = application["IsEnabled"].BooleanValue;
            this.IsExpired = application["IsExpired"].BooleanValue;
            this.IsHidden = application["IsHidden"].BooleanValue;
            this.IsLatest = application["IsLatest"].BooleanValue;
            this.IsQuarantined = application["IsQuarantined"].BooleanValue;
            this.IsSuperseded = application["IsSuperseded"].BooleanValue;
            this.IsSuperseding = application["IsSuperseding"].BooleanValue;
            this.IsUserDefined = application["IsUserDefined"].BooleanValue;
            this.LastModifiedBy = application["LastModifiedBy"].StringValue;
            this.Manufacturer = application["Manufacturer"].StringValue;
            this.ModelId = application["ModelID"].IntegerValue;
            this.ModelName = application["ModelName"].StringValue;
            this.NumberOfDependedTasksequences = application["NumberOfDependentTS"].IntegerValue;
            this.NumberOfDependentDTs = application["NumberOfDependentDTs"].IntegerValue;
            this.NumberOfDeploymentTypes = application["NumberOfDeploymentTypes"].IntegerValue;
            this.NumberOfDeployments = application["NumberOfDeployments"].IntegerValue;
            this.NumberOfDevicesWithApps = application["NumberOfDevicesWithApp"].IntegerValue;
            this.NumberOfDevicesWithFailure = application["NumberOfDevicesWithFailure"].IntegerValue;
            this.NumberOfUsersWithApp = application["NumberOfUsersWithApp"].IntegerValue;
            this.NumberOfUsersWithAppFailure = application["NumberOfUsersWithFailure"].IntegerValue;
            this.NumberOfUsersWithRequest = application["NumberOfUsersWithRequest"].IntegerValue;
            this.NumberOfVirtualEnvironments = application["NumberOfVirtualEnvironments"].IntegerValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplication"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="applicationId">The application id.</param>
        public SmsApplication(WqlConnectionManager connection, int applicationId)
        {
            using (var application = connection.GetInstance("SMS_Application.CI_ID=" + applicationId))
            {
                application.Get();
                this.ApplicationDescription = application["LocalizedDescription"].StringValue;
                this.ApplicationId = application["CI_ID"].IntegerValue;
                this.ApplicationName = application["LocalizedDisplayName"].StringValue;
                this.ApplicationType = application["CIType_ID"].IntegerValue.ToString(CultureInfo.InvariantCulture);
                this.ApplicationUniqueId = application["CI_UniqueID"].StringValue;
                this.ApplicationVersion = application["CIVersion"].IntegerValue;
                this.CreatedBy = application["CreatedBy"].StringValue;
                this.HasContent = application["HasContent"].BooleanValue;
                this.IsBundle = application["IsBundle"].BooleanValue;
                this.IsDeployable = application["IsDeployable"].BooleanValue;
                this.IsDeployed = application["IsDeployed"].BooleanValue;
                this.IsDigest = application["IsDigest"].BooleanValue;
                this.IsEnabled = application["IsEnabled"].BooleanValue;
                this.IsExpired = application["IsExpired"].BooleanValue;
                this.IsHidden = application["IsHidden"].BooleanValue;
                this.IsLatest = application["IsLatest"].BooleanValue;
                this.IsQuarantined = application["IsQuarantined"].BooleanValue;
                this.IsSuperseded = application["IsSuperseded"].BooleanValue;
                this.IsSuperseding = application["IsSuperseding"].BooleanValue;
                this.IsUserDefined = application["IsUserDefined"].BooleanValue;
                this.LastModifiedBy = application["LastModifiedBy"].StringValue;
                this.Manufacturer = application["Manufacturer"].StringValue;
                this.ModelId = application["ModelID"].IntegerValue;
                this.ModelName = application["ModelName"].StringValue;
                this.NumberOfDependedTasksequences = application["NumberOfDependentTS"].IntegerValue;
                this.NumberOfDependentDTs = application["NumberOfDependentDTs"].IntegerValue;
                this.NumberOfDeploymentTypes = application["NumberOfDeploymentTypes"].IntegerValue;
                this.NumberOfDeployments = application["NumberOfDeployments"].IntegerValue;
                this.NumberOfDevicesWithApps = application["NumberOfDevicesWithApp"].IntegerValue;
                this.NumberOfDevicesWithFailure = application["NumberOfDevicesWithFailure"].IntegerValue;
                this.NumberOfUsersWithApp = application["NumberOfUsersWithApp"].IntegerValue;
                this.NumberOfUsersWithAppFailure = application["NumberOfUsersWithFailure"].IntegerValue;
                this.NumberOfUsersWithRequest = application["NumberOfUsersWithRequest"].IntegerValue;
                this.NumberOfVirtualEnvironments = application["NumberOfVirtualEnvironments"].IntegerValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplication"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="applicationName">Name of the application.</param>
        public SmsApplication(WqlConnectionManager connection, string applicationName)
        {
            using (IResultObject applications = connection.QueryProcessor.ExecuteQuery("SELECT * FROM SMS_Application WHERE LocalizedDisplayName = '" + applicationName + "'"))
            {
                foreach (IResultObject application in applications)
                {
                    application.Get();
                    this.ApplicationDescription = application["LocalizedDescription"].StringValue;
                    this.ApplicationId = application["CI_ID"].IntegerValue;
                    this.ApplicationName = application["LocalizedDisplayName"].StringValue;
                    this.ApplicationType = application["CIType_ID"].IntegerValue.ToString(CultureInfo.InvariantCulture);
                    this.ApplicationUniqueId = application["CI_UniqueID"].StringValue;
                    this.ApplicationVersion = application["CIVersion"].IntegerValue;
                    this.CreatedBy = application["CreatedBy"].StringValue;
                    this.HasContent = application["HasContent"].BooleanValue;
                    this.IsBundle = application["IsBundle"].BooleanValue;
                    this.IsDeployable = application["IsDeployable"].BooleanValue;
                    this.IsDeployed = application["IsDeployed"].BooleanValue;
                    this.IsDigest = application["IsDigest"].BooleanValue;
                    this.IsEnabled = application["IsEnabled"].BooleanValue;
                    this.IsExpired = application["IsExpired"].BooleanValue;
                    this.IsHidden = application["IsHidden"].BooleanValue;
                    this.IsLatest = application["IsLatest"].BooleanValue;
                    this.IsQuarantined = application["IsQuarantined"].BooleanValue;
                    this.IsSuperseded = application["IsSuperseded"].BooleanValue;
                    this.IsSuperseding = application["IsSuperseding"].BooleanValue;
                    this.IsUserDefined = application["IsUserDefined"].BooleanValue;
                    this.LastModifiedBy = application["LastModifiedBy"].StringValue;
                    this.Manufacturer = application["Manufacturer"].StringValue;
                    this.ModelId = application["ModelID"].IntegerValue;
                    this.ModelName = application["ModelName"].StringValue;
                    this.NumberOfDependedTasksequences = application["NumberOfDependentTS"].IntegerValue;
                    this.NumberOfDependentDTs = application["NumberOfDependentDTs"].IntegerValue;
                    this.NumberOfDeploymentTypes = application["NumberOfDeploymentTypes"].IntegerValue;
                    this.NumberOfDeployments = application["NumberOfDeployments"].IntegerValue;
                    this.NumberOfDevicesWithApps = application["NumberOfDevicesWithApp"].IntegerValue;
                    this.NumberOfDevicesWithFailure = application["NumberOfDevicesWithFailure"].IntegerValue;
                    this.NumberOfUsersWithApp = application["NumberOfUsersWithApp"].IntegerValue;
                    this.NumberOfUsersWithAppFailure = application["NumberOfUsersWithFailure"].IntegerValue;
                    this.NumberOfUsersWithRequest = application["NumberOfUsersWithRequest"].IntegerValue;
                    this.NumberOfVirtualEnvironments = application["NumberOfVirtualEnvironments"].IntegerValue;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the application id.
        /// </summary>
        /// <value>
        /// The application id.
        /// </value>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the application unique id.
        /// </summary>
        /// <value>
        /// The application unique id.
        /// </value>
        public string ApplicationUniqueId { get; set; }

        /// <summary>
        /// Gets or sets the type of the application. Assignment must be an integer.
        /// </summary>
        public string ApplicationType
        {
            get
            {
                return Enum.GetName(typeof(ApplicationTypes), this._applicationType);
            }

            set
            {
                if (!int.TryParse(value, out this._applicationType)) this._applicationType = 7;
            }
        }

        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        public int ApplicationVersion { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has content.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has content; otherwise, <c>false</c>.
        /// </value>
        public bool HasContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is bundle.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is bundle; otherwise, <c>false</c>.
        /// </value>
        public bool IsBundle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deployable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is deployable; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeployable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deployed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is deployed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeployed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is digest.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is digest; otherwise, <c>false</c>.
        /// </value>
        public bool IsDigest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is expired.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is expired; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is hidden.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is hidden; otherwise, <c>false</c>.
        /// </value>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is latest.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is latest; otherwise, <c>false</c>.
        /// </value>
        public bool IsLatest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is quarantined.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is quarantined; otherwise, <c>false</c>.
        /// </value>
        public bool IsQuarantined { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is superseded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is superseded; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuperseded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is superseding.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is superseding; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuperseding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is user defined.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is user defined; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserDefined { get; set; }

        /// <summary>
        /// Gets or sets the last modified by.
        /// </summary>
        /// <value>
        /// The last modified by.
        /// </value>
        public string LastModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the application description.
        /// </summary>
        /// <value>
        /// The application description.
        /// </value>
        public string ApplicationDescription { get; set; }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        /// <value>
        /// The manufacturer.
        /// </value>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        /// <value>
        /// The name of the model.
        /// </value>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the model id.
        /// </summary>
        /// <value>
        /// The model id.
        /// </value>
        public int ModelId { get; set; }

        /// <summary>
        /// Gets or sets the number of dependent D ts.
        /// </summary>
        /// <value>
        /// The number of dependent D ts.
        /// </value>
        public int NumberOfDependentDTs { get; set; }

        /// <summary>
        /// Gets or sets the number of depended task sequences.
        /// </summary>
        /// <value>
        /// The number of depended task sequences.
        /// </value>
        public int NumberOfDependedTasksequences { get; set; }

        /// <summary>
        /// Gets or sets the number of deployments.
        /// </summary>
        /// <value>
        /// The number of deployments.
        /// </value>
        public int NumberOfDeployments { get; set; }

        /// <summary>
        /// Gets or sets the number of deployment types.
        /// </summary>
        /// <value>
        /// The number of deployment types.
        /// </value>
        public int NumberOfDeploymentTypes { get; set; }

        /// <summary>
        /// Gets or sets the number of devices with apps.
        /// </summary>
        /// <value>
        /// The number of devices with apps.
        /// </value>
        public int NumberOfDevicesWithApps { get; set; }

        /// <summary>
        /// Gets or sets the number of devices with failure.
        /// </summary>
        /// <value>
        /// The number of devices with failure.
        /// </value>
        public int NumberOfDevicesWithFailure { get; set; }

        /// <summary>
        /// Gets or sets the number of users with app.
        /// </summary>
        /// <value>
        /// The number of users with app.
        /// </value>
        public int NumberOfUsersWithApp { get; set; }

        /// <summary>
        /// Gets or sets the number of users with app failure.
        /// </summary>
        /// <value>
        /// The number of users with app failure.
        /// </value>
        public int NumberOfUsersWithAppFailure { get; set; }

        /// <summary>
        /// Gets or sets the number of users with request.
        /// </summary>
        /// <value>
        /// The number of users with request.
        /// </value>
        public int NumberOfUsersWithRequest { get; set; }

        /// <summary>
        /// Gets or sets the number of virtual environments.
        /// </summary>
        /// <value>
        /// The number of virtual environments.
        /// </value>
        public int NumberOfVirtualEnvironments { get; set; }

        /// <summary>
        /// Creates a new instance of the SMS object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            throw new NotImplementedException();
        }
    }
}
