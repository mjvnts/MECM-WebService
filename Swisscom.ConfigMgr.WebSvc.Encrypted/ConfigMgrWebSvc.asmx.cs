namespace Swisscom.ConfigMgr.WebSvc.Encrypted
{
    using System;
    using System.Collections.Generic;
    using System.Web.Services;
    using System.Runtime.CompilerServices;
    using System.Linq;
    using System.Management.Automation.Runspaces;
    using System.IO;
    using System.Xml;
    using System.Text.RegularExpressions;
    using System.Web.Services.Protocols;
    using Swisscom.ConfigMgr.Library;
    using Swisscom.ConfigMgr.Library.Interfaces;
    using Swisscom.ConfigMgr.Library.Util;
    using Swisscom.ConfigMgr.WebSvc.Encrypted.Config;
    using Swisscom.ConfigMgr.WebSvc.Encrypted.Helpers;

    /// <summary>
    /// Summary description for ConfigMgrWebSvc
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]

    public class ConfigMgrWebSvc : System.Web.Services.WebService
    {
        private static readonly string ThreadName = System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name;
        private readonly ConfigHandler _configHandler;
        private readonly ConfigMgrUtility _configMgrUtility;
        private readonly Logging _logging;
        private readonly ActiveDirectory _activeDirectory;

        public ConfigMgrWebSvc()
        {
            _configHandler = new ConfigHandler();

            _logging = Logging.Instance;
            _logging.LogFile = _configHandler.LogFile;
            _logging.LoggingLevel = (LoggingLevels)_configHandler.LogLevel;
            _logging.MaxSize = _configHandler.MaxLogSize;
            _logging.MaxLogFiles = _configHandler.MaxLogFiles;

            try
            {
                _configMgrUtility = new ConfigMgrUtility(_configHandler.SiteServer);
                _activeDirectory = new ActiveDirectory();
            }
            catch (Exception ex)
            {
                WriteLogMessage(SeverityTypes.Error, $"Failed to connect to the SCCM Server \"{_configHandler.SiteServer}\": {ex.Message}");
                WriteLogMessage(SeverityTypes.Error, "Full Stack Trace:" + ex.StackTrace);
            }
        }


        [WebMethod(Description = "Adds a new computer by its BIOS GUID to ConfigMgr and sends DDR record with AD information.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddNewComputerByBiosGuid(
    string computerName,
    string biosGuid,
    string timestamp)
        {
            try
            {
                ParameterValidator.ValidateComputerName(computerName);
                ParameterValidator.ValidateGuid(biosGuid);

                this.WriteLogMessage(SeverityTypes.Information, $"Add a new computer with name \"{computerName}\" and bios guid \"{biosGuid}\".");
                this.WriteLogMessage(SeverityTypes.Information, $"Verifying if a computer with bios guid \"{biosGuid}\" already exists.");

                if (this._configMgrUtility.ClientWithUUIDExists(biosGuid))
                {
                    this.WriteLogMessage(SeverityTypes.Warning, $"A computer with bios guid \"{biosGuid}\" already exists.");
                    return SoapResponseHelper.Failure<object>(
                        $"A computer with name {computerName} and bios guid {biosGuid} already exists"
                    );
                }
                else
                {
                    var resourceId = this._configMgrUtility.AddNewComputerBySmBiosGuid(computerName, biosGuid);

                    if (resourceId == 0)
                    {
                        this.WriteLogMessage(SeverityTypes.Error, $"Failed to add new computer with name \"{computerName}\" and bios guid \"{biosGuid}\" (ResourceID 0).");
                        return SoapResponseHelper.Failure<object>(
                            "Failed to add new computer (ResourceID 0)."
                        );
                    }
                    else
                    {
                        this.WriteLogMessage(SeverityTypes.Information, "Successfully updated collection \"SMS00001\".");
                        this.WriteLogMessage(SeverityTypes.Information, $"Add new computer with name \"{computerName}\" and bios guid \"{biosGuid}\" returned ResourceID \"{resourceId}\".");

                        // Wait for ConfigMgr to initialize the new resource
                        // NOTE: Thread.Sleep blocks the thread pool thread. Consider replacing with
                        // a polling mechanism that checks if the resource is ready.
                        System.Threading.Thread.Sleep(3000);

                        // Send DDR record with AD information if available
                        if (this._activeDirectory != null)
                        {
                            this.WriteLogMessage(SeverityTypes.Information, $"Querying Active Directory for computer \"{computerName}\".");

                            var computerInfo = this._activeDirectory.GetActiveDirectoryComputerInformation(null, computerName);

                            if (!string.IsNullOrEmpty(computerInfo.ComputerSid))
                            {
                                var ouCount = computerInfo.SystemOUName != null ? computerInfo.SystemOUName.Length : 0;
                                var groupCount = computerInfo.GroupNames != null ? computerInfo.GroupNames.Length : 0;

                                this.WriteLogMessage(SeverityTypes.Information, $"Computer \"{computerName}\" found in Active Directory. SID: {computerInfo.ComputerSid}, OU paths: {ouCount}, Groups: {groupCount}.");

                                // Extract domain from OU path
                                string domainForDDR = this._configHandler.DomainShortName;
                                if (computerInfo.SystemOUName != null && computerInfo.SystemOUName.Length > 0)
                                {
                                    var ouPath = computerInfo.SystemOUName[0];
                                    var slashIndex = ouPath.IndexOf('/');
                                    if (slashIndex > 0)
                                    {
                                        domainForDDR = ouPath.Substring(0, slashIndex);
                                    }
                                }

                                // Send DDR record
                                this.WriteLogMessage(SeverityTypes.Information, $"Sending DDR record for computer \"{computerName}\".");

                                try
                                {
                                    this._configMgrUtility.SendDDRRecord(
                                        computerName,
                                        domainForDDR,
                                        computerInfo.ComputerSid,
                                        computerInfo.SystemOUName,
                                        computerInfo.GroupNames
                                    );

                                    this.WriteLogMessage(SeverityTypes.Information, $"Successfully sent DDR record for computer \"{computerName}\".");
                                }
                                catch (Exception ddrEx)
                                {
                                    this.WriteLogMessage(SeverityTypes.Error, $"Failed to send DDR record for computer \"{computerName}\". Error: {ddrEx.Message}");
                                }
                            }
                            else
                            {
                                this.WriteLogMessage(SeverityTypes.Information, $"Computer \"{computerName}\" not found in Active Directory. Skipping DDR record.");
                            }
                        }
                        else
                        {
                            this.WriteLogMessage(SeverityTypes.Warning, "Active Directory utility is not initialized. Skipping DDR record.");
                        }

                        return SoapResponseHelper.Success<object>();
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Adding new computer with name \"{computerName}\" and bios guid \"{biosGuid}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Adds a primary user on an existing computer object in ConfigMgr.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddPrimaryUser(
            string computerName,
            string userName,
            string timestamp)
        {
            try
            {
                ParameterValidator.ValidateComputerName(computerName);
                ParameterValidator.ValidateUserName(userName);

                var user = string.Format("{0}\\\\{1}", this._configHandler.DomainShortName, userName);
                this.WriteLogMessage(SeverityTypes.Information, $"Trying to add the user \"{user}\" on \"{computerName}\"");
                this._configMgrUtility.SetPrimaryUser(computerName, user, ConfigMgrUtility.DeviceAffinityTypes.Administrator);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully added a new primary user with name \"{user}\" on computer \"{computerName}\".");                
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Adding a new primary user failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");                
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Gets a list of primary users of an existing computer object in ConfigMgr.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<List<string>> GetPrimaryUsers(
            string computerName,
            string timestamp)
        {
            try
            {
                var primaryUserList = this._configMgrUtility.GetAllPrimaryUsersOfClient(computerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully retrieved a list of primary users of computer \"{computerName}\", returned \"{primaryUserList.Count}\" users.");
                return SoapResponseHelper.Success(primaryUserList);
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Getting a list of primary users of computer \"{computerName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<List<string>>(ex.Message);
            }
        }


        [WebMethod(Description = "Retrieves the USMT migration status between two computers by their names.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<string> GetUSMTMigrationStatus(
            string sourceComputerName,
            string destinationComputerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Checking status of an association between \"{sourceComputerName}\" and \"{destinationComputerName}\".");
                string statusResult = this._configMgrUtility.GetUSMTMigrationStatus(sourceComputerName, destinationComputerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Status result: {statusResult}");
                return SoapResponseHelper.Success(statusResult);
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Error checking association between \"{sourceComputerName}\" and \"{destinationComputerName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");               
                return SoapResponseHelper.Failure<string>(ex.Message);
            }
        }


        [WebMethod(Description = "Removes a primary user of an existing computer object in ConfigMgr.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> RemovePrimaryUser(
            string computerName,
            string userName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Remove the primary user \"{userName}\" of computer \"{computerName}\".");
                var user = string.Format("{0}\\\\{1}", this._configHandler.DomainShortName, userName);
                this._configMgrUtility.DeletePrimaryUser(computerName, user);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully removed the primary user \"{user}\" of computer \"{computerName}\".");                
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Removing the primary user \"{userName}\" of computer \"{computerName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");                
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        //[WebMethod(Description = "Registers the first login of a user on a device in the database.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> RegisterFirstLogin(
            string computerName,
            string userName,
            string timestamp)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DapDwhConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                this.WriteLogMessage(SeverityTypes.Error, "DapDwhConnection connection string is not configured in web.config.");
                return SoapResponseHelper.Failure<object>("Database connection is not configured.");
            }

            try
            {
                using (var connection = new System.Data.OleDb.OleDbConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new System.Data.OleDb.OleDbCommand("[DWH].[sp_Add_FirstLogon]", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Device", computerName);
                        command.Parameters.AddWithValue("@UserID", userName);
                        command.ExecuteNonQuery();
                    }
                }
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully registered first login for {computerName} - {userName}.");
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Error registering first login for {computerName} - {userName}: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Removes a computer from a collection in ConfigMgr.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> RemoveComputerFromCollection(
            string computerName,
            string collectionName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Remove computer \"{computerName}\" from collection \"{collectionName}\".");
                var collectionId = this._configMgrUtility.GetCollectionIdByName(collectionName);
                this._configMgrUtility.RemoveMemberFromCollectionDirectByName(collectionId, computerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully removed computer \"{computerName}\" from collection \"{collectionName}\".");                
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Removing computer \"{computerName}\" from collection \"{collectionName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");                
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        // RemoveComputerFromOSDCollection removed - duplicate of RemoveFromOsdCollection.
        // Use RemoveFromOsdCollection instead.


        [WebMethod(Description = "Creates an USMT association between two computers by their names.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddUSMTComputerAssociation(
            string sourceComputerName,
            string destinationComputerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Attempting to create an association between \"{sourceComputerName}\" and \"{destinationComputerName}\".");
                bool success = this._configMgrUtility.AddUSMTComputerAssociation(sourceComputerName, destinationComputerName);

                if (success)
                {
                    this.WriteLogMessage(SeverityTypes.Information, "Association creation successful.");                    
                    return SoapResponseHelper.Success<object>();
                }
                else
                {
                    string errorMessage = "Unknown error during association creation.";
                    this.WriteLogMessage(SeverityTypes.Warning, errorMessage);                    
                    return SoapResponseHelper.Failure<object>(errorMessage);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error creating association between \"{sourceComputerName}\" and \"{destinationComputerName}\": {ex.Message}";
                this.WriteLogMessage(SeverityTypes.Error, errorMessage);                
                return SoapResponseHelper.Failure<object>(errorMessage);
            }
        }


        [WebMethod(Description = "Deletes a computer object in ConfigMgr.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> DeleteComputer(
            string computerName,
            string timestamp)
        {
            try
            {
                ParameterValidator.ValidateComputerName(computerName);

                this.WriteLogMessage(SeverityTypes.Information, $"Delete computer \"{computerName}\".");
                this._configMgrUtility.DeleteComputerByName(computerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully deleted computer \"{computerName}\".");               
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Deleting computer \"{computerName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");                
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }

        [WebMethod(Description = "Deletes a computer from Intune if it exists.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<string> DeleteComputerFromIntune(
            string computerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Attempting to delete device \"{computerName}\" from Intune...");

                using (var graphUtil = new GraphUtil(
                    this._configHandler.AppDisplayName,
                    this._configHandler.AppId,
                    this._configHandler.TenantId,
                    this._configHandler.GraphUrl,
                    this._configHandler.SecretString))
                {
                    // 1. Retrieve device ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving Intune device id for \"{computerName}\"...");
                    var deviceId = graphUtil.GetIntuneDeviceIdByName(computerName);

                    if (string.IsNullOrEmpty(deviceId))
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"Device \"{computerName}\" not found in Intune. Nothing to delete.");
                        return SoapResponseHelper.Success("NOT_FOUND");
                    }

                    this.WriteLogMessage(SeverityTypes.Information, $"Found Intune device id \"{deviceId}\" for \"{computerName}\". Deleting...");

                    // 2. Delete
                    var deleted = graphUtil.DeleteIntuneDevice(deviceId);

                    if (deleted)
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"Successfully deleted device \"{computerName}\" (ID: {deviceId}) from Intune.");
                        return SoapResponseHelper.Success("DELETED");
                    }
                    else
                    {
                        this.WriteLogMessage(SeverityTypes.Warning, $"Device \"{computerName}\" (ID: {deviceId}) was not found during deletion (possibly already removed).");
                        return SoapResponseHelper.Success("NOT_FOUND");
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to delete device \"{computerName}\" from Intune: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<string>(ex.Message);
            }
        }


        [WebMethod(Description = "Deletes a computer object by its guid in ConfigMgr.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> DeleteComputerByGuid(
            string guid,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Delete computer with GUID \"{guid}\".");
                this._configMgrUtility.DeleteComputerBySmBiosGuid(guid);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully deleted computer with GUID \"{guid}\".");                
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Deleting computer with GUID \"{guid}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");                
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Adds a computer to a collection in ConfigMgr.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddComputerToCollection(
            string computerName,
            string collectionName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Add computer \"{computerName}\" to collection \"{collectionName}\".");
                var collectionId = this._configMgrUtility.GetCollectionIdByName(collectionName);
                this._configMgrUtility.AddMemberToCollectionDirectByName(collectionId, computerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully added computer \"{computerName}\" to collection \"{collectionName}\".");
                this._configMgrUtility.UpdateCollectionMembership(collectionId);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully updated collection \"{collectionName}\".");
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Adding computer \"{computerName}\" to collection \"{collectionName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");                
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Clears the PXE Flag of a computer.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> ClearPxeFlag(
            string computerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Clear the PXE flag of computer \"{computerName}\".");
                this._configMgrUtility.ClearLastPxeAdvertisement(computerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully cleared the PXE flag of computer \"{computerName}\".");                
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Clearing the PXE flag of computer \"{computerName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");                
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Cleans up the primary users according the list submitted.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> ChangePrimaryUsers(
            string computerName,
            string[] userList,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Change primary users of \"{computerName}\" to \"{string.Join(";", userList)}\"");
                var client = this._configMgrUtility.GetClient(computerName);
                if (client == null)
                    throw new Exception("The client " + computerName + " does not exist in SCCM");
                if (client.ResourceId.ToString().StartsWith("2"))
                    throw new Exception("The client " + computerName + " is currently not a valid client in SCCM");

                var oldUserList = this._configMgrUtility.GetAllPrimaryUsersOfClient(computerName);
                this.WriteLogMessage(SeverityTypes.Information, "Retrieved currently assigned users \"" + string.Join(";", oldUserList) + "\" of computer \"" + computerName + "\".");
                var newUserList = new List<string>(userList);
                var usersToRemove = oldUserList.Except(newUserList);
                var usersToAdd = newUserList.Except(oldUserList);

                // Remove users
                foreach (var user in usersToRemove)
                {
                    if (string.IsNullOrEmpty(user)) continue;
                    var userName = string.Format("{0}\\\\{1}", this._configHandler.DomainShortName, user);
                    this.WriteLogMessage(SeverityTypes.Information, $"Trying to remove the user \"{userName}\" from \"{computerName}\"");
                    try
                    {
                        this._configMgrUtility.DeletePrimaryUser(computerName, userName);
                        this.WriteLogMessage(SeverityTypes.Information, $"\"{userName}\" removed successfully from \"{computerName}\"");
                    }
                    catch (Exception innerEx)
                    {
                        this.WriteLogMessage(SeverityTypes.Warning, $"Failed to remove the user \"{userName}\" from \"{computerName}\": {innerEx.Message}");
                    }
                }

                // Add users
                foreach (var user in usersToAdd)
                {
                    if (string.IsNullOrEmpty(user)) continue;
                    var userName = string.Format("{0}\\\\{1}", this._configHandler.DomainShortName, user);
                    this.WriteLogMessage(SeverityTypes.Information, $"Trying to add the user \"{userName}\" on \"{computerName}\"");
                    try
                    {
                        this._configMgrUtility.SetPrimaryUser(computerName, userName, ConfigMgrUtility.DeviceAffinityTypes.Administrator);
                        this.WriteLogMessage(SeverityTypes.Information, $"\"{userName}\" added successfully on \"{computerName}\"");
                    }
                    catch (Exception innerEx)
                    {
                        this.WriteLogMessage(SeverityTypes.Error, $"Failed to add the user \"{userName}\" on \"{computerName}\": {innerEx.Message}");
                        this.WriteLogMessage(SeverityTypes.Warning, $"Adding the \"{userName}\" and \"{computerName}\" to a temporary text file to add it later");
                        if (!Directory.Exists(this._configHandler.TemporaryUserDirectory))
                        {
                            Directory.CreateDirectory(this._configHandler.TemporaryUserDirectory);
                        }
                        var file = new FileInfo(Path.Combine(this._configHandler.TemporaryUserDirectory, computerName + ".txt"));
                        using (var streamWriter = file.AppendText())
                        {
                            streamWriter.WriteLine(userName);
                            streamWriter.Flush();
                        }
                        this.WriteLogMessage(SeverityTypes.Warning, $"The user \"{userName}\" was successfully added to \"{file.FullName}\"");
                    }
                }

                // Set variable if needed
                var newUserListString = string.Join(";", newUserList);
                if (!string.IsNullOrEmpty(newUserListString))
                {
                    try
                    {
                        this._configMgrUtility.AddComputerVariableNewComputer(computerName, this._configHandler.PrimaryUserVariable, newUserListString);
                        this.WriteLogMessage(SeverityTypes.Information, $"The user \"{newUserListString}\" was successfully added on the computer object of \"{computerName}\" as variable");
                    }
                    catch (Exception ex)
                    {
                        this.WriteLogMessage(SeverityTypes.Error, $"Failed to add the user \"{newUserListString}\" on the computer object of \"{computerName}\" as variable: {ex.Message}");
                        this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    this.WriteLogMessage(SeverityTypes.Warning, $"Skipping the variable for \"{computerName}\" with name \"{this._configHandler.PrimaryUserVariable}\" because value is empty.");
                }
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Changing the primary users of \"{computerName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Adds a computer to a OSD collection that is referenced by the identifier.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddComputerToOsdCollection(
            string computerName,
            string identifier,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Add computer \"{computerName}\" to OSD collection of \"{identifier}\".");
                var osdCollection = this._configHandler.GetOsDeploymentCollection(identifier);
                if (string.IsNullOrEmpty(osdCollection))
                    throw new Exception($"No OSD collection with key \"{identifier}\" exists.");

                this.WriteLogMessage(SeverityTypes.Information, $"OSD collection of \"{identifier}\" is \"{osdCollection}\".");
                this.WriteLogMessage(SeverityTypes.Information, $"Get collection ID of \"{osdCollection}\".");
                var collectionId = this._configMgrUtility.GetCollectionIdByName(osdCollection);
                this.WriteLogMessage(SeverityTypes.Information, $"The ID of collection \"{osdCollection}\" is \"{collectionId}\".");
                this.WriteLogMessage(SeverityTypes.Information, $"Trying to add \"{computerName}\" to collection \"{collectionId}\".");
                this._configMgrUtility.AddMemberToCollectionDirectByName(collectionId, computerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Added \"{computerName}\" successfully to collection \"{collectionId}\".");
                this._configMgrUtility.UpdateCollectionMembership(collectionId);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully updated collection membership of \"{collectionId}\".");
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Adding computer \"{computerName}\" to OSD collection of \"{identifier}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Triggers the restaging of a computer.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> TriggerRestagingOfComputer(
            string computerName,
            string identifier,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Triggering the restaging of computer \"{computerName}\".");

                try
                {
                    this.WriteLogMessage(SeverityTypes.Information, $"Clear the PXE flag of computer \"{computerName}\".");
                    this._configMgrUtility.ClearLastPxeAdvertisement(computerName);
                    this.WriteLogMessage(SeverityTypes.Information, $"Successfully cleared the PXE flag of computer \"{computerName}\".");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to clear the PXE Flag of {computerName}: {ex.Message}");
                }

                try
                {
                    this.WriteLogMessage(SeverityTypes.Information, $"Add computer \"{computerName}\" to OSD collection of \"{identifier}\".");
                    var osdCollection = this._configHandler.GetOsDeploymentCollection(identifier);
                    if (string.IsNullOrEmpty(osdCollection))
                        throw new Exception($"No OSD collection with key \"{identifier}\" exists.");

                    this.WriteLogMessage(SeverityTypes.Information, $"OSD collection of \"{identifier}\" is \"{osdCollection}\".");
                    this.WriteLogMessage(SeverityTypes.Information, $"Get collection ID of \"{osdCollection}\".");
                    var collectionId = this._configMgrUtility.GetCollectionIdByName(osdCollection);
                    this.WriteLogMessage(SeverityTypes.Information, $"The ID of collection \"{osdCollection}\" is \"{collectionId}\".");
                    this.WriteLogMessage(SeverityTypes.Information, $"Trying to add \"{computerName}\" to collection \"{collectionId}\".");
                    this._configMgrUtility.AddMemberToCollectionDirectByName(collectionId, computerName);
                    this.WriteLogMessage(SeverityTypes.Information, $"Added \"{computerName}\" successfully to collection \"{collectionId}\".");
                    this._configMgrUtility.UpdateCollectionMembership(collectionId);
                    this.WriteLogMessage(SeverityTypes.Information, $"Successfully updated collection membership of \"{collectionId}\".");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to add the computer {computerName} to OSD collection of {identifier}: {ex.Message}");
                }                
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Triggering the restaging of computer \"{computerName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Adds a variable to a computer object.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddComputerVariable(
            string computerName,
            string variableName,
            string variableValue,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Adding a computer variable on computer \"{computerName}\" with name \"{variableName}\" and value \"{variableValue}\".");
                if (!string.IsNullOrEmpty(variableValue))
                {
                    this._configMgrUtility.AddComputerVariableNewComputer(computerName, variableName, variableValue);
                    this.WriteLogMessage(SeverityTypes.Information, $"Successfully added computer variable on computer \"{computerName}\" with name \"{variableName}\" and value \"{variableValue}\".");
                    return SoapResponseHelper.Success<object>();
                }
                else
                {
                    this.WriteLogMessage(SeverityTypes.Warning, $"Skipping the variable for \"{computerName}\" with name \"{variableName}\" and value because it is empty.");
                    return SoapResponseHelper.Failure<object>("Skipped: Variable value is empty");
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Adding variable on computer \"{computerName}\" with name \"{variableName}\" and value \"{variableValue}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Deletes a variable from a computer object.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> DeleteComputerVariable(
            string computerName,
            string variableName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Deleting a computer variable on computer \"{computerName}\" with name \"{variableName}\".");
                this._configMgrUtility.RemoveComputerVariable(computerName, variableName);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully deleted computer variable on computer \"{computerName}\" with name \"{variableName}\".");
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Deleting variable on computer \"{computerName}\" with name \"{variableName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Deletes an USMT association between two computers by their names.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> RemoveUSMTComputerAssociation(
            string sourceComputerName,
            string destinationComputerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Attempting to delete an association between \"{sourceComputerName}\" and \"{destinationComputerName}\".");
                bool success = this._configMgrUtility.RemoveUSMTComputerAssociation(sourceComputerName, destinationComputerName);

                if (success)
                {
                    this.WriteLogMessage(SeverityTypes.Information, "Association deletion successful.");
                    return SoapResponseHelper.Success<object>();
                }
                else
                {
                    string errorMessage = "Unknown error during association deletion.";
                    this.WriteLogMessage(SeverityTypes.Warning, errorMessage);
                    return SoapResponseHelper.Failure<object>(errorMessage);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error deleting association between \"{sourceComputerName}\" and \"{destinationComputerName}\": {ex.Message}";
                this.WriteLogMessage(SeverityTypes.Error, errorMessage);
                return SoapResponseHelper.Failure<object>(errorMessage);
            }
        }


        [WebMethod(Description = "Replaces all variables of a computer object.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> ReplaceAllComputerVariables(
            string computerName,
            List<KeyValuePair<string, string>> variables,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Replacing all computer variables on computer \"{computerName}\".");
                this.WriteLogMessage(SeverityTypes.Information, $"Deleting all variables on computer \"{computerName}\".");
                this._configMgrUtility.RemoveAllComputerVariables(computerName);

                foreach (var variable in variables)
                {
                    this.WriteLogMessage(SeverityTypes.Information, $"Adding a computer variable on computer \"{computerName}\" with name \"{variable.Key}\" and value \"{variable.Value}\".");
                    if (string.IsNullOrEmpty(variable.Value))
                    {
                        this._configMgrUtility.AddComputerVariableNewComputer(computerName, variable.Key, " ");
                    }
                    else
                    {
                        this._configMgrUtility.AddComputerVariableNewComputer(computerName, variable.Key, variable.Value);
                    }
                    this.WriteLogMessage(SeverityTypes.Information, $"Successfully added computer variable on computer \"{computerName}\" with name \"{variable.Key}\" and value \"{variable.Value}\".");
                }

                this.WriteLogMessage(SeverityTypes.Information, $"Successfully replaced all computer variables on computer \"{computerName}\".");
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Replacing all variables on computer \"{computerName}\" failed: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Sends a status message to SCCM.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> SendSccmStatusMessage(
            string computerName,
            string severity,
            string component,
            string description,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Sending a status message for computer \"{computerName}\" of component \"{component}\" with severity \"{severity}\".");
                SeverityTypes severityType;
                if (!Enum.TryParse(severity, true, out severityType)) severityType = SeverityTypes.Information;
                this._configMgrUtility.SendSccmStatusMessage(computerName, 512, severityType, component, description);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully sent a status message for computer \"{computerName}\" of component \"{component}\" with severity \"{severity}\".");             
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to send a status message for computer \"{computerName}\" of component \"{component}\" with severity \"{severity}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Checks if a client is member of a collection.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<string> IsMemberOfCollection(
            string computerName,
            string collectionName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Checking if computer \"{computerName}\" is member of collection \"{collectionName}\".");
                var client = this._configMgrUtility.GetClient(computerName);
                var collectionId = this._configMgrUtility.GetCollectionIdByName(collectionName);
                bool isMember = this._configMgrUtility.IsMemberOfCollection(client.ResourceId, collectionId);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully checked if computer \"{computerName}\" is member of collection \"{collectionName}\": {isMember}");

                string result = isMember ? "MEMBER" : "NOT_MEMBER";
                return SoapResponseHelper.Success(result);
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to check if computer \"{computerName}\" is member of collection \"{collectionName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<string>(ex.Message);
            }
        }


        //[WebMethod(Description = "Move computer inside Active Directory.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> MoveComputerObject(
            string computerName,
            string targetOuPath,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Attempting to move computer \"{computerName}\" to \"{targetOuPath}\".");
                _activeDirectory.MoveComputerObject(computerName, targetOuPath);

                this.WriteLogMessage(SeverityTypes.Information, $"Successfully moved computer \"{computerName}\" to \"{targetOuPath}\".");
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to move computer \"{computerName}\" to \"{targetOuPath}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }


        [WebMethod(Description = "Checks if a computer exists in SCCM.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<string> ClientExistsByName(
            string computerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Checking if a client with name \"{computerName}\" already exists in SCCM.");
                bool exists = this._configMgrUtility.ClientExistsByName(computerName);
                this.WriteLogMessage(SeverityTypes.Information, $"Successfully verified if a computer with name \"{computerName}\" exists in SCCM: {exists}");

                string result = exists ? "EXISTS" : "NOT_FOUND";
                return SoapResponseHelper.Success(result);
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to check if computer with name \"{computerName}\" exists in SCCM: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<string>(ex.Message);
            }
        }


        [WebMethod(Description = "Removes a client from an advertised collection.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> RemoveFromOsdCollection(
            string computerName,
            string advertisementId,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Removing the client \"{computerName}\" from OSD collection with AdvertisementID \"{advertisementId}\".");

                var smsAdvertisement = this._configMgrUtility.GetAdvertisement(advertisementId);
                var collectionId = this._configMgrUtility.GetCollectionIdByName(smsAdvertisement.CollectionId);
                this._configMgrUtility.RemoveMemberFromCollectionDirectByName(collectionId, computerName);

                this.WriteLogMessage(SeverityTypes.Information, $"Successfully removed the computer \"{computerName}\" from collection \"{collectionId}\".");
                return SoapResponseHelper.Success<object>();
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to remove computer \"{computerName}\" from collection with advertisement id \"{advertisementId}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }

        [WebMethod(Description = "Checks if a computer exists in Intune.")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<string> ClientExistsInIntune(
            string computerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Checking if a device with name \"{computerName}\" exists in Intune...");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving device id of \"{computerName}\" in Intune...");
                    var deviceId = graphUtil.GetIntuneDeviceIdByName(computerName);

                    bool exists = !string.IsNullOrEmpty(deviceId);
                    this.WriteLogMessage(SeverityTypes.Information, $"Device exists in Intune: {exists}");

                    string result = exists ? "EXISTS" : "NOT_FOUND";
                    return SoapResponseHelper.Success(result);
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to check if device with name \"{computerName}\" exists in Intune: {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<string>(ex.Message);
            }
        }

        [WebMethod(Description = "Sets the primary user in Intune")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> SetPrimaryUserInIntune(
            string computerName,
            string userName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Changing the primary user of \"{computerName}\" in Intune.");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving user id of \"{userName}\" in Intune.");
                    var newUserId = graphUtil.GetUserIdBySamAccountName(userName);
                    if (string.IsNullOrEmpty(newUserId))
                        throw new Exception($"Unable to find a user with name \"{userName}\" in Intune.");

                    this.WriteLogMessage(SeverityTypes.Information, $"The user id of \"{userName}\" is \"{newUserId}\".");
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving device id of \"{computerName}\" in Intune.");
                    var deviceId = graphUtil.GetIntuneDeviceIdByName(computerName);
                    if (string.IsNullOrEmpty(deviceId))
                        throw new Exception($"Unable to find a device with name \"{computerName}\" in Intune.");

                    this.WriteLogMessage(SeverityTypes.Information, $"The device id of \"{computerName}\" is \"{deviceId}\".");
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving the primary user of \"{computerName}\" with id \"{deviceId}\" in Intune.");
                    var primaryUser = graphUtil.GetPrimaryUser(deviceId);
                    this.WriteLogMessage(SeverityTypes.Information, $"The primary user of \"{computerName}\" is \"{primaryUser}\".");
                    var existingUserId = string.Empty;
                    if (!string.IsNullOrEmpty(primaryUser))
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"Retrieving user id of \"{primaryUser}\" in Intune.");
                        existingUserId = graphUtil.GetUserIdByUpn(primaryUser);
                        this.WriteLogMessage(SeverityTypes.Information, $"The user id of \"{primaryUser}\" is \"{existingUserId}\".");
                    }

                    if (string.Equals(newUserId, existingUserId))
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"The existing user id of \"{primaryUser}\" ({existingUserId}) matches the one of the new user \"{userName}\"({newUserId}).");
                    }
                    else
                    {
                        this.WriteLogMessage(SeverityTypes.Warning, $"The existing user id of \"{primaryUser}\" ({existingUserId}) does not match the one of the new user \"{userName}\"({newUserId}). Changing it in Intune.");
                        if (!graphUtil.SetPrimaryUser(deviceId, newUserId))
                            throw new Exception("Unspecified Error");
                    }

                    return SoapResponseHelper.Success<object>();
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to change primary user of \"{computerName}\" to \"{userName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");               
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }

        [WebMethod(Description = "Sets the device category in Intune")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> SetDeviceCategoryInIntune(
            string computerName,
            string deviceCategoryName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Assigning device category \"{deviceCategoryName}\" for \"{computerName}\" in Intune.");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving device id of \"{computerName}\" in Intune.");
                    var deviceId = graphUtil.GetIntuneDeviceIdByName(computerName);
                    if (string.IsNullOrEmpty(deviceId))
                        throw new Exception($"Unable to find a device with name \"{computerName}\" in Intune.");

                    this.WriteLogMessage(SeverityTypes.Information, $"The device id of \"{computerName}\" is \"{deviceId}\".");
                    this.WriteLogMessage(SeverityTypes.Information, $"Trying to assign device category \"{deviceCategoryName}\" for device id \"{deviceId}\" in Intune.");

                    if (!graphUtil.SetDeviceCategory(deviceId, deviceCategoryName))
                        throw new Exception("Failed to assign device category (Unspecified Error).");

                    this.WriteLogMessage(SeverityTypes.Information, $"Device category assignment for \"{computerName}\" successful.");
                    return SoapResponseHelper.Success<object>();
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to assign device category for \"{computerName}\" to \"{deviceCategoryName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }

        [WebMethod(Description = "Checks if the device in Intune is Co-Managed (SCCM + Intune).")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<string> CheckIfDeviceIsCoManagedIntune(
            string computerName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Checking if device \"{computerName}\" is Co-managed in Intune...");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving device id of \"{computerName}\" in Intune...");
                    var deviceId = graphUtil.GetIntuneDeviceIdByName(computerName);
                    if (string.IsNullOrEmpty(deviceId))
                        throw new Exception($"Unable to find a device with name \"{computerName}\" in Intune.");

                    this.WriteLogMessage(SeverityTypes.Information, $"Checking Co-Management status for device id \"{deviceId}\"...");
                    bool isCoManaged = graphUtil.IsDeviceCoManaged(deviceId);

                    string result = isCoManaged ? "CO_MANAGED" : "NOT_CO_MANAGED";
                    this.WriteLogMessage(SeverityTypes.Information, $"Result: {result} for device \"{computerName}\" (id: {deviceId})");

                    return SoapResponseHelper.Success(result);
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to check Co-Management for \"{computerName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<string>(ex.Message);
            }
        }

        [WebMethod(Description = "Adds a computer to an Entra group")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddComputerToEntraGroup(
            string computerName,
            string groupName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Adding computer \"{computerName}\" to Entra group \"{groupName}\".");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    // Step 1: Retrieve device ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving device id of \"{computerName}\" in Entra.");
                    var deviceId = graphUtil.GetEntraDeviceIdByName(computerName);
                    if (string.IsNullOrEmpty(deviceId))
                        throw new Exception($"Unable to find a device with name \"{computerName}\" in Entra.");

                    this.WriteLogMessage(SeverityTypes.Information, $"The device id of \"{computerName}\" is \"{deviceId}\".");

                    // Step 2: Retrieve group ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving group id of \"{groupName}\" in Entra.");
                    var groupId = graphUtil.GetGroupIdByName(groupName);
                    if (string.IsNullOrEmpty(groupId))
                        throw new Exception($"Unable to find a group with name \"{groupName}\" in Entra.");

                    this.WriteLogMessage(SeverityTypes.Information, $"The group id of \"{groupName}\" is \"{groupId}\".");

                    // Step 3: Check membership
                    this.WriteLogMessage(SeverityTypes.Information, $"Checking if \"{computerName}\" is already a member of \"{groupName}\".");
                    var isMember = graphUtil.IsDeviceMemberOfGroup(deviceId, groupId);
                    if (isMember)
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"The computer \"{computerName}\" is already a member of group \"{groupName}\".");
                        return SoapResponseHelper.Success<object>();
                    }
                    else
                    {
                        // Step 4: Add device to group
                        this.WriteLogMessage(SeverityTypes.Information, $"Adding computer \"{computerName}\" to group \"{groupName}\".");
                        if (!graphUtil.AddDeviceToGroup(deviceId, groupId))
                            throw new Exception("Failed to add computer to group.");
                        this.WriteLogMessage(SeverityTypes.Information, $"Successfully added \"{computerName}\" to \"{groupName}\".");
                        return SoapResponseHelper.Success<object>();
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to add computer \"{computerName}\" to group \"{groupName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }

        [WebMethod(Description = "Removes a computer from an Entra group")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> RemoveComputerFromEntraGroup(
            string computerName,
            string groupName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Removing computer \"{computerName}\" from Entra group \"{groupName}\".");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    // Step 1: Retrieve device ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving Entra device id of \"{computerName}\".");
                    var deviceId = graphUtil.GetEntraDeviceIdByName(computerName);
                    if (string.IsNullOrEmpty(deviceId))
                        throw new Exception($"Unable to find a device with name \"{computerName}\" in Entra ID.");

                    this.WriteLogMessage(SeverityTypes.Information, $"The Entra device id of \"{computerName}\" is \"{deviceId}\".");

                    // Step 2: Retrieve group ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving group id of \"{groupName}\" in Entra.");
                    var groupId = graphUtil.GetGroupIdByName(groupName);
                    if (string.IsNullOrEmpty(groupId))
                        throw new Exception($"Unable to find a group with name \"{groupName}\" in Entra.");

                    this.WriteLogMessage(SeverityTypes.Information, $"The group id of \"{groupName}\" is \"{groupId}\".");

                    // Step 3: Check membership
                    this.WriteLogMessage(SeverityTypes.Information, $"Checking if \"{computerName}\" is a member of \"{groupName}\".");
                    var isMember = graphUtil.IsDeviceMemberOfGroup(deviceId, groupId);
                    if (!isMember)
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"The computer \"{computerName}\" is not a member of group \"{groupName}\". Nothing to remove.");
                        return SoapResponseHelper.Success<object>();
                    }
                    else
                    {
                        // Step 4: Remove device from group
                        this.WriteLogMessage(SeverityTypes.Information, $"Removing computer \"{computerName}\" from group \"{groupName}\".");
                        if (!graphUtil.RemoveDeviceFromGroup(deviceId, groupId))
                            throw new Exception($"Failed to remove computer \"{computerName}\" from group \"{groupName}\".");
                        this.WriteLogMessage(SeverityTypes.Information, $"Successfully removed \"{computerName}\" from \"{groupName}\".");
                        return SoapResponseHelper.Success<object>();
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to remove computer \"{computerName}\" from group \"{groupName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }

        [WebMethod(Description = "Adds a user to an Entra group")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> AddUserToEntraGroup(
            string samAccountName,
            string groupName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Adding user \"{samAccountName}\" to Entra group \"{groupName}\".");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    // Step 1: Retrieve user ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving user id of \"{samAccountName}\" in Entra.");
                    var userId = graphUtil.GetUserIdBySamAccountName(samAccountName);
                    if (string.IsNullOrEmpty(userId))
                        throw new Exception($"Unable to find a user with UPN \"{samAccountName}\" in Entra.");
                    this.WriteLogMessage(SeverityTypes.Information, $"The user id of \"{samAccountName}\" is \"{userId}\".");

                    // Step 2: Retrieve group ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving group id of \"{groupName}\" in Entra.");
                    var groupId = graphUtil.GetGroupIdByName(groupName);
                    if (string.IsNullOrEmpty(groupId))
                        throw new Exception($"Unable to find a group with name \"{groupName}\" in Entra.");
                    this.WriteLogMessage(SeverityTypes.Information, $"The group id of \"{groupName}\" is \"{groupId}\".");

                    // Step 3: Check membership
                    this.WriteLogMessage(SeverityTypes.Information, $"Checking if \"{samAccountName}\" is already a member of \"{groupName}\".");
                    var isMember = graphUtil.IsUserMemberOfGroup(userId, groupId);
                    if (isMember)
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"The user \"{samAccountName}\" is already a member of group \"{groupName}\".");
                        return SoapResponseHelper.Success<object>();
                    }
                    else
                    {
                        // Step 4: Add user to group
                        this.WriteLogMessage(SeverityTypes.Information, $"Adding user \"{samAccountName}\" to group \"{groupName}\".");
                        if (!graphUtil.AddUserToGroup(userId, groupId))
                            throw new Exception("Failed to add user to group.");
                        this.WriteLogMessage(SeverityTypes.Information, $"Successfully added \"{samAccountName}\" to \"{groupName}\".");
                        return SoapResponseHelper.Success<object>();
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to add user \"{samAccountName}\" to group \"{groupName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }

        [WebMethod(Description = "Removes a user from an Entra group")]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Wrapped)]
        public GenericSoapResponse<object> RemoveUserFromEntraGroup(
            string samAccountName,
            string groupName,
            string timestamp)
        {
            try
            {
                this.WriteLogMessage(SeverityTypes.Information, $"Removing user \"{samAccountName}\" from Entra group \"{groupName}\".");
                using (var graphUtil = new GraphUtil(this._configHandler.AppDisplayName, this._configHandler.AppId, this._configHandler.TenantId, this._configHandler.GraphUrl, this._configHandler.SecretString))
                {
                    // Step 1: Retrieve user ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving Entra user id of \"{samAccountName}\".");
                    var userId = graphUtil.GetUserIdBySamAccountName(samAccountName);
                    if (string.IsNullOrEmpty(userId))
                        throw new Exception($"Unable to find a user with UPN \"{samAccountName}\" in Entra ID.");
                    this.WriteLogMessage(SeverityTypes.Information, $"The Entra user id of \"{samAccountName}\" is \"{userId}\".");

                    // Step 2: Retrieve group ID
                    this.WriteLogMessage(SeverityTypes.Information, $"Retrieving group id of \"{groupName}\" in Entra.");
                    var groupId = graphUtil.GetGroupIdByName(groupName);
                    if (string.IsNullOrEmpty(groupId))
                        throw new Exception($"Unable to find a group with name \"{groupName}\" in Entra.");
                    this.WriteLogMessage(SeverityTypes.Information, $"The group id of \"{groupName}\" is \"{groupId}\".");

                    // Step 3: Check membership
                    this.WriteLogMessage(SeverityTypes.Information, $"Checking if \"{samAccountName}\" is a member of \"{groupName}\".");
                    var isMember = graphUtil.IsUserMemberOfGroup(userId, groupId);
                    if (!isMember)
                    {
                        this.WriteLogMessage(SeverityTypes.Information, $"The user \"{samAccountName}\" is not a member of group \"{groupName}\". Nothing to remove.");
                        return SoapResponseHelper.Success<object>();
                    }
                    else
                    {
                        // Step 4: Remove user from group
                        this.WriteLogMessage(SeverityTypes.Information, $"Removing user \"{samAccountName}\" from group \"{groupName}\".");
                        if (!graphUtil.RemoveUserFromGroup(userId, groupId))
                            throw new Exception($"Failed to remove user \"{samAccountName}\" from group \"{groupName}\".");
                        this.WriteLogMessage(SeverityTypes.Information, $"Successfully removed \"{samAccountName}\" from \"{groupName}\".");
                        return SoapResponseHelper.Success<object>();
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteLogMessage(SeverityTypes.Error, $"Failed to remove user \"{samAccountName}\" from group \"{groupName}\": {ex.Message}");
                this.WriteLogMessage(SeverityTypes.Error, $"Full Stack Trace: {ex.StackTrace}");
                return SoapResponseHelper.Failure<object>(ex.Message);
            }
        }



        [WebMethod(CacheDuration = 0, Description = "Gets the log file in XML")]
        public XmlDocument GetLog()
        {
            var logContent = File.ReadAllLines(this._configHandler.LogFile);
            var xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement("logFile");
            var declaration = xmlDocument.CreateNode(XmlNodeType.XmlDeclaration, null, null);
            xmlDocument.InsertBefore(declaration, xmlDocument.DocumentElement);
            foreach (var line in logContent)
            {
                var logNode = xmlDocument.CreateElement("logEntry");
                var logAttribute = xmlDocument.CreateAttribute("text");
                var logTime = xmlDocument.CreateAttribute("time");
                var logDate = xmlDocument.CreateAttribute("date");
                var logMethod = xmlDocument.CreateAttribute("method");
                var logSource = xmlDocument.CreateAttribute("source");

                var regExMessage = Regex.Match(line, @"\<\!\[LOG\[(?<Message>.*)?\]LOG\]\!\>");
                var test = Regex.Matches(line, @"\<\!\[LOG\[(?<Message>.*)?\]LOG\]\!\>\<time=\""(?<Time>.+)(?<TZAdjust>[+|-])(?<TZOffse>\d{2,3})\""\s+date=\""(?<Date>.+)?\""\s+component=\""(?<Component>.+)?\""\s+context=""(?<Context>.*)?\""\s+type=\""(?<Type>\d)?\""\s+thread=\""(?<TID>\d+)?\""\s+file=\""(?<Reference>.+)?\""\>");
                var text = string.Empty;
                if (regExMessage.Groups.Count > 0)
                {
                    text = regExMessage.Groups[1].Value;
                    var regExTime = Regex.Match(line, @"\<time=\""(?<Time>.+)");
                }


                //var startPoint = 7;
                var endPoint = line.IndexOf("]LOG]!>");
                // var text = line.Substring(startPoint, endPoint - 7);
                var rest = line.Substring(endPoint + 7, line.Length - endPoint - 7);


                logNode.Attributes.Append(logAttribute);
                logNode.Attributes.Append(logTime);
                logNode.Attributes.Append(logDate);
                logNode.Attributes.Append(logMethod);
                logNode.Attributes.Append(logSource);
                root.AppendChild(logNode);
            }


            return xmlDocument;
        }

        public static Dictionary<string, string> CallPowerShellScript(string scriptName, Dictionary<string, string> arguments)
        {
            var returnCode = string.Empty;
            var returnMessage = string.Empty;

            var returnValue = new Dictionary<string, string>();
            using (var runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();
                using (var pipeline = runspace.CreatePipeline())
                {
                    var command = new Command(scriptName);
                    foreach (var argument in arguments)
                    {
                        var parameter = new CommandParameter(argument.Key, argument.Value);
                        command.Parameters.Add(parameter);
                    }

                    pipeline.Commands.Add(command);
                    var result = pipeline.Invoke();

                    if (result.Count == 0)
                    {
                        returnValue.Add(false.ToString(), "Unspecified Error: \"" + scriptName + "\" did not return any values.");
                    }
                    else
                    {
                        foreach (dynamic pSObject in result)
                        {

                            returnCode = string.IsNullOrEmpty(pSObject.ReturnValue) ? string.Empty : pSObject.ReturnValue;
                            returnMessage = string.IsNullOrEmpty(pSObject.Message) ? string.Empty : pSObject.Message;

                            returnValue.Add("ReturnCode", returnCode);
                            returnValue.Add("Message", returnMessage);
                            break;

                        }
                    }
                }
            }

            return returnValue;
        }

        private void WriteLogMessage(SeverityTypes severityType, string message, [CallerFilePath] string callingFilePath = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int callerLinerNumber = 0)
        {
            var clientName = Context.Request.UserHostName;
            var userName = string.IsNullOrEmpty(Context.Request.ServerVariables["LOGON_USER"]) ? string.Empty : Context.Request.ServerVariables["LOGON_USER"];

            var sourceName = string.IsNullOrEmpty(userName) ? clientName : userName;

            var preFixMessage = string.IsNullOrEmpty(userName) ? string.Format("New request from computer \"{0}\"", clientName) : string.Format("New request from user \"{0}\" on computer \"{1}\"", userName, clientName);
            var completeMessage = string.Format("{0}: {1}", preFixMessage, message);
            this._logging.WriteMessage(severityType, methodName, System.Threading.Thread.CurrentThread.ManagedThreadId, sourceName, completeMessage);
        }
    }
}
