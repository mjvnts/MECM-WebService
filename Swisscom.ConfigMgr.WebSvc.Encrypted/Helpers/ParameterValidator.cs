using System;
using System.Text.RegularExpressions;

namespace Swisscom.ConfigMgr.WebSvc.Encrypted.Helpers
{
    /// <summary>
    /// Validates WebMethod input parameters to prevent invalid or malicious input.
    /// </summary>
    public static class ParameterValidator
    {
        private static readonly Regex ComputerNamePattern = new Regex(@"^[a-zA-Z0-9\-]{1,15}$", RegexOptions.Compiled);
        private static readonly Regex UserNamePattern = new Regex(@"^[a-zA-Z0-9._\-]{1,256}$", RegexOptions.Compiled);
        private static readonly Regex GuidPattern = new Regex(@"^[{(]?[0-9a-fA-F]{8}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{12}[)}]?$", RegexOptions.Compiled);

        public static void RequireNotEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Parameter '{parameterName}' cannot be null or empty.");
        }

        public static void ValidateComputerName(string computerName)
        {
            RequireNotEmpty(computerName, nameof(computerName));
            if (!ComputerNamePattern.IsMatch(computerName))
                throw new ArgumentException($"Computer name '{computerName}' is invalid. Must be 1-15 alphanumeric characters or hyphens.");
        }

        public static void ValidateUserName(string userName)
        {
            RequireNotEmpty(userName, nameof(userName));
            if (!UserNamePattern.IsMatch(userName))
                throw new ArgumentException($"User name '{userName}' contains invalid characters.");
        }

        public static void ValidateGuid(string guid)
        {
            RequireNotEmpty(guid, nameof(guid));
            if (!GuidPattern.IsMatch(guid))
                throw new ArgumentException($"GUID '{guid}' is not a valid GUID format.");
        }

        public static void ValidateCollectionName(string collectionName)
        {
            RequireNotEmpty(collectionName, nameof(collectionName));
            if (collectionName.Length > 256)
                throw new ArgumentException($"Collection name exceeds maximum length of 256 characters.");
        }

        public static void ValidateVariableName(string variableName)
        {
            RequireNotEmpty(variableName, nameof(variableName));
            if (variableName.Length > 256)
                throw new ArgumentException($"Variable name exceeds maximum length of 256 characters.");
        }
    }
}
