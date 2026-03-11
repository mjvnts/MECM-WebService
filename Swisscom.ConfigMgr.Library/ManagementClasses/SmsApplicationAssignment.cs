// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsApplicationAssignment.cs" company="LANexpert S.A.">
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
    /// There are certain properties, that contain null so every WMI property
    /// should be checked for a possible null reference first.
    /// To do this, every property is checked with type "ObjectValue".
    /// </summary>
    [Serializable]
    public class SmsApplicationAssignment : SmsAssignmentBase, ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplicationAssignment"/> class.
        /// </summary>
        public SmsApplicationAssignment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplicationAssignment"/> class.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        public SmsApplicationAssignment(WqlResultObject deployment)
        {
            deployment.Get();
            if (deployment["AppModelID"].ObjectValue != null) this.AppModelId = deployment["AppModelID"].IntegerValue;
            if (deployment["ApplicationName"].ObjectValue != null) this.ApplicationName = deployment["ApplicationName"].StringValue;
            if (deployment["ApplyToSubTargets"].ObjectValue != null) this.ApplyToSubTargets = deployment["ApplyToSubTargets"].BooleanValue;
            if (deployment["AssignedCI_UniqueID"].ObjectValue != null) this.AssignedCiUniqueId = deployment["AssignedCI_UniqueID"].StringValue;
            if (deployment["AssignedCIs"].ObjectValue != null) this.AssignedCis = deployment["AssignedCIs"].IntegerArrayValue;
            if (deployment["AssignmentAction"].ObjectValue != null) this.AssignmentAction = (AssignmentActions)deployment["AssignmentAction"].IntegerValue;
            if (deployment["AssignmentDescription"].ObjectValue != null) this.AssignmentDescription = deployment["AssignmentDescription"].StringValue;
            if (deployment["AssignmentID"].ObjectValue != null) this.AssignmentId = deployment["AssignmentID"].IntegerValue;
            if (deployment["AssignmentName"].ObjectValue != null) this.AssignmentName = deployment["AssignmentName"].StringValue;
            if (deployment["AssignmentType"].ObjectValue != null) this.AssignmentType = (AssignmentTypes)deployment["AssignmentType"].IntegerValue;
            if (deployment["AssignmentUniqueID"].ObjectValue != null) this.AssignmentUniqueId = deployment["AssignmentUniqueID"].StringValue;
            if (deployment["CollectionName"].ObjectValue != null) this.CollectionName = deployment["CollectionName"].StringValue;
            if (deployment["ContainsExpiredUpdates"].ObjectValue != null) this.ContainsExpiredUpdates = deployment["ContainsExpiredUpdates"].BooleanValue;
            if (deployment["CreationTime"].ObjectValue != null) this.CreationTime = deployment["CreationTime"].DateTimeValue;
            if (deployment["DesiredConfigType"].ObjectValue != null) this.DesiredConfigType = (DesiredConfigTypes)deployment["DesiredConfigType"].IntegerValue;
            if (deployment["DisableMomAlerts"].ObjectValue != null) this.DisableMomAlerts = deployment["DisableMomAlerts"].BooleanValue;
            if (deployment["DPLocality"].ObjectValue != null) this.DpLocality = (DpLocalities)deployment["DPLocality"].IntegerValue;
            if (deployment["Enabled"].ObjectValue != null) this.Enabled = deployment["Enabled"].BooleanValue;
            if (deployment["EnforcementDeadline"].ObjectValue != null) this.EnforcementDeadline = deployment["EnforcementDeadline"].DateTimeValue;
            if (deployment["EvaluationSchedule"].ObjectValue != null) this.EvaluationSchedule = deployment["EvaluationSchedule"].StringValue;
            if (deployment["ExpirationTime"].ObjectValue != null) this.ExpirationTime = deployment["ExpirationTime"].DateTimeValue;
            if (deployment["LastModificationTime"].ObjectValue != null) this.LastModificationTime = deployment["LastModificationTime"].DateTimeValue;
            if (deployment["LastModifiedBy"].ObjectValue != null) this.LastModifiedBy = deployment["LastModifiedBy"].StringValue;
            if (deployment["LocaleID"].ObjectValue != null) this.LocalId = (LocaleIds)deployment["LocaleID"].IntegerValue;
            if (deployment["LogComplianceToWinEvent"].ObjectValue != null) this.LogComplianceToWinEvent = deployment["LogComplianceToWinEvent"].BooleanValue;
            if (deployment["NonComplianceCriticality"].ObjectValue != null) this.NonComplianceCriticality = deployment["NonComplianceCriticality"].IntegerValue;
            if (deployment["NotifyUser"].ObjectValue != null) this.NotifyUser = deployment["NotifyUser"].BooleanValue;
            if (deployment["OfferFlags"].ObjectValue != null) this.OfferFlag = (OfferFlags)deployment["OfferFlags"].IntegerValue;
            if (deployment["OfferTypeID"].ObjectValue != null) this.OfferType = (OfferTypeTypes)deployment["OfferTypeID"].IntegerValue;
            if (deployment["OverrideServiceWindows"].ObjectValue != null) this.OverrideServiceWindows = deployment["OverrideServiceWindows"].BooleanValue;
            if (deployment["Priority"].ObjectValue != null) this.Priority = (PriorityTypes)deployment["Priority"].IntegerValue;
            if (deployment["RaiseMomAlertsOnFailure"].ObjectValue != null) this.RaiseMomAlertsOnFailure = deployment["RaiseMomAlertsOnFailure"].BooleanValue;
            if (deployment["RebootOutsideOfServiceWindows"].ObjectValue != null) this.RebootOutsideOfServiceWindows = deployment["RebootOutsideOfServiceWindows"].BooleanValue;
            if (deployment["RequireApproval"].ObjectValue != null) this.RequireApproval = deployment["RequireApproval"].BooleanValue;
            if (deployment["SendDetailedNonComplianceStatus"].ObjectValue != null) this.SendDetailedNonComplianceStatus = deployment["SendDetailedNonComplianceStatus"].BooleanValue;
            if (deployment["SourceSite"].ObjectValue != null) this.SourceSite = deployment["SourceSite"].StringValue;
            if (deployment["StartTime"].ObjectValue != null) this.StartTime = deployment["StartTime"].DateTimeValue;
            if (deployment["StateMessagePriority"].ObjectValue != null) this.StateMessagePriority = (StateMessagePrioritites)deployment["StateMessagePriority"].IntegerValue;
            if (deployment["SuppressReboot"].ObjectValue != null) this.SuppressReboot = (SuppressRebooTypes)deployment["SuppressReboot"].IntegerValue;
            if (deployment["TargetCollectionID"].ObjectValue != null) this.TargetCollectionId = deployment["TargetCollectionID"].StringValue;
            if (deployment["UpdateDeadline"].ObjectValue != null) this.UpdateDeadline = deployment["UpdateDeadline"].DateTimeValue;
            if (deployment["UpdateSupersedence"].ObjectValue != null) this.UpdateSupersedence = deployment["UpdateSupersedence"].BooleanValue;
            if (deployment["UseGMTTimes"].ObjectValue != null) this.UseGmtTimes = deployment["UseGMTTimes"].BooleanValue;
            if (deployment["UserUIExperience"].ObjectValue != null) this.UserUiExperience = deployment["UserUIExperience"].BooleanValue;
            if (deployment["WoLEnabled"].ObjectValue != null) this.WoLEnabled = deployment["WoLEnabled"].BooleanValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplicationAssignment"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="deploymentId">The deployment id.</param>
        public SmsApplicationAssignment(WqlConnectionManager connection, string deploymentId)
        {
            using (var deployment = connection.GetInstance("SMS_ApplicationAssignment.AssignmentId='" + deploymentId + "'"))
            {
                deployment.Get();
                if (deployment["AppModelID"].ObjectValue != null) this.AppModelId = deployment["AppModelID"].IntegerValue;
                if (deployment["ApplicationName"].ObjectValue != null) this.ApplicationName = deployment["ApplicationName"].StringValue;
                if (deployment["ApplyToSubTargets"].ObjectValue != null) this.ApplyToSubTargets = deployment["ApplyToSubTargets"].BooleanValue;
                if (deployment["AssignedCI_UniqueID"].ObjectValue != null) this.AssignedCiUniqueId = deployment["AssignedCI_UniqueID"].StringValue;
                if (deployment["AssignedCIs"].ObjectValue != null) this.AssignedCis = deployment["AssignedCIs"].IntegerArrayValue;
                if (deployment["AssignmentAction"].ObjectValue != null) this.AssignmentAction = (AssignmentActions)deployment["AssignmentAction"].IntegerValue;
                if (deployment["AssignmentDescription"].ObjectValue != null) this.AssignmentDescription = deployment["AssignmentDescription"].StringValue;
                if (deployment["AssignmentID"].ObjectValue != null) this.AssignmentId = deployment["AssignmentID"].IntegerValue;
                if (deployment["AssignmentName"].ObjectValue != null) this.AssignmentName = deployment["AssignmentName"].StringValue;
                if (deployment["AssignmentType"].ObjectValue != null) this.AssignmentType = (AssignmentTypes)deployment["AssignmentType"].IntegerValue;
                if (deployment["AssignmentUniqueID"].ObjectValue != null) this.AssignmentUniqueId = deployment["AssignmentUniqueID"].StringValue;
                if (deployment["CollectionName"].ObjectValue != null) this.CollectionName = deployment["CollectionName"].StringValue;
                if (deployment["ContainsExpiredUpdates"].ObjectValue != null) this.ContainsExpiredUpdates = deployment["ContainsExpiredUpdates"].BooleanValue;
                if (deployment["CreationTime"].ObjectValue != null) this.CreationTime = deployment["CreationTime"].DateTimeValue;
                if (deployment["DesiredConfigType"].ObjectValue != null) this.DesiredConfigType = (DesiredConfigTypes)deployment["DesiredConfigType"].IntegerValue;
                if (deployment["DisableMomAlerts"].ObjectValue != null) this.DisableMomAlerts = deployment["DisableMomAlerts"].BooleanValue;
                if (deployment["DPLocality"].ObjectValue != null) this.DpLocality = (DpLocalities)deployment["DPLocality"].IntegerValue;
                if (deployment["Enabled"].ObjectValue != null) this.Enabled = deployment["Enabled"].BooleanValue;
                if (deployment["EnforcementDeadline"].ObjectValue != null) this.EnforcementDeadline = deployment["EnforcementDeadline"].DateTimeValue;
                if (deployment["EvaluationSchedule"].ObjectValue != null) this.EvaluationSchedule = deployment["EvaluationSchedule"].StringValue;
                if (deployment["ExpirationTime"].ObjectValue != null) this.ExpirationTime = deployment["ExpirationTime"].DateTimeValue;
                if (deployment["LastModificationTime"].ObjectValue != null) this.LastModificationTime = deployment["LastModificationTime"].DateTimeValue;
                if (deployment["LastModifiedBy"].ObjectValue != null) this.LastModifiedBy = deployment["LastModifiedBy"].StringValue;
                if (deployment["LocaleID"].ObjectValue != null) this.LocalId = (LocaleIds)deployment["LocaleID"].IntegerValue;
                if (deployment["LogComplianceToWinEvent"].ObjectValue != null) this.LogComplianceToWinEvent = deployment["LogComplianceToWinEvent"].BooleanValue;
                if (deployment["NonComplianceCriticality"].ObjectValue != null) this.NonComplianceCriticality = deployment["NonComplianceCriticality"].IntegerValue;
                if (deployment["NotifyUser"].ObjectValue != null) this.NotifyUser = deployment["NotifyUser"].BooleanValue;
                if (deployment["OfferFlags"].ObjectValue != null) this.OfferFlag = (OfferFlags)deployment["OfferFlags"].IntegerValue;
                if (deployment["OfferTypeID"].ObjectValue != null) this.OfferType = (OfferTypeTypes)deployment["OfferTypeID"].IntegerValue;
                if (deployment["OverrideServiceWindows"].ObjectValue != null) this.OverrideServiceWindows = deployment["OverrideServiceWindows"].BooleanValue;
                if (deployment["Priority"].ObjectValue != null) this.Priority = (PriorityTypes)deployment["Priority"].IntegerValue;
                if (deployment["RaiseMomAlertsOnFailure"].ObjectValue != null) this.RaiseMomAlertsOnFailure = deployment["RaiseMomAlertsOnFailure"].BooleanValue;
                if (deployment["RebootOutsideOfServiceWindows"].ObjectValue != null) this.RebootOutsideOfServiceWindows = deployment["RebootOutsideOfServiceWindows"].BooleanValue;
                if (deployment["RequireApproval"].ObjectValue != null) this.RequireApproval = deployment["RequireApproval"].BooleanValue;
                if (deployment["SendDetailedNonComplianceStatus"].ObjectValue != null) this.SendDetailedNonComplianceStatus = deployment["SendDetailedNonComplianceStatus"].BooleanValue;
                if (deployment["SourceSite"].ObjectValue != null) this.SourceSite = deployment["SourceSite"].StringValue;
                if (deployment["StartTime"].ObjectValue != null) this.StartTime = deployment["StartTime"].DateTimeValue;
                if (deployment["StateMessagePriority"].ObjectValue != null) this.StateMessagePriority = (StateMessagePrioritites)deployment["StateMessagePriority"].IntegerValue;
                if (deployment["SuppressReboot"].ObjectValue != null) this.SuppressReboot = (SuppressRebooTypes)deployment["SuppressReboot"].IntegerValue;
                if (deployment["TargetCollectionID"].ObjectValue != null) this.TargetCollectionId = deployment["TargetCollectionID"].StringValue;
                if (deployment["UpdateDeadline"].ObjectValue != null) this.UpdateDeadline = deployment["UpdateDeadline"].DateTimeValue;
                if (deployment["UpdateSupersedence"].ObjectValue != null) this.UpdateSupersedence = deployment["UpdateSupersedence"].BooleanValue;
                if (deployment["UseGMTTimes"].ObjectValue != null) this.UseGmtTimes = deployment["UseGMTTimes"].BooleanValue;
                if (deployment["UserUIExperience"].ObjectValue != null) this.UserUiExperience = deployment["UserUIExperience"].BooleanValue;
                if (deployment["WoLEnabled"].ObjectValue != null) this.WoLEnabled = deployment["WoLEnabled"].BooleanValue;
            }
        }

        /// <summary>
        /// Offer flags.
        /// </summary>
        [Serializable]
        public enum OfferFlags
        {
            /// <summary>
            /// The offer is set to default.
            /// </summary>
            Default = 0,

            /// <summary>
            /// The offer is predeploy.
            /// </summary>
            PreDeploy = 1
        }

        /// <summary>
        /// Action associated with the configuration item assignment.
        /// </summary>
        [Serializable]
        public enum AssignmentActions
        {
            /// <summary>
            /// Assignment action is detect.
            /// </summary>
            Detect = 1,

            /// <summary>
            /// Assignment action is apply.
            /// </summary>
            Apply = 2
        }

        /// <summary>
        /// Type of assignment.
        /// </summary>
        [Serializable]
        public enum AssignmentTypes
        {
            /// <summary>
            /// Assignment is DCM baseline.
            /// </summary>
            CiaTypeDcmBaseline = 0,

            /// <summary>
            /// Assignment is updates.
            /// </summary>
            CiaTypeUpdates = 1,

            /// <summary>
            /// Assignment is application.
            /// </summary>
            CiaTypeApplication = 2,

            /// <summary>
            /// Assignment is update group.
            /// </summary>
            CiaTypeUpdateGroup = 5,

            /// <summary>
            /// Assignment is policy.
            /// </summary>
            CiaTypePolicy = 8
        }

        /// <summary>
        /// The type of the configuration item.
        /// </summary>
        [Serializable]
        public enum DesiredConfigTypes
        {
            /// <summary>
            /// Config type is required.
            /// </summary>
            Required = 1,

            /// <summary>
            /// Config type is not allowed.
            /// </summary>
            NotAllowed = 2
        }

        /// <summary>
        /// Flags that determine how the client obtains distribution points, according to distribution point locality.
        /// </summary>
        [Serializable]
        public enum DpLocalities
        {
            /// <summary>
            /// DP locality is load from local.
            /// </summary>
            DpDownloadFromLocal = 4,

            /// <summary>
            /// DP locality is load from remote.
            /// </summary>
            DpDownloadFromRemote = 6,

            /// <summary>
            /// DP locality is fallback unprotected.
            /// </summary>
            DpNoFallbakUnprotected = 17,

            /// <summary>
            /// DP locallity is allow WUMU.
            /// </summary>
            DpAllowWumu = 18,

            /// <summary>
            /// DP locality is allow metered network.
            /// </summary>
            DpAllowMeteredNetwork = 19,

            /// <summary>
            /// DP locality is default.
            /// </summary>
            DpDefault = 80
        }

        /// <summary>
        /// Locale IDs, default is EnglishUs.
        /// </summary>
        [Serializable]
        public enum LocaleIds
        {
            /// <summary>
            /// English US.
            /// </summary>
            EnglishUs = 1033,

            /// <summary>
            /// German Switzerland.
            /// </summary>
            GermandSwitzerland = 2055
        }

        /// <summary>
        /// Priority for installation of the application.
        /// </summary>
        [Serializable]
        public enum PriorityTypes
        {
            /// <summary>
            /// Priority is low.
            /// </summary>
            Low = 0,

            /// <summary>
            /// Priority is medium.
            /// </summary>
            Medium = 1,

            /// <summary>
            /// Priority is high.
            /// </summary>
            High = 2
        }

        /// <summary>
        /// Value indicating whether the client should not reboot the computer, if there is a reboot pending after the configuration item is applied.
        /// </summary>
        [Serializable]
        public enum SuppressRebooTypes
        {
            /// <summary>
            /// Suppresses reboots on workstations.
            /// </summary>
            SuppressRebootWorkstations = 0,

            /// <summary>
            /// Suppresses reboots on servers.
            /// </summary>
            SuppressRebootServers = 1
        }

        /// <summary>
        /// Priority of state message to be reported from client. The default value is 5.
        /// </summary>
        [Serializable]
        public enum StateMessagePrioritites
        {
            /// <summary>
            /// Urgent Priority.
            /// </summary>
            Urgent = 0,

            /// <summary>
            /// High Priority.
            /// </summary>
            High = 1,

            /// <summary>
            /// Normal Priority.
            /// </summary>
            Normal = 5,

            /// <summary>
            /// Low Priority.
            /// </summary>
            Low = 10
        }

        /// <summary>
        /// How the user will be notified. Is not stored in WMI class itself.
        /// </summary>
        [Serializable]
        public enum UserNotificationBehaviourTypes
        {
            /// <summary>
            /// Hide in Software Center and all notifications:
            /// <list type="bullet">
            /// <item><description>UserUIExperience = False</description></item>
            /// <item><description>NotifyUser = False</description></item>
            /// </list>
            /// </summary>
            UserNotificationHideInSoftwareCenter = 0,

            /// <summary>
            /// Display in Software Center and show all notifications:
            /// <list type="bullet">
            /// <item><description>UserUIExperience = True</description></item>
            /// <item><description>NotifyUser = True</description></item>
            /// </list>
            /// </summary>
            UserNotificationDisplayInSoftwareCenterAllNotifications = 1,

            /// <summary>
            /// Display in Software Center and only show notifications for restarts:
            /// <list type="bullet">
            /// <item><description>UserUIExperience = True</description></item>
            /// <item><description>NotifyUser = False</description></item>
            /// </list>
            /// </summary>
            UserNotificationDisplayInSoftwareCenterAndRebootsOnly = 2,
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [apply to sub targets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [apply to sub targets]; otherwise, <c>false</c>.
        /// </value>
        public bool ApplyToSubTargets { get; set; }

        /// <summary>
        /// Gets or sets the app model id.
        /// </summary>
        /// <value>
        /// The app model id.
        /// </value>
        public int AppModelId { get; set; }

        /// <summary>
        /// Gets or sets the assigned ci unique id.
        /// </summary>
        /// <value>
        /// The assigned ci unique id.
        /// </value>
        public string AssignedCiUniqueId { get; set; }

        /// <summary>
        /// Gets or sets the assigned cis.
        /// </summary>
        /// <value>
        /// The assigned cis.
        /// </value>
        public int[] AssignedCis { get; set; }

        /// <summary>
        /// Gets or sets the assignment action.
        /// </summary>
        /// <value>
        /// The assignment action.
        /// </value>
        public AssignmentActions AssignmentAction { get; set; }

        /// <summary>
        /// Gets or sets the assignment description.
        /// </summary>
        /// <value>
        /// The assignment description.
        /// </value>
        public string AssignmentDescription { get; set; }

        /// <summary>
        /// Gets or sets the name of the assignment.
        /// </summary>
        /// <value>
        /// The name of the assignment.
        /// </value>
        public string AssignmentName { get; set; }

        /// <summary>
        /// Gets or sets the type of the assignment.
        /// </summary>
        /// <value>
        /// The type of the assignment.
        /// </value>
        public AssignmentTypes AssignmentType { get; set; }

        /// <summary>
        /// Gets or sets the assignment unique id.
        /// </summary>
        /// <value>
        /// The assignment unique id.
        /// </value>
        public string AssignmentUniqueId { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [contains expired updates].
        /// </summary>
        /// <value><c>true</c> if [contains expired updates]; otherwise, <c>false</c>.</value>
        public bool ContainsExpiredUpdates { get; set; }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the desired config.
        /// </summary>
        /// <value>
        /// The type of the desired config.
        /// </value>
        public DesiredConfigTypes DesiredConfigType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable mom alerts].
        /// </summary>
        /// <value><c>true</c> if [disable mom alerts]; otherwise, <c>false</c>.</value>
        public bool DisableMomAlerts { get; set; }

        /// <summary>
        /// Gets or sets the DP locality.
        /// </summary>
        /// <value>
        /// The DP locality.
        /// </value>
        public DpLocalities DpLocality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SmsApplicationAssignment"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the enforcement deadline.
        /// </summary>
        /// <value>
        /// The enforcement deadline.
        /// </value>
        public DateTime EnforcementDeadline { get; set; }

        /// <summary>
        /// Gets or sets the evaluation schedule.
        /// </summary>
        /// <value>
        /// The evaluation schedule.
        /// </value>
        public string EvaluationSchedule { get; set; }

        /// <summary>
        /// Gets or sets the last modification time.
        /// </summary>
        /// <value>
        /// The last modification time.
        /// </value>
        public DateTime LastModificationTime { get; set; }

        /// <summary>
        /// Gets or sets the last modified by.
        /// </summary>
        /// <value>
        /// The last modified by.
        /// </value>
        public string LastModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the local id.
        /// </summary>
        /// <value>
        /// The local id.
        /// </value>
        public LocaleIds LocalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [log compliance to win event].
        /// </summary>
        /// <value><c>true</c> if [log compliance to win event]; otherwise, <c>false</c>.</value>
        public bool LogComplianceToWinEvent { get; set; }

        /// <summary>
        /// Gets or sets the non compliance criticality.
        /// </summary>
        /// <value>
        /// The non compliance criticality.
        /// </value>
        public int NonComplianceCriticality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [notify user].
        /// </summary>
        /// <value><c>true</c> if [notify user]; otherwise, <c>false</c>.</value>
        public bool NotifyUser { get; set; }

        /// <summary>
        /// Gets or sets the offer flag.
        /// </summary>
        /// <value>
        /// The offer flag.
        /// </value>
        public OfferFlags OfferFlag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [override service windows].
        /// </summary>
        /// <value><c>true</c> if [override service windows]; otherwise, <c>false</c>.</value>
        public bool OverrideServiceWindows { get; set; }

        /// <summary>
        /// Gets or sets the Priority.
        /// </summary>
        /// <value>
        /// The Priority.
        /// </value>
        public PriorityTypes Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [raise mom alerts on failure].
        /// </summary>
        /// <value><c>true</c> if [raise mom alerts on failure]; otherwise, <c>false</c>.</value>
        public bool RaiseMomAlertsOnFailure { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reboot outside of service windows].
        /// </summary>
        /// <value><c>true</c> if [reboot outside of service windows]; otherwise, <c>false</c>.</value>
        public bool RebootOutsideOfServiceWindows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [require approval].
        /// </summary>
        /// <value><c>true</c> if [require approval]; otherwise, <c>false</c>.</value>
        public bool RequireApproval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send detailed non compliance status].
        /// </summary>
        /// <value><c>true</c> if [send detailed non compliance status]; otherwise, <c>false</c>.</value>
        public bool SendDetailedNonComplianceStatus { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the state message Priority.
        /// </summary>
        /// <value>
        /// The state message Priority.
        /// </value>
        public StateMessagePrioritites StateMessagePriority { get; set; }

        /// <summary>
        /// Gets or sets the suppress reboot.
        /// </summary>
        /// <value>
        /// The suppress reboot.
        /// </value>
        public SuppressRebooTypes SuppressReboot { get; set; }

        /// <summary>
        /// Gets or sets the target collection id.
        /// </summary>
        /// <value>
        /// The target collection id.
        /// </value>
        public string TargetCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the update deadline.
        /// </summary>
        /// <value>
        /// The update deadline.
        /// </value>
        public DateTime UpdateDeadline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [update supersedence].
        /// </summary>
        /// <value><c>true</c> if [update supersedence]; otherwise, <c>false</c>.</value>
        public bool UpdateSupersedence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use GMT times].
        /// </summary>
        /// <value><c>true</c> if [use GMT times]; otherwise, <c>false</c>.</value>
        public bool UseGmtTimes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [user UI experience].
        /// </summary>
        /// <value><c>true</c> if [user UI experience]; otherwise, <c>false</c>.</value>
        public bool UserUiExperience { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [wo L enabled].
        /// </summary>
        /// <value><c>true</c> if [wo L enabled]; otherwise, <c>false</c>.</value>
        public bool WoLEnabled { get; set; }

        /// <summary>
        /// Creates a new instance of the SMS object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            var deployment = connection.CreateInstance("SMS_ApplicationAssignment");
            deployment["ApplicationName"].StringValue = this.ApplicationName;
            deployment["ApplyToSubTargets"].BooleanValue = this.ApplyToSubTargets;
            deployment["AssignedCIs"].IntegerArrayValue = this.AssignedCis;
            deployment["AssignmentAction"].IntegerValue = (int)this.AssignmentAction;
            deployment["AssignmentDescription"].StringValue = this.AssignmentDescription;
            deployment["AssignmentName"].StringValue = this.AssignmentName;
            deployment["AssignmentType"].IntegerValue = (int)this.AssignmentType;
            deployment["CollectionName"].StringValue = this.CollectionName;
            deployment["ContainsExpiredUpdates"].BooleanValue = this.ContainsExpiredUpdates;
            deployment["CreationTime"].DateTimeValue = DateTime.Now;
            deployment["DesiredConfigType"].IntegerValue = (int)this.DesiredConfigType;
            deployment["DisableMomAlerts"].BooleanValue = this.DisableMomAlerts;
            deployment["DPLocality"].IntegerValue = (int)this.DpLocality;
            deployment["Enabled"].BooleanValue = this.Enabled;
            if (this.EnforcementDeadline != DateTime.MinValue) deployment["EnforcementDeadline"].DateTimeValue = this.EnforcementDeadline;
            //// deployment["EvaluationSchedule"].StringValue = this.EvaluationSchedule; -> value is always null
            if (this.ExpirationTime != DateTime.MinValue) deployment["ExpirationTime"].DateTimeValue = this.ExpirationTime;
            deployment["LocaleID"].IntegerValue = (int)this.LocalId;
            deployment["LogComplianceToWinEvent"].BooleanValue = this.LogComplianceToWinEvent;
            //// deployment["NonComplianceCriticality"].IntegerValue = this.NonComplianceCriticality; -> value is always null
            deployment["NotifyUser"].BooleanValue = this.NotifyUser;
            deployment["OfferFlags"].IntegerValue = (int)this.OfferFlag;
            deployment["OfferTypeID"].IntegerValue = (int)this.OfferType;
            deployment["OverrideServiceWindows"].BooleanValue = this.OverrideServiceWindows;
            deployment["Priority"].IntegerValue = (int)this.Priority;
            deployment["RaiseMomAlertsOnFailure"].BooleanValue = this.RaiseMomAlertsOnFailure;
            deployment["RebootOutsideOfServiceWindows"].BooleanValue = this.RebootOutsideOfServiceWindows;
            deployment["RequireApproval"].BooleanValue = this.RequireApproval;
            deployment["SendDetailedNonComplianceStatus"].BooleanValue = this.SendDetailedNonComplianceStatus;
            deployment["SourceSite"].StringValue = this.SourceSite;
            deployment["StartTime"].DateTimeValue = this.StartTime;
            deployment["StateMessagePriority"].IntegerValue = (int)this.StateMessagePriority;
            deployment["SuppressReboot"].IntegerValue = (int)this.SuppressReboot;
            deployment["TargetCollectionID"].StringValue = this.TargetCollectionId;
            if (this.UpdateDeadline != DateTime.MinValue) deployment["UpdateDeadline"].DateTimeValue = this.UpdateDeadline;
            deployment["UpdateSupersedence"].BooleanValue = this.UpdateSupersedence;
            deployment["UseGMTTimes"].BooleanValue = this.UseGmtTimes;
            deployment["UserUIExperience"].BooleanValue = this.UserUiExperience;
            deployment["WoLEnabled"].BooleanValue = this.WoLEnabled;
            deployment.Put();
            deployment.Get();
            return deployment;
        }
    }
}
