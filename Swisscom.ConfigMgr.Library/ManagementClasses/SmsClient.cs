// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsClient.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;

    using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class SmsClient : SmsResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsClient"/> class.
        /// </summary>
        public SmsClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsClient"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public SmsClient(WqlResultObject client)
        {
            if (client["Active"].ObjectValue != null) this.Active = (ClientActiveStates)client["Active"].IntegerValue;
            if (client["ADSiteName"].ObjectValue != null) this.AdSiteName = client["ADSiteName"].StringValue;
            if (client["AlwaysInternet"].ObjectValue != null) this.AlwaysInternet = client["AlwaysInternet"].IntegerValue;
            if (client["Client"].ObjectValue != null) this.Client = (ClientSoftwareStates)client["Client"].IntegerValue;
            if (client["ClientType"].ObjectValue != null) this.ClientType = (ClientTypes)client["ClientType"].IntegerValue;
            if (client["ClientVersion"].ObjectValue != null) this.ClientVersion = client["ClientVersion"].StringValue;
            if (client["CPUType"].ObjectValue != null) this.CpuType = client["CPUType"].StringValue;
            if (client["CreationDate"].ObjectValue != null) this.CreationDate = client["CreationDate"].DateTimeValue;
            if (client["Decommissioned"].ObjectValue != null) this.Decommissioned = client["Decommissioned"].IntegerValue;
            if (client["DistinguishedName"].ObjectValue != null) this.DistinguishedName = client["DistinguishedName"].StringValue;
            if (client["HardwareID"].ObjectValue != null) this.HardwareId = client["HardwareID"].StringValue;
            if (client["SMSInstalledSites"].ObjectValue != null) this.InstalledSites = client["SMSInstalledSites"].StringArrayValue;
            if (client["InternetEnabled"].ObjectValue != null) this.InternetEnabled = client["InternetEnabled"].IntegerValue;
            if (client["IPAddresses"].ObjectValue != null) this.IpAddresses = client["IPAddresses"].StringArrayValue;
            if (client["IPSubnets"].ObjectValue != null) this.IpSubnets = client["IPSubnets"].StringArrayValue;
            if (client["IsAssignedToUser"].ObjectValue != null) this.IsAssignedToUser = client["IsAssignedToUser"].BooleanValue;
            if (client["IsMachineChangesPersisted"].ObjectValue != null) this.IsMachineChangesPersisted = client["IsMachineChangesPersisted"].BooleanValue;
            if (client["IsVirtualMachine"].ObjectValue != null) this.IsVirtualMachine = client["IsVirtualMachine"].BooleanValue;
            if (client["IsWriteFilterCapable"].ObjectValue != null) this.IsWriteFilterCapable = client["IsWriteFilterCapable"].BooleanValue;
            if (client["LastLogonTimeStamp"].ObjectValue != null) this.LastLogon = client["LastLogonTimestamp"].DateTimeValue;
            if (client["LastLogonUserDomain"].ObjectValue != null) this.LastLogonUserDomain = client["LastLogonUserDomain"].StringValue;
            if (client["LastLogonUserName"].ObjectValue != null) this.LastLogonUserName = client["LastLogonUserName"].StringValue;
            if (client["MACAddresses"].ObjectValue != null) this.MacAddresses = client["MACAddresses"].StringArrayValue;
            if (client["Name"].ObjectValue != null) this.Name = client["Name"].StringValue;
            if (client["NetbiosName"].ObjectValue != null) this.NetbiosName = client["NetbiosName"].StringValue;
            if (client["Obsolete"].ObjectValue != null) this.Obsolte = (ObsoleteStates)client["Obsolete"].IntegerValue;
            if (client["OperatingSystemNameandVersion"].ObjectValue != null) this.OperatingSystemNamedVersion = client["OperatingSystemNameandVersion"].StringValue;
            if (client["PreviousSMSUUID"].ObjectValue != null) this.PrevioussmsUuid = client["PreviousSMSUUID"].StringValue;
            if (client["SMSResidentSites"].ObjectValue != null) this.ResidentSites = client["SMSResidentSites"].StringArrayValue;
            if (client["ResourceDomainORWorkgroup"].ObjectValue != null) this.ResourceDomainOrWorkgroup = client["ResourceDomainORWorkgroup"].StringValue;
            if (client["ResourceType"].ObjectValue != null) this.ResourceType = (ResourceTypes)client["ResourceType"].IntegerValue;
            if (client["ResourceId"].ObjectValue != null) this.ResourceId = client["ResourceId"].IntegerValue;
            if (client["ResourceNames"].ObjectValue != null) this.ResourceNames = client["ResourceNames"].StringArrayValue;
            if (client["SecurityGroupName"].ObjectValue != null) this.SecurityGroupNames = client["SecurityGroupName"].StringArrayValue;
            if (client["SMBIOSGUID"].ObjectValue != null) this.SmBiosGuid = client["SMBIOSGUID"].StringValue;
            if (client["SMSUniqueIdentifier"].ObjectValue != null) this.SmsUniqueIdentifier = client["SMSUniqueIdentifier"].StringValue;
            if (client["SystemGroupName"].ObjectValue != null) this.SystemGroupName = client["SystemGroupName"].StringArrayValue;
            if (client["SystemOUName"].ObjectValue != null) this.SystemOuName = client["SystemOUName"].StringArrayValue;
            if (client["SystemRoles"].ObjectValue != null) this.SystemRoles = client["SystemRoles"].StringArrayValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsClient"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="resourceId">The resource id.</param>
        public SmsClient(WqlConnectionManager connection, int resourceId)
        {
            using (var client = connection.GetInstance("SMS_R_SYSTEM.ResourceID=" + resourceId))
            {
                if (client["Active"].ObjectValue != null) this.Active = (ClientActiveStates)client["Active"].IntegerValue;
                if (client["ADSiteName"].ObjectValue != null) this.AdSiteName = client["ADSiteName"].StringValue;
                if (client["AlwaysInternet"].ObjectValue != null) this.AlwaysInternet = client["AlwaysInternet"].IntegerValue;
                if (client["Client"].ObjectValue != null) this.Client = (ClientSoftwareStates)client["Client"].IntegerValue;
                if (client["ClientType"].ObjectValue != null) this.ClientType = (ClientTypes)client["ClientType"].IntegerValue;
                if (client["ClientVersion"].ObjectValue != null) this.ClientVersion = client["ClientVersion"].StringValue;
                if (client["CPUType"].ObjectValue != null) this.CpuType = client["CPUType"].StringValue;
                if (client["CreationDate"].ObjectValue != null) this.CreationDate = client["CreationDate"].DateTimeValue;
                if (client["Decommissioned"].ObjectValue != null) this.Decommissioned = client["Decommissioned"].IntegerValue;
                if (client["DistinguishedName"].ObjectValue != null) this.DistinguishedName = client["DistinguishedName"].StringValue;
                if (client["HardwareID"].ObjectValue != null) this.HardwareId = client["HardwareID"].StringValue;
                if (client["SMSInstalledSites"].ObjectValue != null) this.InstalledSites = client["SMSInstalledSites"].StringArrayValue;
                if (client["InternetEnabled"].ObjectValue != null) this.InternetEnabled = client["InternetEnabled"].IntegerValue;
                if (client["IPAddresses"].ObjectValue != null) this.IpAddresses = client["IPAddresses"].StringArrayValue;
                if (client["IPSubnets"].ObjectValue != null) this.IpSubnets = client["IPSubnets"].StringArrayValue;
                if (client["IsAssignedToUser"].ObjectValue != null) this.IsAssignedToUser = client["IsAssignedToUser"].BooleanValue;
                if (client["IsMachineChangesPersisted"].ObjectValue != null) this.IsMachineChangesPersisted = client["IsMachineChangesPersisted"].BooleanValue;
                if (client["IsVirtualMachine"].ObjectValue != null) this.IsVirtualMachine = client["IsVirtualMachine"].BooleanValue;
                if (client["IsWriteFilterCapable"].ObjectValue != null) this.IsWriteFilterCapable = client["IsWriteFilterCapable"].BooleanValue;
                if (client["LastLogonTimeStamp"].ObjectValue != null) this.LastLogon = client["LastLogonTimestamp"].DateTimeValue;
                if (client["LastLogonUserDomain"].ObjectValue != null) this.LastLogonUserDomain = client["LastLogonUserDomain"].StringValue;
                if (client["LastLogonUserName"].ObjectValue != null) this.LastLogonUserName = client["LastLogonUserName"].StringValue;
                if (client["MACAddresses"].ObjectValue != null) this.MacAddresses = client["MACAddresses"].StringArrayValue;
                if (client["Name"].ObjectValue != null) this.Name = client["Name"].StringValue;
                if (client["NetbiosName"].ObjectValue != null) this.NetbiosName = client["NetbiosName"].StringValue;
                if (client["Obsolete"].ObjectValue != null) this.Obsolte = (ObsoleteStates)client["Obsolete"].IntegerValue;
                if (client["OperatingSystemNameandVersion"].ObjectValue != null) this.OperatingSystemNamedVersion = client["OperatingSystemNameandVersion"].StringValue;
                if (client["PreviousSMSUUID"].ObjectValue != null) this.PrevioussmsUuid = client["PreviousSMSUUID"].StringValue;
                if (client["SMSResidentSites"].ObjectValue != null) this.ResidentSites = client["SMSResidentSites"].StringArrayValue;
                if (client["ResourceDomainORWorkgroup"].ObjectValue != null) this.ResourceDomainOrWorkgroup = client["ResourceDomainORWorkgroup"].StringValue;
                if (client["ResourceType"].ObjectValue != null) this.ResourceType = (ResourceTypes)client["ResourceType"].IntegerValue;
                if (client["ResourceId"].ObjectValue != null) this.ResourceId = client["ResourceId"].IntegerValue;
                if (client["ResourceNames"].ObjectValue != null) this.ResourceNames = client["ResourceNames"].StringArrayValue;
                if (client["SecurityGroupName"].ObjectValue != null) this.SecurityGroupNames = client["SecurityGroupName"].StringArrayValue;
                if (client["SMBIOSGUID"].ObjectValue != null) this.SmBiosGuid = client["SMBIOSGUID"].StringValue;
                if (client["SMSUniqueIdentifier"].ObjectValue != null) this.SmsUniqueIdentifier = client["SMSUniqueIdentifier"].StringValue;
                if (client["SystemGroupName"].ObjectValue != null) this.SystemGroupName = client["SystemGroupName"].StringArrayValue;
                if (client["SystemOUName"].ObjectValue != null) this.SystemOuName = client["SystemOUName"].StringArrayValue;
                if (client["SystemRoles"].ObjectValue != null) this.SystemRoles = client["SystemRoles"].StringArrayValue;
            }
        }

        /// <summary>
        /// Flag that indicates the state of the client on the network.
        /// Although it is usually set to 1, this flag is set to 0 by the client health 
        /// tools when it is determined that the client is not healthy or not actively participating on the network.
        /// </summary>
        [Serializable]
        public enum ClientActiveStates
        {
            /// <summary>
            /// Client is not active.
            /// </summary>
            NotActive = 0,

            /// <summary>
            /// Client is active.
            /// </summary>
            Active = 1
        }

        /// <summary>
        /// Value that indicates whether a computer has System Center 2012 Configuration Manager client software installed.
        /// </summary>
        [Serializable]
        public enum ClientSoftwareStates
        {
            /// <summary>
            /// A computer that has no client software installed.
            /// </summary>
            NoClientSoftwareInstalled = 0,

            /// <summary>
            /// A computer that has client software installed.
            /// </summary>
            ClientSoftwareInstalled = 1
        }

        /// <summary>
        /// The type of the client that is installed on the computer.
        /// </summary>
        [Serializable]
        public enum ClientTypes
        {
            /// <summary>
            /// Legacy client.
            /// </summary>
            Legacy = 0,

            /// <summary>
            /// Advanced client.
            /// </summary>
            AdvancedClient = 1,

            /// <summary>
            /// Device client.
            /// </summary>
            DeviceClient = 3
        }

        /// <summary>
        /// Value identifying the state of the record. Although it is usually set to 0,
        /// this value is set to 1 when the server detects that the record has been superseded
        /// by another record for the same computer. If several records have the same HardwareID value 
        /// (same computer), the older records are marked as obsolete.
        /// </summary>
        [Serializable]
        public enum ObsoleteStates
        {
            /// <summary>
            /// Client is not obsolete.
            /// </summary>
            NotObsolete = 0,

            /// <summary>
            /// Client is obsolete.
            /// </summary>
            Obsolete = 1
        }

        /// <summary>
        /// Gets or sets the active.
        /// </summary>
        /// <value>
        /// The active.
        /// </value>
        public ClientActiveStates Active { get; set; }

        /// <summary>
        /// Gets or sets the name of the ad site.
        /// </summary>
        /// <value>
        /// The name of the ad site.
        /// </value>
        public string AdSiteName { get; set; }

        /// <summary>
        /// Gets or sets the always internet.
        /// </summary>
        /// <value>
        /// The always internet.
        /// </value>
        public int AlwaysInternet { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public ClientSoftwareStates Client { get; set; }

        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        /// <value>
        /// The type of the client.
        /// </value>
        public ClientTypes ClientType { get; set; }

        /// <summary>
        /// Gets or sets the client version.
        /// </summary>
        /// <value>
        /// The client version.
        /// </value>
        public string ClientVersion { get; set; }

        /// <summary>
        /// Gets or sets the type of the cpu.
        /// </summary>
        /// <value>
        /// The type of the cpu.
        /// </value>
        public string CpuType { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        /// <value>
        /// The creation date.
        /// </value>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the decommissioned.
        /// </summary>
        /// <value>
        /// The decommissioned.
        /// </value>
        public int Decommissioned { get; set; }

        /// <summary>
        /// Gets or sets the name of the distinguished.
        /// </summary>
        /// <value>
        /// The name of the distinguished.
        /// </value>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// Gets or sets the hardware id.
        /// </summary>
        /// <value>
        /// The hardware id.
        /// </value>
        public string HardwareId { get; set; }

        /// <summary>
        /// Gets or sets the internet enabled.
        /// </summary>
        /// <value>
        /// The internet enabled.
        /// </value>
        public int InternetEnabled { get; set; }

        /// <summary>
        /// Gets or sets the ip addresses.
        /// </summary>
        /// <value>
        /// The ip addresses.
        /// </value>
        public string[] IpAddresses { get; set; }

        /// <summary>
        /// Gets or sets the ip subnets.
        /// </summary>
        /// <value>
        /// The ip subnets.
        /// </value>
        public string[] IpSubnets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is assigned to user.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is assigned to user; otherwise, <c>false</c>.
        /// </value>
        public bool IsAssignedToUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is machine changes persisted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is machine changes persisted; otherwise, <c>false</c>.
        /// </value>
        public bool IsMachineChangesPersisted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is virtual machine.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is virtual machine; otherwise, <c>false</c>.
        /// </value>
        public bool IsVirtualMachine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is write filter capable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is write filter capable; otherwise, <c>false</c>.
        /// </value>
        public bool IsWriteFilterCapable { get; set; }

        /// <summary>
        /// Gets or sets the last logon.
        /// </summary>
        /// <value>
        /// The last logon.
        /// </value>
        public DateTime LastLogon { get; set; }

        /// <summary>
        /// Gets or sets the last logon user domain.
        /// </summary>
        /// <value>
        /// The last logon user domain.
        /// </value>
        public string LastLogonUserDomain { get; set; }

        /// <summary>
        /// Gets or sets the last name of the logon user.
        /// </summary>
        /// <value>
        /// The last name of the logon user.
        /// </value>
        public string LastLogonUserName { get; set; }

        /// <summary>
        /// Gets or sets the mac addresses.
        /// </summary>
        /// <value>
        /// The mac addresses.
        /// </value>
        public string[] MacAddresses { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the netbios.
        /// </summary>
        /// <value>
        /// The name of the netbios.
        /// </value>
        public string NetbiosName { get; set; }

        /// <summary>
        /// Gets or sets the obsolte.
        /// </summary>
        /// <value>
        /// The obsolte.
        /// </value>
        public ObsoleteStates Obsolte { get; set; }

        /// <summary>
        /// Gets or sets the operating system named version.
        /// </summary>
        /// <value>
        /// The operating system named version.
        /// </value>
        public string OperatingSystemNamedVersion { get; set; }

        /// <summary>
        /// Gets or sets the previoussms UUID.
        /// </summary>
        /// <value>
        /// The previoussms UUID.
        /// </value>
        public string PrevioussmsUuid { get; set; }

        /// <summary>
        /// Gets or sets the resource domain or workgroup.
        /// </summary>
        /// <value>
        /// The resource domain or workgroup.
        /// </value>
        public string ResourceDomainOrWorkgroup { get; set; }

        /// <summary>
        /// Gets or sets the resource names.
        /// </summary>
        /// <value>
        /// The resource names.
        /// </value>
        public string[] ResourceNames { get; set; }

        /// <summary>
        /// Gets or sets the security group names.
        /// </summary>
        /// <value>
        /// The security group names.
        /// </value>
        public string[] SecurityGroupNames { get; set; }

        /// <summary>
        /// Gets or sets the sm bios GUID.
        /// </summary>
        /// <value>
        /// The sm bios GUID.
        /// </value>
        public string SmBiosGuid { get; set; }

        /// <summary>
        /// Gets or sets the assigned sites.
        /// </summary>
        /// <value>
        /// The assigned sites.
        /// </value>
        public string[] AssignedSites { get; set; }

        /// <summary>
        /// Gets or sets the installed sites.
        /// </summary>
        /// <value>
        /// The installed sites.
        /// </value>
        public string[] InstalledSites { get; set; }

        /// <summary>
        /// Gets or sets the resident sites.
        /// </summary>
        /// <value>
        /// The resident sites.
        /// </value>
        public string[] ResidentSites { get; set; }

        /// <summary>
        /// Gets or sets the SMS unique identifier.
        /// </summary>
        /// <value>
        /// The SMS unique identifier.
        /// </value>
        public string SmsUniqueIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the system group.
        /// </summary>
        /// <value>
        /// The name of the system group.
        /// </value>
        public string[] SystemGroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the system ou.
        /// </summary>
        /// <value>
        /// The name of the system ou.
        /// </value>
        public string[] SystemOuName { get; set; }

        /// <summary>
        /// Gets or sets the system roles.
        /// </summary>
        /// <value>
        /// The system roles.
        /// </value>
        public string[] SystemRoles { get; set; }
    }
}
