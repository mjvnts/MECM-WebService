// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActiveDirectory.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Security.Principal;

    /// <summary>
    /// Provides Active Directory functions.
    /// </summary>
    public class ActiveDirectory : IDisposable
    {
        /// <summary>
        /// The directory root.
        /// </summary>
        private readonly DirectoryEntry _root;

        /// <summary>
        /// Indicates if the object is disposed or not.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectory"/> class.
        /// </summary>
        public ActiveDirectory()
        {
            this._isDisposed = false;
            this._root = this.GetEntry(this.GetRoot());
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ActiveDirectory"/> class.
        /// </summary>
        ~ActiveDirectory()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// ADSI Provider.
        /// </summary>
        public enum Provider
        {
            /// <summary>
            /// LDAP provider.
            /// </summary>
            Ldap,

            /// <summary>
            /// WinNT provider.
            /// </summary>
            WinNt
        }

        /// <summary>
        /// The account type.
        /// </summary>
        public enum AccountType
        {
            /// <summary>
            /// Type is user.
            /// </summary>
            User,

            /// <summary>
            /// Type is group.
            /// </summary>
            Group,

            /// <summary>
            /// Type is a computer.
            /// </summary>
            Computer
        }

        /// <summary>
        /// The account modification return types.
        /// </summary>
        public enum AccountModificationReturnTypes
        {
            /// <summary>
            /// The modification was successful.
            /// </summary>
            Successful,

            /// <summary>
            /// The modification was unsuccessful.
            /// </summary>
            NotSuccessful,

            /// <summary>
            /// The account is not a member.
            /// </summary>
            NotMember,

            /// <summary>
            /// The account is already a member.
            /// </summary>
            AlreadyMember,

            /// <summary>
            /// Unable to find the group.
            /// </summary>
            GroupNotFound,

            /// <summary>
            /// Unable to find the account.
            /// </summary>
            AccountNotFound
        }

        /// <summary>
        /// Computer information class for Active Directory computer details.
        /// </summary>
        public class ComputerInformation
        {
            public string ComputerName { get; set; }
            public string ComputerSid { get; set; }
            public string[] SystemOUName { get; set; }
            public string[] GroupNames { get; set; }

            public ComputerInformation()
            {
                ComputerName = string.Empty;
                ComputerSid = string.Empty;
                SystemOUName = new string[0];
                GroupNames = new string[0];
            }
        }

        /// <summary>
        /// Gets Active Directory computer information including SID, OU path, and group memberships.
        /// Returns an empty ComputerInformation object if the computer is not found (no exception thrown).
        /// </summary>
        /// <param name="domainName">The domain name (can be null for auto-detect).</param>
        /// <param name="computerName">The computer name.</param>
        /// <returns>ComputerInformation object with SID, OU names, and group memberships. Empty object if computer not found.</returns>
        public ComputerInformation GetActiveDirectoryComputerInformation(string domainName, string computerName)
        {
            var returnValue = new ComputerInformation();
            DirectoryEntry searchRoot = null;
            DirectorySearcher searcher = null;

            try
            {
                // Try multiple methods to connect to AD
                Exception lastConnectionException = null;

                // Method 1: Try with provided domain name (if not empty)
                if (!string.IsNullOrEmpty(domainName))
                {
                    try
                    {
                        searchRoot = new DirectoryEntry("LDAP://" + domainName);
                        searchRoot.RefreshCache(); // Test the connection
                    }
                    catch (Exception ex)
                    {
                        lastConnectionException = ex;
                        searchRoot?.Dispose();
                        searchRoot = null;
                    }
                }

                // Method 2: Try without domain (uses current domain context)
                if (searchRoot == null)
                {
                    try
                    {
                        searchRoot = new DirectoryEntry("LDAP://RootDSE");
                        var defaultNamingContext = searchRoot.Properties["defaultNamingContext"].Value.ToString();
                        searchRoot.Dispose();
                        searchRoot = new DirectoryEntry("LDAP://" + defaultNamingContext);
                        searchRoot.RefreshCache();
                    }
                    catch (Exception ex)
                    {
                        lastConnectionException = ex;
                        searchRoot?.Dispose();
                        searchRoot = null;
                    }
                }

                // Method 3: Try with domain from DirectoryEntry without LDAP prefix
                if (searchRoot == null)
                {
                    try
                    {
                        using (var domain = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain())
                        {
                            searchRoot = new DirectoryEntry("LDAP://" + domain.Name);
                            searchRoot.RefreshCache();
                        }
                    }
                    catch (Exception ex)
                    {
                        lastConnectionException = ex;
                        searchRoot?.Dispose();
                        searchRoot = null;
                    }
                }

                // Method 4: Last resort - try plain LDAP without domain
                if (searchRoot == null)
                {
                    try
                    {
                        searchRoot = new DirectoryEntry();
                        searchRoot.RefreshCache();
                    }
                    catch (Exception ex)
                    {
                        lastConnectionException = ex;
                        searchRoot?.Dispose();
                        searchRoot = null;
                    }
                }

                // If all methods failed, return empty object (no exception thrown)
                if (searchRoot == null)
                {
                    // Connection to AD failed - return empty object
                    return returnValue;
                }

                // Now search for the computer
                searcher = new DirectorySearcher(searchRoot);
                searcher.SearchScope = SearchScope.Subtree;
                searcher.Filter = "(&(objectClass=computer)(name=" + computerName + "))";
                searcher.PropertiesToLoad.Add("objectSID");
                searcher.PropertiesToLoad.Add("memberOf");
                searcher.PropertiesToLoad.Add("distinguishedName");

                var result = searcher.FindOne();

                // If computer not found in AD, return empty object (no exception thrown)
                if (result == null)
                {
                    return returnValue;
                }

                // Computer found - extract information
                // Get Computer SID
                if (result.Properties["objectSID"].Count > 0)
                {
                    var sidBytes = (byte[])result.Properties["objectSID"][0];
                    var computerSid = new SecurityIdentifier(sidBytes, 0);
                    returnValue.ComputerSid = computerSid.Value;
                }

                // Get Distinguished Name and parse OU path
                if (result.Properties["distinguishedName"].Count > 0)
                {
                    var computerDN = result.Properties["distinguishedName"][0].ToString();
                    var systemOuList = new List<string>();
                    var computerDnSplitted = computerDN.Split(',');
                    var domainNameArray = new List<string>();
                    var computerDnArray = new List<string>();

                    foreach (var item in computerDnSplitted)
                    {
                        var value = item.Split('=');
                        if (value.Length == 2)
                        {
                            if (value[0].Trim().Equals("OU", StringComparison.OrdinalIgnoreCase))
                            {
                                computerDnArray.Add(value[1].Trim());
                            }
                            else if (value[0].Trim().Equals("DC", StringComparison.OrdinalIgnoreCase))
                            {
                                domainNameArray.Add(value[1].Trim());
                            }
                        }
                    }

                    var domainNameConstructed = string.Join(".", domainNameArray).ToUpper();
                    var previousOu = string.Empty;

                    for (int i = computerDnArray.Count - 1; i >= 0; i--)
                    {
                        if (string.IsNullOrEmpty(previousOu))
                        {
                            systemOuList.Add(domainNameConstructed + "/" + computerDnArray[i].ToUpper());
                            previousOu = computerDnArray[i].ToUpper();
                        }
                        else
                        {
                            systemOuList.Add(domainNameConstructed + "/" + previousOu + "/" + computerDnArray[i].ToUpper());
                            previousOu = previousOu + "/" + computerDnArray[i].ToUpper();
                        }
                    }

                    returnValue.SystemOUName = systemOuList.ToArray();
                }

                // Get Group Memberships
                if (result.Properties["memberOf"].Count > 0)
                {
                    var groupList = new List<string>();

                    // Extract domain from DN for group prefix
                    string groupDomainPrefix = domainName;
                    if (result.Properties["distinguishedName"].Count > 0)
                    {
                        var dn = result.Properties["distinguishedName"][0].ToString();
                        var dcParts = new List<string>();
                        foreach (var part in dn.Split(','))
                        {
                            if (part.Trim().StartsWith("DC=", StringComparison.OrdinalIgnoreCase))
                            {
                                dcParts.Add(part.Trim().Substring(3));
                            }
                        }
                        if (dcParts.Count > 0)
                        {
                            groupDomainPrefix = string.Join(".", dcParts);
                        }
                    }

                    foreach (string memberOf in result.Properties["memberOf"])
                    {
                        // Extract CN from memberOf DN
                        var parts = memberOf.Split(',');
                        if (parts.Length > 0)
                        {
                            var cnPart = parts[0].Split('=');
                            if (cnPart.Length == 2 && cnPart[0].Trim().Equals("CN", StringComparison.OrdinalIgnoreCase))
                            {
                                groupList.Add(groupDomainPrefix + "\\" + cnPart[1].Trim());
                            }
                        }
                    }
                    returnValue.GroupNames = groupList.ToArray();
                }

                returnValue.ComputerName = computerName;
            }
            catch (Exception)
            {
                // ANY exception - just return empty object (no exception thrown)
                // This includes connection errors, search errors, parsing errors, etc.
            }
            finally
            {
                // Clean up resources
                searcher?.Dispose();
                searchRoot?.Dispose();
            }

            return returnValue;
        }


        /// <summary>
        /// Deletes a computer object.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        public void DeleteComputerObject(string computerName)
        {
            using (var computer = this.GetComputer(computerName))
            {
                computer.DeleteTree();
            }
        }

        /// <summary>
        /// Creates the computer object.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="path">The path to the computer object.</param>
        public void CreateComputerObject(string computerName, string path)
        {
            using (var ou = new DirectoryEntry(path))
            {
                using (var computer = ou.Children.Add("CN=" + computerName, "computer"))
                {
                    computer.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Moves the computer object.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="path">The path to the computer object.</param>
        public void MoveComputerObject(string computerName, string path)
        {
            using (var computer = this.GetComputer(computerName))
            {
                using (var targetOu = this.GetEntry(path))
                {
                    computer.MoveTo(targetOu);
                }
            }
        }

        /// <summary>
        /// Gets the computer.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>The computer object.</returns>
        public DirectoryEntry GetComputer(string computerName)
        {
            return this.GetAccount(computerName, AccountType.Computer);
        }

        /// <summary>
        /// Gets the account.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="accountType">Type of the account.</param>
        /// <returns>The account or null if .</returns>
        public DirectoryEntry GetAccount(string accountName, AccountType accountType)
        {
            DirectoryEntry accountObject = null;
            var searchFilter = string.Empty;
            if (accountType == AccountType.User)
            {
                searchFilter = "(&(objectCategory=person)(objectClass=user)(sAMAccountName=" + accountName + "))";
            }
            else if (accountType == AccountType.Group)
            {
                searchFilter = "(&(objectClass=Group)(CN=" + accountName + "))";
            }
            else if (accountType == AccountType.Computer)
            {
                searchFilter = "(&(objectClass=computer)(|(cn=" + accountName + ")(dn=" + accountName + ")))";
            }

            using (var directorySearcher = new DirectorySearcher(this._root))
            {
                directorySearcher.Filter = searchFilter;
                directorySearcher.SearchScope = SearchScope.Subtree;
                var results = directorySearcher.FindAll();
                foreach (var result in results)
                {
                    accountObject = ((SearchResult)result).GetDirectoryEntry();
                    break;
                }

                results.Dispose();
                directorySearcher.Dispose();
            }

            return accountObject;
        }

        public List<string> GetGroupMembership(DirectoryEntry userAccount)
        {
            return this.GetGroupMembership(userAccount, string.Empty);
        }

        public List<string> GetGroupMembership(DirectoryEntry userAccount, string groupPrefix)
        {
            var groupList = new List<string>();
            userAccount.RefreshCache(new string[] { "tokenGroups" });

            foreach (byte[] sid in userAccount.Properties["tokenGroups"])
            {
                var groupSID = new SecurityIdentifier(sid, 0).ToString();
                using (var groupEntry = new DirectoryEntry("LDAP://<SID=" + groupSID + ">"))
                {
                    var groupName = groupEntry.Properties["samAccountName"][0].ToString();
                    if (string.IsNullOrEmpty(groupPrefix))
                    {
                        groupList.Add(groupName);
                    }
                    else
                    {
                        if (groupName.StartsWith(groupPrefix, StringComparison.CurrentCultureIgnoreCase))
                        {
                            groupList.Add(groupName);
                        }
                    }
                }
            }

            return groupList;
        }

        public List<string> GetGroupMembershipOfUser(string userName, string groupPrefix)
        {
            List<string> groupList = null;
            using (var account = this.GetAccount(userName, AccountType.User))
            {
                groupList = this.GetGroupMembership(account, groupPrefix);
            }

            return groupList;
        }

        /// <summary>
        /// Removes the member.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="someType">Some type.</param>
        /// <returns>A value indicating the state of the operation.</returns>
        public AccountModificationReturnTypes RemoveMember(string groupName, string accountName, string someType)
        {
            var accountType = (AccountType)Enum.Parse(typeof(AccountType), someType, true);
            using (var group = this.GetAccount(groupName, AccountType.Group))
            {
                if (group == null) return AccountModificationReturnTypes.GroupNotFound;
                using (var account = this.GetAccount(accountName, accountType))
                {
                    if (account == null) return AccountModificationReturnTypes.AccountNotFound;
                    if (this.IsMember(group, account))
                    {
                        group.Invoke("Remove", new object[] { account.Path });
                        return AccountModificationReturnTypes.Successful;
                    }

                    return AccountModificationReturnTypes.NotMember;
                }
            }
        }

        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="someType">Some type.</param>
        /// <returns>A value indicating the state of the operation.</returns>
        public AccountModificationReturnTypes AddMember(string groupName, string accountName, string someType)
        {
            var accountType = (AccountType)Enum.Parse(typeof(AccountType), someType, true);

            using (var group = this.GetAccount(groupName, AccountType.Group))
            {
                if (group == null) return AccountModificationReturnTypes.GroupNotFound;
                using (var account = this.GetAccount(accountName, accountType))
                {
                    if (account == null) return AccountModificationReturnTypes.AccountNotFound;
                    if (!this.IsMember(group, account))
                    {
                        group.Invoke("Add", new object[] { account.Path });
                        return AccountModificationReturnTypes.Successful;
                    }

                    return AccountModificationReturnTypes.AlreadyMember;
                }
            }
        }

        /// <summary>
        /// Gets the bit locker key.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="currentBitlockerId">The Recovery GUId.</param>
        /// <returns>The RecoveryPassword.</returns>
        public string GetBitlockerKey(string computerName, string currentBitlockerId)
        {
            var recoveryPassword = string.Empty;
            using (var computerObject = this.GetComputer(computerName))
            {
                var children = computerObject.Children;
                var schemaFilter = children.SchemaFilter;
                schemaFilter.Add("msFVE-RecoveryInformation");
                foreach (DirectoryEntry child in children) using (child)
                {
                    var bitlockerIds = currentBitlockerId.Split(',');
                    foreach (var bitlockerId in bitlockerIds)
                    {
                        var recoveryGuid = new Guid((byte[])child.Properties["msFVE-RecoveryGuid"].Value);
                        if (bitlockerId.ToUpper().Contains(recoveryGuid.ToString().ToUpper()))
                        {
                            recoveryPassword = child.Properties["msFVE-RecoveryPassword"].Value.ToString();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(recoveryPassword)) break;
                }
            }

            return recoveryPassword;
        }

        /// <summary>
        /// Gets a dictionary containing the bit locker keys and recovery GUIDs.
        /// </summary>
        /// <param name="computerName">The name of the computer.</param>
        /// <returns>The dictionary.</returns>
        public Dictionary<string, string> GetBitlockerKeys(string computerName)
        {
            var keys = new Dictionary<string, string>();
            using (var computerObject = this.GetComputer(computerName))
            {
                var children = computerObject.Children;
                var schemaFilter = children.SchemaFilter;
                schemaFilter.Add("msFVE-RecoveryInformation");
                foreach (DirectoryEntry child in children) using (child)
                {
                    var recoveryGuid = new Guid((byte[])child.Properties["msFVE-RecoveryGuid"].Value).ToString();
                    var recoveryPassword = child.Properties["msFVE-RecoveryPassword"].Value.ToString();
                    if (!keys.ContainsKey(recoveryGuid))
                    {
                        keys.Add(recoveryGuid, recoveryPassword);
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// Disposes the instantiated object.
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
                    if (this._root != null) this._root.Dispose();
                }

                // free unmanaged ressources
            }

            this._isDisposed = true;
        }

        /// <summary>
        /// Determines whether the specified group is member.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="account">The account.</param>
        /// <returns><c>true</c> if the specified group is member; otherwise, <c>false</c>. </returns>
        private bool IsMember(DirectoryEntry group, DirectoryEntry account)
        {
            return (bool)group.Invoke("IsMember", new object[] { account.Path });
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <returns>Returns the root.</returns>
        private string GetRoot()
        {
            using (var rootDse = this.GetEntry("rootDSE"))
            {
                return (string)rootDse.Properties["defaultNamingContext"].Value;
            }
        }

        /// <summary>
        /// Gets the entry.
        /// </summary>
        /// <param name="dn">The distinguished name.</param>
        /// <returns>A <see cref="DirectoryEntry"/> object.</returns>
        private DirectoryEntry GetEntry(string dn)
        {
            return new DirectoryEntry(this.EnumToString(Provider.Ldap) + "://" + dn, null, null, AuthenticationTypes.Secure | AuthenticationTypes.Sealing | AuthenticationTypes.Signing);
        }

        /// <summary>
        /// Get the text value of the enumeration.
        /// </summary>
        /// <param name="enumType">Type of the enumeration type.</param>
        /// <returns>The name of the enumeration type.</returns>
        private string EnumToString(Enum enumType)
        {
            return Enum.GetName(enumType.GetType(), enumType).ToUpper();
        }
    }
}
