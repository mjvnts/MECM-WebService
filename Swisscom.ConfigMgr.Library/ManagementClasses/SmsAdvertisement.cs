// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsAdvertisement.cs" company="LANexpert S.A.">
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
    /// The SMS_Advertisement Windows Management Instrumentation (WMI) class
    /// is an SMS Provider server class, in System Center 2012 Configuration Manager,
    /// that represents an advertisement used to announce software package programs
    /// that are available for running on clients.
    /// </summary>
    [Serializable]
    public class SmsAdvertisement : SmsAssignmentBase, ISmsBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsAdvertisement"/> class.
        /// </summary>
        public SmsAdvertisement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsAdvertisement"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="advertisementId">The advertisement id.</param>
        public SmsAdvertisement(WqlConnectionManager connection, string advertisementId)
        {
            using (var advertisement = connection.GetInstance("SMS_Advertisement.AdvertisementId='" + advertisementId + "'"))
            {
                this.ActionInProgress = (AdvertisementActionInProgress)advertisement["ActionInProgress"].IntegerValue;
                this.AdvertFlags = (AdvertisementFlags)advertisement["AdvertFlags"].IntegerValue;
                this.AdvertisementId = advertisement["AdvertisementID"].StringValue;
                if (advertisement["AdvertisementName"].ObjectValue != null) this.AdvertisementName = advertisement["AdvertisementName"].StringValue;
                this.AssignmentScheduleEnabled = advertisement["AssignedScheduleEnabled"].BooleanValue;
                if (this.AssignmentScheduleEnabled)
                {
                    // currently only implemented the first occurence because of time presure....
                    foreach (var resultObject in advertisement.GetArrayItems("AssignedSchedule"))
                    {
                        this.AssigmentSchedule = new SmsScheduleToken(resultObject);
                        break;
                    }

                    this.AssignmentScheduleIsGmt = advertisement["AssignedScheduleIsGMT"].BooleanValue;
                }

                if (advertisement["AssignmentID"].ObjectValue != null) this.AssingmentId = advertisement["AssignmentID"].IntegerValue;
                this.CollectionId = advertisement["CollectionID"].StringValue;
                if (advertisement["Comment"].ObjectValue != null) this.Comment = advertisement["Comment"].StringValue;
                this.DeviceFlag = (DeviceFlags)advertisement["DeviceFlags"].IntegerValue;
                this.ExpirationTimeEnabled = advertisement["ExpirationTimeEnabled"].BooleanValue;
                if (this.ExpirationTimeEnabled)
                {
                    this.ExpirationTime = advertisement["ExpirationTime"].DateTimeValue;
                    this.ExpirationTimeIsGmt = advertisement["ExpirationTimeIsGMT"].BooleanValue;
                }

                if (advertisement["HierarchyPath"].ObjectValue != null) this.HierarchyPath = advertisement["HierarchyPath"].StringValue;
                this.IncludeSubCollection = advertisement["IncludeSubCollection"].BooleanValue;
                if (advertisement["ISVData"].ObjectValue != null) this.IsVData = advertisement["ISVData"].IntegerArrayValue;
                if (advertisement["ISVDataSize"].ObjectValue != null) this.IsVDataSize = advertisement["ISVDataSize"].IntegerValue;
                if (advertisement["MandatoryCountdown"].ObjectValue != null) this.MandatoryCountdown = advertisement["MandatoryCountdown"].IntegerValue;
                this.OfferType = (OfferTypeTypes)advertisement["OfferType"].IntegerValue;
                if (advertisement["PackageID"].ObjectValue != null) this.PackageId = advertisement["PackageID"].StringValue;
                this.PresentTimeEnabled = advertisement["PresentTimeEnabled"].BooleanValue;
                if (this.PresentTimeEnabled)
                {
                    this.PresentTime = advertisement["PresentTime"].DateTimeValue;
                    this.PresentTimeIsGmt = advertisement["PresentTimeIsGMT"].BooleanValue;
                }

                this.Priority = (AdvertisementPriority)advertisement["Priority"].IntegerValue;
                if (advertisement["ProgramName"].ObjectValue != null) this.ProgramName = advertisement["ProgramName"].StringValue;
                this.RemoteClientFlags = (AdvertisementRemoteClientFlags)advertisement["RemoteClientFlags"].IntegerValue;
                this.TimeFlags = (AdvertisementTimeFlags)advertisement["TimeFlags"].IntegerValue;
                this.SourceSite = advertisement["SourceSite"].StringValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsAdvertisement"/> class.
        /// </summary>
        /// <param name="advertisement">The advertisement.</param>
        public SmsAdvertisement(IResultObject advertisement)
        {
            this.ActionInProgress = (AdvertisementActionInProgress)advertisement["ActionInProgress"].IntegerValue;
            this.AdvertFlags = (AdvertisementFlags)advertisement["AdvertFlags"].IntegerValue;
            this.AdvertisementId = advertisement["AdvertisementID"].StringValue;
            if (advertisement["AdvertisementName"].ObjectValue != null) this.AdvertisementName = advertisement["AdvertisementName"].StringValue;
            this.AssignmentScheduleEnabled = advertisement["AssignedScheduleEnabled"].BooleanValue;
            if (this.AssignmentScheduleEnabled)
            {
                // currently only implemented the first occurence because of time presure....
                foreach (var resultObject in advertisement.GetArrayItems("AssignedSchedule"))
                {
                    this.AssigmentSchedule = new SmsScheduleToken(resultObject);
                    break;
                }

                this.AssignmentScheduleIsGmt = advertisement["AssignedScheduleIsGMT"].BooleanValue;
            }

            if (advertisement["AssignmentID"].ObjectValue != null) this.AssingmentId = advertisement["AssignmentID"].IntegerValue;
            this.CollectionId = advertisement["CollectionID"].StringValue;
            if (advertisement["Comment"].ObjectValue != null) this.Comment = advertisement["Comment"].StringValue;
            this.DeviceFlag = (DeviceFlags)advertisement["DeviceFlags"].IntegerValue;
            this.ExpirationTimeEnabled = advertisement["ExpirationTimeEnabled"].BooleanValue;
            if (this.ExpirationTimeEnabled)
            {
                this.ExpirationTime = advertisement["ExpirationTime"].DateTimeValue;
                this.ExpirationTimeIsGmt = advertisement["ExpirationTimeIsGMT"].BooleanValue;
            }

            if (advertisement["HierarchyPath"].ObjectValue != null) this.HierarchyPath = advertisement["HierarchyPath"].StringValue;
            this.IncludeSubCollection = advertisement["IncludeSubCollection"].BooleanValue;
            if (advertisement["ISVData"].ObjectValue != null) this.IsVData = advertisement["ISVData"].IntegerArrayValue;
            if (advertisement["ISVDataSize"].ObjectValue != null) this.IsVDataSize = advertisement["ISVDataSize"].IntegerValue;
            if (advertisement["MandatoryCountdown"].ObjectValue != null) this.MandatoryCountdown = advertisement["MandatoryCountdown"].IntegerValue;
            this.OfferType = (OfferTypeTypes)advertisement["OfferType"].IntegerValue;
            if (advertisement["PackageID"].ObjectValue != null) this.PackageId = advertisement["PackageID"].StringValue;
            this.PresentTimeEnabled = advertisement["PresentTimeEnabled"].BooleanValue;
            if (this.PresentTimeEnabled)
            {
                this.PresentTime = advertisement["PresentTime"].DateTimeValue;
                this.PresentTimeIsGmt = advertisement["PresentTimeIsGMT"].BooleanValue;
            }

            this.Priority = (AdvertisementPriority)advertisement["Priority"].IntegerValue;
            if (advertisement["ProgramName"].ObjectValue != null) this.ProgramName = advertisement["ProgramName"].StringValue;
            this.RemoteClientFlags = (AdvertisementRemoteClientFlags)advertisement["RemoteClientFlags"].IntegerValue;
            this.TimeFlags = (AdvertisementTimeFlags)advertisement["TimeFlags"].IntegerValue;
            this.SourceSite = advertisement["SourceSite"].StringValue;
        }

        /// <summary>
        /// Gets or sets the action in progress.
        /// </summary>
        /// <value>
        /// The action in progress.
        /// </value>
        public AdvertisementActionInProgress ActionInProgress { get; set; }

        /// <summary>
        /// Gets or sets the adver flags.
        /// </summary>
        /// <value>
        /// The adver flags.
        /// </value>
        public AdvertisementFlags AdvertFlags { get; set; }

        /// <summary>
        /// Gets or sets the advertisement id.
        /// </summary>
        /// <value>
        /// The advertisement id.
        /// </value>
        public string AdvertisementId { get; set; }

        /// <summary>
        /// Gets or sets the name of the advertisement.
        /// </summary>
        /// <value>
        /// The name of the advertisement.
        /// </value>
        public string AdvertisementName { get; set; }

        /// <summary>
        /// Gets or sets the assigment schedule.
        /// </summary>
        /// <value>
        /// The assigment schedule.
        /// </value>
        public SmsScheduleToken AssigmentSchedule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [assignment schedule enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [assignment schedule enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool AssignmentScheduleEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [assignment schedule is GMT].
        /// </summary>
        /// <value>
        /// <c>true</c> if [assignment schedule is GMT]; otherwise, <c>false</c>.
        /// </value>
        public bool AssignmentScheduleIsGmt { get; set; }

        /// <summary>
        /// Gets or sets the assingment id.
        /// </summary>
        /// <value>
        /// The assingment id.
        /// </value>
        public int AssingmentId { get; set; }

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        /// <value>
        /// The collection id.
        /// </value>
        public string CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the device flag.
        /// </summary>
        /// <value>
        /// The device flag.
        /// </value>
        public DeviceFlags DeviceFlag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [expiration time enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [expiration time enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpirationTimeEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [expiration time is GMT].
        /// </summary>
        /// <value>
        /// <c>true</c> if [expiration time is GMT]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpirationTimeIsGmt { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy path.
        /// </summary>
        /// <value>
        /// The hierarchy path.
        /// </value>
        public string HierarchyPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include sub collection].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include sub collection]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeSubCollection { get; set; }

        /// <summary>
        /// Gets or sets the is V data.
        /// </summary>
        /// <value>
        /// The is V data.
        /// </value>
        public int[] IsVData { get; set; }

        /// <summary>
        /// Gets or sets the size of the is V data.
        /// </summary>
        /// <value>
        /// The size of the is V data.
        /// </value>
        public int IsVDataSize { get; set; }

        /// <summary>
        /// Gets or sets the mandatory countdown.
        /// </summary>
        /// <value>
        /// The mandatory countdown.
        /// </value>
        public int MandatoryCountdown { get; set; }

        /// <summary>
        /// Gets or sets the package id.
        /// </summary>
        /// <value>
        /// The package id.
        /// </value>
        public string PackageId { get; set; }

        /// <summary>
        /// Gets or sets the present time.
        /// </summary>
        /// <value>
        /// The present time.
        /// </value>
        public DateTime PresentTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [present time enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [present time enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool PresentTimeEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [present time is GMT].
        /// </summary>
        /// <value>
        /// <c>true</c> if [present time is GMT]; otherwise, <c>false</c>.
        /// </value>
        public bool PresentTimeIsGmt { get; set; }

        /// <summary>
        /// Gets or sets the Priority.
        /// </summary>
        /// <value>
        /// The Priority.
        /// </value>
        public AdvertisementPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>
        /// The name of the program.
        /// </value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the remote client flags.
        /// </summary>
        /// <value>
        /// The remote client flags.
        /// </value>
        public AdvertisementRemoteClientFlags RemoteClientFlags { get; set; }

        /// <summary>
        /// Gets or sets the time flags.
        /// </summary>
        /// <value>
        /// The time flags.
        /// </value>
        public AdvertisementTimeFlags TimeFlags { get; set; }

        /// <summary>
        /// Creates a new instance of the SMS object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// The new created object.
        /// </returns>
        public IResultObject CreateInstance(WqlConnectionManager connection)
        {
            var advertisement = connection.CreateInstance("SMS_Advertisement");
            advertisement["AdvertFlags"].IntegerValue = (int)this.AdvertFlags;
            advertisement["AdvertisementName"].StringValue = this.AdvertisementName;
            advertisement["AssignedScheduleEnabled"].BooleanValue = this.AssignmentScheduleEnabled;
            if (this.AssignmentScheduleEnabled)
            {
                var smsTokenObject = this.AssigmentSchedule.CreateInstance(connection);
                var scheduleTokens = new List<IResultObject>();
                scheduleTokens.Add(smsTokenObject);
                advertisement.SetArrayItems("AssignedSchedule", scheduleTokens);
                advertisement["AssignedScheduleIsGMT"].BooleanValue = this.AssignmentScheduleIsGmt;
            }

            advertisement["CollectionID"].StringValue = this.CollectionId;
            advertisement["Comment"].StringValue = this.Comment;
            advertisement["DeviceFlags"].IntegerValue = (int)this.DeviceFlag;
            advertisement["ExpirationTimeEnabled"].BooleanValue = this.ExpirationTimeEnabled;
            if (this.ExpirationTimeEnabled)
            {
                advertisement["ExpirationTime"].DateTimeValue = this.ExpirationTime;
                advertisement["ExpirationTimeIsGMT"].BooleanValue = this.ExpirationTimeIsGmt;
            }

            advertisement["IncludeSubCollection"].BooleanValue = this.IncludeSubCollection;
            advertisement["MandatoryCountdown"].IntegerValue = this.MandatoryCountdown;
            advertisement["OfferType"].IntegerValue = (int)this.OfferType;
            advertisement["PackageID"].StringValue = this.PackageId;
            advertisement["PresentTimeEnabled"].BooleanValue = this.PresentTimeEnabled;
            if (this.PresentTimeEnabled)
            {
                advertisement["PresentTime"].DateTimeValue = this.PresentTime;
                advertisement["PresentTimeIsGMT"].BooleanValue = this.PresentTimeIsGmt;
            }

            advertisement["Priority"].IntegerValue = (int)this.Priority;
            advertisement["ProgramName"].StringValue = this.ProgramName;
            advertisement["RemoteClientFlags"].IntegerValue = (int)this.RemoteClientFlags;
            advertisement.Put();
            advertisement.Get();
            return advertisement;
        }
    }
}
