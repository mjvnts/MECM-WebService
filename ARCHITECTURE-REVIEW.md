# ConfigMgr WebService - Architecture Review & Refactoring Plan

## 1. Current Architecture Summary

### Solution Structure

```
MECM-WebService/
├── Swisscom.ConfigMgr.Library/          # Shared library (.NET 4.7.2)
│   ├── ActiveDirectory.cs               # AD operations (LDAP queries, computer/user/group management, BitLocker)
│   ├── ConfigMgrUtility.cs              # Core SCCM/ConfigMgr WMI operations (~1500+ lines)
│   ├── Interfaces/
│   │   ├── ILogging.cs                  # Logging interface (severity types, logging levels)
│   │   └── ISmsBaseClass.cs             # Base interface for SMS management classes
│   ├── ManagementClasses/               # 20 SMS WMI wrapper classes
│   │   ├── SmsAdvertisement.cs
│   │   ├── SmsApplication.cs / SmsApplicationBase.cs / SmsApplicationInfo.cs / SmsApplicationAssignment.cs
│   │   ├── SmsApprovalRequest.cs
│   │   ├── SmsAssignmentBase.cs
│   │   ├── SmsClient.cs
│   │   ├── SmsCollection.cs / SmsCollectionRule.cs / SmsCollectionRuleDirect.cs / SmsCollectionRuleQuery.cs
│   │   ├── SmsFolder.cs
│   │   ├── SmsPackage.cs
│   │   ├── SmsResource.cs
│   │   ├── SmsScheduleToken.cs
│   │   ├── SmsSite.cs
│   │   ├── SmsTaskSequence.cs / SmsTaskSequenceAction.cs / SmsTaskSequenceGroup.cs
│   │   ├── SmsTaskSequenceInstallApplication.cs / SmsTaskSequenceInstallSoftware.cs
│   │   ├── SmsTaskSequenceStep.cs
│   │   ├── SmsUser.cs
│   │   └── SmsVariables.cs
│   └── Util/
│       ├── AuthenticationHelper.cs      # Azure AD / MSAL auth helper
│       ├── GraphUtil.cs                 # Microsoft Graph API client (Intune + Entra ID)
│       ├── Logging.cs                   # Singleton file-based logger (CMTrace format)
│       ├── ScsCrypto.cs                 # AES encryption/decryption utility
│       ├── ScsSemaphore.cs              # Thread synchronization helper
│       ├── ScsServiceTextWriterTraceListener.cs  # Custom trace listener
│       ├── TaskSequence.cs              # Task sequence helper
│       └── Utility.cs                   # Static utilities (EventLog, SecureString, XML)
│
└── Swisscom.ConfigMgr.WebSvc.Encrypted/ # ASP.NET WebService (.asmx, .NET 4.7.2)
    ├── ConfigMgrWebSvc.asmx             # WebService entry point
    ├── ConfigMgrWebSvc.asmx.cs          # All WebMethods (~1435 lines, single God class)
    ├── Global.asax / Global.asax.cs     # Application lifecycle
    ├── App_Start/WebApiConfig.cs        # WebAPI route config (unused?)
    ├── Config/
    │   ├── ConfigHandler.cs             # Reads custom config section from web.config
    │   └── ConfigMgrWebSvcConfigSection.cs  # Custom ConfigurationSection definition
    ├── Helpers/
    │   └── GenericSoapResponse.cs       # Generic SOAP response wrapper + helper
    ├── Security/
    │   ├── SecurityHandler.cs           # Encryption/decryption + timestamp validation
    │   ├── TimestampValidationExtension.cs  # SOAP extension for timestamp validation
    │   └── TransportSecurityExtension.cs    # SOAP extension for transport security
    └── Web.config                       # Configuration with custom sccmWebSvc section
```

### How the WebService Uses the Library

The WebService (`ConfigMgrWebSvc.asmx.cs`) instantiates and depends on these Library classes:

| Library Class | Field in WebService | Usage |
|---|---|---|
| `ConfigMgrUtility` | `_configMgrUtility` | All SCCM/ConfigMgr operations (WMI-based) |
| `ActiveDirectory` | `_activeDirectory` | AD operations (computer lookup, DDR, BitLocker, group membership) |
| `Logging` | `_logging` | Singleton file-based logging (CMTrace format) |
| `GraphUtil` | Created per-method | Intune/Entra ID operations via Microsoft Graph API |
| `ScsCrypto` | Via `SecurityHandler` | Payload encryption/decryption |

The dependency flow is:
```
WebService (ConfigMgrWebSvc.asmx.cs)
  ├── ConfigHandler (reads web.config)
  ├── ConfigMgrUtility (connects to SCCM via WMI)
  │   ├── WqlConnectionManager (Microsoft.ConfigurationManagement.ManagementProvider)
  │   ├── SmsSite, SmsCollection, SmsClient, SmsUser, etc.
  │   └── SMS_StateMigration (USMT associations)
  ├── ActiveDirectory (LDAP queries via System.DirectoryServices)
  ├── Logging (singleton, file-based)
  └── GraphUtil (Microsoft Graph API - Intune & Entra ID)
      └── MSAL ConfidentialClientApplication (token acquisition)
```

---

## 2. Complete WebMethod Inventory

### Currently Exposed WebMethods (26 total)

| # | Current Name | Parameters | Return Type | Domain | Description |
|---|---|---|---|---|---|
| 1 | `AddNewComputerByBiosGuid` | computerName, biosGuid, timestamp | `GenericSoapResponse<object>` | SCCM | Import computer by BIOS GUID + send DDR from AD |
| 2 | `AddPrimaryUser` | computerName, userName, timestamp | `GenericSoapResponse<object>` | SCCM | Add user-device affinity |
| 3 | `GetPrimaryUsers` | computerName, timestamp | `GenericSoapResponse<List<string>>` | SCCM | List primary users |
| 4 | `GetUSMTMigrationStatus` | sourceComputerName, destinationComputerName, timestamp | `GenericSoapResponse<string>` | SCCM | Check USMT migration status |
| 5 | `RemovePrimaryUser` | computerName, userName, timestamp | `GenericSoapResponse<object>` | SCCM | Remove user-device affinity |
| 6 | `RegisterFirstLogin` | computerName, userName, timestamp | `GenericSoapResponse<object>` | Database | Execute stored procedure (HARDCODED connection string!) |
| 7 | `RemoveComputerFromCollection` | computerName, collectionName, timestamp | `GenericSoapResponse<object>` | SCCM | Remove from collection by name |
| 8 | `RemoveComputerFromOSDCollection` | computerName, advertisementId, timestamp | `GenericSoapResponse<object>` | SCCM | Remove from OSD collection by advertisement |
| 9 | `AddUSMTComputerAssociation` | sourceComputerName, destinationComputerName, timestamp | `GenericSoapResponse<object>` | SCCM | Create USMT migration association |
| 10 | `DeleteComputer` | computerName, timestamp | `GenericSoapResponse<object>` | SCCM | Delete computer from SCCM |
| 11 | `DeleteComputerFromIntune` | computerName, timestamp | `GenericSoapResponse<string>` | Intune | Delete device from Intune |
| 12 | `DeleteComputerByGuid` | guid, timestamp | `GenericSoapResponse<object>` | SCCM | Delete computer by BIOS GUID |
| 13 | `AddComputerToCollection` | computerName, collectionName, timestamp | `GenericSoapResponse<object>` | SCCM | Add to collection by name |
| 14 | `ClearPxeFlag` | computerName, timestamp | `GenericSoapResponse<object>` | SCCM | Clear PXE advertisement flag |
| 15 | `ChangePrimaryUsers` | computerName, userList[], timestamp | `GenericSoapResponse<object>` | SCCM | Bulk sync primary users |
| 16 | `AddComputerToOsdCollection` | computerName, identifier, timestamp | `GenericSoapResponse<object>` | SCCM | Add to OSD collection by config key |
| 17 | `TriggerRestagingOfComputer` | computerName, identifier, timestamp | `GenericSoapResponse<object>` | SCCM | Clear PXE + add to OSD (composite) |
| 18 | `AddComputerVariable` | computerName, variableName, variableValue, timestamp | `GenericSoapResponse<object>` | SCCM | Set a computer variable |
| 19 | `DeleteComputerVariable` | computerName, variableName, timestamp | `GenericSoapResponse<object>` | SCCM | Remove a computer variable |
| 20 | `RemoveUSMTComputerAssociation` | sourceComputerName, destinationComputerName, timestamp | `GenericSoapResponse<object>` | SCCM | Delete USMT association |
| 21 | `ReplaceAllComputerVariables` | computerName, variables (KVP list), timestamp | `GenericSoapResponse<object>` | SCCM | Bulk replace all variables |
| 22 | `SendSccmStatusMessage` | computerName, severity, component, description, timestamp | `GenericSoapResponse<object>` | SCCM | Send status message |
| 23 | `IsMemberOfCollection` | computerName, collectionName, timestamp | `GenericSoapResponse<string>` | SCCM | Check collection membership |
| 24 | `ClientExistsByName` | computerName, timestamp | `GenericSoapResponse<string>` | SCCM | Check if client exists |
| 25 | `RemoveFromOsdCollection` | computerName, advertisementId, timestamp | `GenericSoapResponse<object>` | SCCM | Remove from advertised collection |
| 26 | `ClientExistsInIntune` | computerName, timestamp | `GenericSoapResponse<string>` | Intune | Check if device exists in Intune |
| 27 | `SetPrimaryUserInIntune` | computerName, userName, timestamp | `GenericSoapResponse<object>` | Intune | Set primary user in Intune |
| 28 | `SetDeviceCategoryInIntune` | computerName, deviceCategoryName, timestamp | `GenericSoapResponse<object>` | Intune | Set device category in Intune |
| 29 | `CheckIfDeviceIsCoManagedIntune` | computerName, timestamp | `GenericSoapResponse<string>` | Intune | Check co-management status |
| 30 | `AddComputerToEntraGroup` | computerName, groupName, timestamp | `GenericSoapResponse<object>` | Entra ID | Add device to Entra group |
| 31 | `RemoveComputerFromEntraGroup` | computerName, groupName, timestamp | `GenericSoapResponse<object>` | Entra ID | Remove device from Entra group |
| 32 | `AddUserToEntraGroup` | samAccountName, groupName, timestamp | `GenericSoapResponse<object>` | Entra ID | Add user to Entra group |
| 33 | `RemoveUserFromEntraGroup` | samAccountName, groupName, timestamp | `GenericSoapResponse<object>` | Entra ID | Remove user from Entra group |
| 34 | `GetLog` | (none) | `XmlDocument` | Utility | Return log file as XML |

**Commented out (not exposed):**
- `RegisterFirstLogin` - `[WebMethod]` attribute is commented out
- `MoveComputerObject` - `[WebMethod]` attribute is commented out

---

## 3. Identified Design Problems

### 3.1 Critical Issues

#### P1: SQL Injection Vulnerability
**File:** `ConfigMgrWebSvc.asmx.cs:266`
```csharp
string sqlQuery = $"EXEC [DWH].[sp_Add_FirstLogon] @Device = N'{computerName}', @UserID = N'{userName}'";
```
User-supplied `computerName` and `userName` are directly interpolated into SQL. This is a **SQL injection vulnerability**.

#### P2: Hardcoded Connection String
**File:** `ConfigMgrWebSvc.asmx.cs:265`
```csharp
string connectionString = "Provider=SQLOLEDB.1;Server=SCBSSQLDAP02;database=DAPDWH;User ID=DAP;Password=DAP;";
```
Database credentials are hardcoded in the source code including server name, database name, and credentials.

#### P3: Thread.Sleep in Web Request
**File:** `ConfigMgrWebSvc.asmx.cs:95`
```csharp
System.Threading.Thread.Sleep(3000);
```
A 3-second `Thread.Sleep` in `AddNewComputerByBiosGuid` blocks the thread pool thread during a web request. Under load, this will exhaust the thread pool.

#### P4: Deprecated Cryptography
**File:** `ScsCrypto.cs:88,117`
```csharp
var rijndael = new RijndaelManaged();  // Deprecated
var shaHasher = new SHA512Managed();    // Deprecated
var shaHasher = new SHA256Managed();    // Deprecated
```
`RijndaelManaged`, `SHA512Managed`, and `SHA256Managed` are deprecated. Should use `Aes.Create()`, `SHA512.Create()`, `SHA256.Create()`.

### 3.2 Architectural Issues

#### A1: God Class - Single WebService Class
`ConfigMgrWebSvc.asmx.cs` is a single 1435-line class containing ALL 34 WebMethods. It mixes:
- SCCM operations
- Intune/Graph API operations
- Entra ID group management
- Active Directory operations
- Database operations
- Log file retrieval
- PowerShell execution

#### A2: No Dependency Injection
All dependencies are `new`-ed in the constructor:
```csharp
_configHandler = new ConfigHandler();
_logging = Logging.Instance;
_configMgrUtility = new ConfigMgrUtility(_configHandler.SiteServer);
_activeDirectory = new ActiveDirectory();
```
This makes unit testing impossible and creates tight coupling.

#### A3: Library Project is a Dumping Ground
The `Swisscom.ConfigMgr.Library` project contains unrelated concerns:
- SCCM/WMI management (`ConfigMgrUtility`, 20 SMS classes)
- Active Directory operations (`ActiveDirectory`)
- Microsoft Graph/Intune operations (`GraphUtil`)
- Cryptography (`ScsCrypto`)
- Logging (`Logging`)
- General utilities (`Utility`, `ScsSemaphore`)

These should be separated by domain.

#### A4: Duplicate WebMethods
- `RemoveComputerFromOSDCollection` (#8) and `RemoveFromOsdCollection` (#25) do essentially the same thing
- `TriggerRestagingOfComputer` (#17) is just `ClearPxeFlag` + `AddComputerToOsdCollection` combined

#### A5: GraphUtil Created Per-Request
Every Intune/Entra method creates a new `GraphUtil` instance, which acquires a new OAuth token each time:
```csharp
using (var graphUtil = new GraphUtil(
    this._configHandler.AppDisplayName,
    this._configHandler.AppId, ...))
```
This means every single Intune call does a full MSAL token acquisition. Should use token caching.

#### A6: Blocking Async Pattern in GraphUtil
**File:** `GraphUtil.cs:60`
```csharp
this._token = Task.Run<string>(async () => await this.GetAccessToken(...)).Result;
```
`.Result` on an async task is a deadlock risk in ASP.NET. This is the classic sync-over-async anti-pattern.

### 3.3 Code Quality Issues

#### Q1: Inconsistent Return Types
Some methods return `GenericSoapResponse<object>` (no data), some return `GenericSoapResponse<string>` with magic strings like `"EXISTS"`, `"NOT_FOUND"`, `"MEMBER"`, `"NOT_MEMBER"`, `"DELETED"`, `"CO_MANAGED"`. These should be proper boolean or enum-based responses.

#### Q2: Every Method Has Identical Error Handling
Every WebMethod follows this exact pattern:
```csharp
try {
    WriteLogMessage(Information, "Starting...");
    // do work
    WriteLogMessage(Information, "Success...");
    return SoapResponseHelper.Success<object>();
}
catch (Exception ex) {
    WriteLogMessage(Error, "Failed: " + ex.Message);
    WriteLogMessage(Error, "Full Stack Trace: " + ex.StackTrace);
    return SoapResponseHelper.Failure<object>(ex.Message);
}
```
This is massive code duplication across all 34 methods. Should be an aspect/decorator/middleware.

#### Q3: Mixed Languages in Log Messages
Some log messages are in German:
```csharp
// ConfigMgrWebSvc.asmx.cs:809
"Adding a computer variable on computer \"{computerName}\" mit Name \"{variable.Key}\" und Wert \"{variable.Value}\"."
// GraphUtil.cs - German comments throughout
// Typische Co-Management Erkennung (Enum-Prüfung!)
```

#### Q4: Unused Code
- `WebApiConfig.cs` exists but appears unused (the app is SOAP-only)
- `CallPowerShellScript` is a `public static` method in the WebService class, never exposed as a WebMethod
- `ConfigMgrUtility` contains many methods never called from the WebService (collection creation, folder management, task sequence management, etc.)
- Multiple commented-out code blocks throughout

#### Q5: No Input Validation
Parameters are passed directly to SCCM/AD without validation. No checks for:
- Null/empty strings
- Maximum length
- Allowed character patterns
- Computer name format validation

#### Q6: Timestamp Parameter is Unused
Every WebMethod accepts a `timestamp` parameter, but it's never used in the method body itself. The `TimestampValidationExtension` SOAP extension handles this at the transport level, making the parameter redundant in the method signature.

#### Q7: Exception Swallowing
**File:** `ActiveDirectory.cs:345-348`
```csharp
catch (Exception)
{
    // ANY exception - just return empty object
}
```
All exceptions in `GetActiveDirectoryComputerInformation` are silently swallowed.

---

## 4. Proposed Refactoring Plan

### Phase 1: Immediate Fixes (Low Risk, High Value)

1. **Fix SQL injection** in `RegisterFirstLogin` - use parameterized queries
2. **Move hardcoded connection string** to `web.config`
3. **Remove `Thread.Sleep`** - use polling or event-based waiting
4. **Replace deprecated crypto classes** with modern equivalents
5. **Remove duplicate WebMethods** (`RemoveComputerFromOSDCollection` vs `RemoveFromOsdCollection`)
6. **Standardize log messages** to English

### Phase 2: Decouple WebService from Library

#### 2a: Restructure the Library into Focused Projects

```
Swisscom.ConfigMgr.Core/              # Shared abstractions & models
  ├── Interfaces/
  │   ├── IConfigMgrService.cs         # SCCM operations interface
  │   ├── IActiveDirectoryService.cs   # AD operations interface
  │   ├── IGraphService.cs            # Intune/Entra operations interface
  │   └── ILoggingService.cs          # Logging interface
  ├── Models/
  │   ├── ComputerInfo.cs
  │   ├── PrimaryUserInfo.cs
  │   ├── CollectionMembershipResult.cs
  │   └── OperationResult.cs          # Replaces GenericSoapResponse for internal use
  └── Enums/
      └── SeverityType.cs

Swisscom.ConfigMgr.Sccm/              # SCCM/WMI operations only
  ├── ConfigMgrService.cs              # Implements IConfigMgrService
  └── ManagementClasses/               # SMS WMI wrappers (keep as-is)

Swisscom.ConfigMgr.ActiveDirectory/    # AD operations only
  └── ActiveDirectoryService.cs        # Implements IActiveDirectoryService

Swisscom.ConfigMgr.Graph/             # Microsoft Graph operations
  ├── GraphService.cs                  # Implements IGraphService
  └── TokenCache.cs                   # Proper MSAL token caching

Swisscom.ConfigMgr.Infrastructure/     # Cross-cutting concerns
  ├── Logging/
  │   └── CmTraceLogger.cs            # CMTrace-format file logger
  └── Security/
      └── AesCryptoService.cs         # Modern AES encryption
```

#### 2b: Extract Service Interfaces

```csharp
// IConfigMgrService.cs
public interface IConfigMgrService
{
    int AddComputerByBiosGuid(string computerName, string biosGuid);
    void DeleteComputer(string computerName);
    void DeleteComputerByGuid(string biosGuid);
    bool ClientExists(string computerName);
    bool ClientExistsByGuid(string biosGuid);

    void AddToCollection(string collectionId, string computerName);
    void RemoveFromCollection(string collectionId, string computerName);
    string GetCollectionIdByName(string collectionName);
    bool IsMemberOfCollection(int resourceId, string collectionId);
    void UpdateCollectionMembership(string collectionId);

    void SetPrimaryUser(string computerName, string userName);
    void RemovePrimaryUser(string computerName, string userName);
    List<string> GetPrimaryUsers(string computerName);
    void SyncPrimaryUsers(string computerName, string[] desiredUsers);

    void SetComputerVariable(string computerName, string name, string value);
    void RemoveComputerVariable(string computerName, string name);
    void ReplaceAllVariables(string computerName, IDictionary<string, string> variables);

    void ClearPxeFlag(string computerName);
    void SendStatusMessage(string computerName, int messageId, SeverityType severity, string component, string description);

    bool AddUsmtAssociation(string source, string destination);
    bool RemoveUsmtAssociation(string source, string destination);
    string GetUsmtMigrationStatus(string source, string destination);

    void SendDdrRecord(string computerName, string domain, string sid, string[] ouNames, string[] groupNames);
}

// IGraphService.cs (Intune + Entra)
public interface IGraphService
{
    // Intune
    string GetIntuneDeviceId(string computerName);
    bool DeleteIntuneDevice(string deviceId);
    bool DeviceExistsInIntune(string computerName);
    bool SetPrimaryUser(string deviceId, string userId);
    string GetPrimaryUser(string deviceId);
    bool IsCoManaged(string deviceId);
    bool SetDeviceCategory(string deviceId, string categoryName);

    // Entra ID
    string GetEntraDeviceId(string computerName);
    string GetGroupId(string groupName);
    string GetUserIdBySamAccountName(string samAccountName);
    string GetUserIdByUpn(string upn);
    bool AddDeviceToGroup(string deviceId, string groupId);
    bool RemoveDeviceFromGroup(string deviceId, string groupId);
    bool AddUserToGroup(string userId, string groupId);
    bool RemoveUserFromGroup(string userId, string groupId);
    bool IsDeviceMemberOfGroup(string deviceId, string groupId);
    bool IsUserMemberOfGroup(string userId, string groupId);
}
```

### Phase 3: Code Quality Improvements

#### 3a: Cross-Cutting Concerns via Decoration

```csharp
// Replace the repetitive try/catch/log pattern with a decorator or middleware
public class LoggingWebMethodDecorator
{
    public GenericSoapResponse<T> Execute<T>(
        string operationName,
        Func<T> action,
        ILogger logger)
    {
        try
        {
            logger.Information($"Starting: {operationName}");
            var result = action();
            logger.Information($"Completed: {operationName}");
            return SoapResponseHelper.Success(result);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed: {operationName}", ex);
            return SoapResponseHelper.Failure<T>(ex.Message);
        }
    }
}
```

#### 3b: Input Validation

```csharp
// Add validation attributes or a validator class
public static class ParameterValidator
{
    public static void ValidateComputerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Computer name cannot be empty");
        if (name.Length > 15)
            throw new ArgumentException("Computer name exceeds maximum length of 15 characters");
        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\-]+$"))
            throw new ArgumentException("Computer name contains invalid characters");
    }
}
```

#### 3c: Consistent Return Types

Replace magic strings with proper types:
```csharp
// Instead of: GenericSoapResponse<string> returning "EXISTS" / "NOT_FOUND"
// Use: GenericSoapResponse<bool> or GenericSoapResponse<ExistenceResult>

public class ExistenceCheckResult
{
    public bool Exists { get; set; }
    public string ResourceId { get; set; }  // optional additional info
}
```

---

## 5. Proposed WebMethod Renaming

### Naming Convention
Use consistent verb-noun pattern grouped by domain:

| # | Current Name | Suggested Name | Notes |
|---|---|---|---|
| **SCCM - Computers** |||
| 1 | `AddNewComputerByBiosGuid` | `ImportComputerByBiosGuid` | "Import" is ConfigMgr terminology |
| 10 | `DeleteComputer` | `RemoveComputer` | Consistent with "Remove" pattern |
| 12 | `DeleteComputerByGuid` | `RemoveComputerByBiosGuid` | Clarify it's BIOS GUID |
| 24 | `ClientExistsByName` | `CheckComputerExists` | "Client" → "Computer" for consistency |
| **SCCM - Collections** |||
| 13 | `AddComputerToCollection` | `AddToCollection` | Shorter, computer is implied |
| 7 | `RemoveComputerFromCollection` | `RemoveFromCollection` | |
| 23 | `IsMemberOfCollection` | `CheckCollectionMembership` | Verb-first |
| 16 | `AddComputerToOsdCollection` | `AddToOsdCollection` | |
| 8 | `RemoveComputerFromOSDCollection` | _Delete (duplicate of #25)_ | |
| 25 | `RemoveFromOsdCollection` | `RemoveFromOsdCollection` | Keep |
| **SCCM - PXE/OSD** |||
| 14 | `ClearPxeFlag` | `ClearPxeFlag` | Good as-is |
| 17 | `TriggerRestagingOfComputer` | `TriggerRestaging` | Shorter |
| **SCCM - Primary Users** |||
| 2 | `AddPrimaryUser` | `AddPrimaryUser` | Good as-is |
| 5 | `RemovePrimaryUser` | `RemovePrimaryUser` | Good as-is |
| 3 | `GetPrimaryUsers` | `GetPrimaryUsers` | Good as-is |
| 15 | `ChangePrimaryUsers` | `SyncPrimaryUsers` | "Sync" better describes the reconciliation logic |
| **SCCM - Variables** |||
| 18 | `AddComputerVariable` | `SetVariable` | "Set" allows create-or-update |
| 19 | `DeleteComputerVariable` | `RemoveVariable` | |
| 21 | `ReplaceAllComputerVariables` | `ReplaceAllVariables` | |
| **SCCM - USMT** |||
| 9 | `AddUSMTComputerAssociation` | `CreateUsmtAssociation` | |
| 20 | `RemoveUSMTComputerAssociation` | `RemoveUsmtAssociation` | |
| 4 | `GetUSMTMigrationStatus` | `GetUsmtMigrationStatus` | PascalCase fix |
| **SCCM - Status** |||
| 22 | `SendSccmStatusMessage` | `SendStatusMessage` | Drop "Sccm" prefix |
| **Intune** |||
| 26 | `ClientExistsInIntune` | `CheckIntuneDeviceExists` | |
| 11 | `DeleteComputerFromIntune` | `RemoveIntuneDevice` | |
| 27 | `SetPrimaryUserInIntune` | `SetIntunePrimaryUser` | |
| 28 | `SetDeviceCategoryInIntune` | `SetIntuneDeviceCategory` | |
| 29 | `CheckIfDeviceIsCoManagedIntune` | `CheckIntuneCoManagement` | Shorter |
| **Entra ID** |||
| 30 | `AddComputerToEntraGroup` | `AddDeviceToEntraGroup` | "Device" is Entra terminology |
| 31 | `RemoveComputerFromEntraGroup` | `RemoveDeviceFromEntraGroup` | |
| 32 | `AddUserToEntraGroup` | `AddUserToEntraGroup` | Good as-is |
| 33 | `RemoveUserFromEntraGroup` | `RemoveUserFromEntraGroup` | Good as-is |
| **Utility** |||
| 34 | `GetLog` | `GetServiceLog` | Clarify it's the service log |
| - | `RegisterFirstLogin` | _Remove or properly secure_ | SQL injection, hardcoded creds |

---

## 6. ASP.NET Core Migration Plan

### Target Architecture

```
Swisscom.ConfigMgr.WebApi/                    # ASP.NET Core 8+ Web API
├── Program.cs                                 # Minimal hosting + DI setup
├── appsettings.json                          # Configuration
├── Controllers/
│   ├── ComputersController.cs                # SCCM computer operations
│   ├── CollectionsController.cs              # SCCM collection operations
│   ├── PrimaryUsersController.cs             # User-device affinity
│   ├── VariablesController.cs                # Computer variables
│   ├── UsmtController.cs                     # USMT migration
│   ├── OsdController.cs                      # OSD/PXE operations
│   ├── IntuneController.cs                   # Intune device management
│   ├── EntraGroupsController.cs              # Entra ID group operations
│   └── DiagnosticsController.cs              # Log viewer, health check
├── Middleware/
│   ├── RequestLoggingMiddleware.cs            # Replace per-method logging
│   ├── ExceptionHandlingMiddleware.cs         # Global error handling
│   └── EncryptionMiddleware.cs               # Payload encryption/decryption
├── Models/
│   ├── Requests/                             # Typed request DTOs
│   └── Responses/                            # Typed response DTOs (replaces GenericSoapResponse)
├── Services/                                  # Business logic (if any WebService-specific)
└── wwwroot/                                   # Optional future UI
```

### REST API Design

```
# Computers
POST   /api/computers                          # Import (replaces AddNewComputerByBiosGuid)
GET    /api/computers/{name}/exists             # Check existence
DELETE /api/computers/{name}                    # Delete by name
DELETE /api/computers/by-guid/{guid}            # Delete by GUID

# Collections
POST   /api/collections/{name}/members/{computerName}     # Add to collection
DELETE /api/collections/{name}/members/{computerName}      # Remove from collection
GET    /api/collections/{name}/members/{computerName}      # Check membership

# OSD Collections
POST   /api/osd-collections/{identifier}/members/{computerName}  # Add to OSD
DELETE /api/osd-collections/{advertisementId}/members/{computerName}  # Remove
POST   /api/computers/{name}/restage                       # Trigger restaging

# Primary Users
GET    /api/computers/{name}/primary-users                 # List
POST   /api/computers/{name}/primary-users                 # Add
DELETE /api/computers/{name}/primary-users/{userName}       # Remove
PUT    /api/computers/{name}/primary-users                 # Sync (replace all)

# Variables
PUT    /api/computers/{name}/variables/{variableName}      # Set variable
DELETE /api/computers/{name}/variables/{variableName}       # Remove variable
PUT    /api/computers/{name}/variables                     # Replace all

# PXE
POST   /api/computers/{name}/clear-pxe                     # Clear PXE flag

# USMT
POST   /api/usmt/associations                              # Create
DELETE /api/usmt/associations                              # Remove
GET    /api/usmt/associations/status                       # Get status

# Status Messages
POST   /api/status-messages                                # Send

# Intune
GET    /api/intune/devices/{name}/exists                   # Check
DELETE /api/intune/devices/{name}                          # Delete
PUT    /api/intune/devices/{name}/primary-user              # Set primary user
PUT    /api/intune/devices/{name}/category                  # Set category
GET    /api/intune/devices/{name}/co-management             # Check co-management

# Entra Groups
POST   /api/entra/groups/{groupName}/devices/{computerName} # Add device
DELETE /api/entra/groups/{groupName}/devices/{computerName}  # Remove device
POST   /api/entra/groups/{groupName}/users/{samAccountName}  # Add user
DELETE /api/entra/groups/{groupName}/users/{samAccountName}  # Remove user

# Diagnostics
GET    /api/diagnostics/log                                # View log
GET    /api/diagnostics/health                             # Health check
```

### Migration Strategy

1. **Parallel deployment**: Run the new Web API alongside the existing ASMX service on the same IIS server
2. **SOAP compatibility layer**: Add a thin ASMX-compatible endpoint that proxies to the new controllers (for existing SCCM task sequence integrations)
3. **Gradual migration**: Move callers from SOAP to REST one at a time
4. **Feature parity first**: Don't add new features during migration, just port existing functionality

### IIS Hosting

ASP.NET Core on IIS requires:
- Install the ASP.NET Core Hosting Bundle
- Configure as an IIS application (in-process hosting recommended)
- The `web.config` is auto-generated by the SDK for IIS integration

```xml
<!-- web.config for ASP.NET Core on IIS -->
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\Swisscom.ConfigMgr.WebApi.dll"
                stdoutLogEnabled="false" hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

### Dependency Injection Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IConfigMgrService, ConfigMgrService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddSingleton<IGraphService, GraphService>();  // Singleton for token caching
builder.Services.AddSingleton<ILoggingService, CmTraceLogger>();

// Configuration
builder.Services.Configure<SccmOptions>(builder.Configuration.GetSection("Sccm"));
builder.Services.Configure<IntuneOptions>(builder.Configuration.GetSection("Intune"));
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("Security"));

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<EncryptionMiddleware>();

app.MapControllers();
app.Run();
```

---

## 7. Summary of Recommendations

### Do First (Quick Wins)
1. Fix SQL injection in `RegisterFirstLogin`
2. Move hardcoded connection string to config
3. Remove the duplicate `RemoveComputerFromOSDCollection` WebMethod
4. Standardize log messages to English
5. Replace deprecated crypto classes

### Do Next (Refactoring)
6. Extract service interfaces (`IConfigMgrService`, `IGraphService`, etc.)
7. Implement proper token caching in `GraphUtil`
8. Fix sync-over-async in `GraphUtil` constructor
9. Remove `Thread.Sleep` in `AddNewComputerByBiosGuid`
10. Add input validation on all parameters
11. Extract the repetitive try/catch/log pattern into a reusable decorator

### Do Later (Architecture)
12. Split the Library into focused projects
13. Create ASP.NET Core Web API project
14. Implement DI container
15. Add REST endpoints with proper routing
16. Add Swagger/OpenAPI documentation
17. Build optional management UI
18. Add health checks and structured logging
19. Set up parallel deployment for backward compatibility
