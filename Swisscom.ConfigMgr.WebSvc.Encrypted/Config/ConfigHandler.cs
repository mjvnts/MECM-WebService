using System.Collections.Generic;
using System.Configuration;

namespace Swisscom.ConfigMgr.WebSvc.Encrypted.Config
{
    public class ConfigHandler
    {
        private Dictionary<string, string> _osDeploymentCollections = new Dictionary<string, string>();

        public string SiteServer { get; private set; }
        public string SiteCode { get; private set; }
        public string DomainShortName { get; private set; }
        public string LogFile { get; private set; }
        public int LogLevel { get; private set; }
        public int MaxLogSize { get; private set; }
        public int MaxLogFiles { get; private set; }
        public string TemporaryUserDirectory { get; private set; }
        public string PrimaryUserVariable { get; private set; }
        public string AdGroupNamePrefix { get; private set; }
        public bool UseIntune { get; private set; }
        public string AppDisplayName { get; private set; }
        public string AppId { get; private set; }
        public string TenantId { get; private set; }
        public string GraphUrl { get; private set; }
        public string SecretString { get; private set; }
        public string EncryptionKey { get; private set; }
        public string EncryptionSalt { get; private set; }
        public bool RequireEncryption { get; private set; }
        public int TimestampToleranceSeconds { get; private set; }


        public void AddOsDeploymentCollection(string key, string value)
        {
            if (!this._osDeploymentCollections.ContainsKey(key))
            {
                this._osDeploymentCollections.Add(key, value);
            }
        }

        public string GetOsDeploymentCollection(string key)
        {
            if (this._osDeploymentCollections.ContainsKey(key))
            {
                return this._osDeploymentCollections[key];
            }

            return string.Empty;
        }

        public ConfigHandler()
        {
            var sccmConfig = (SccmWebSvcConfig)ConfigurationManager.GetSection("sccmWebSvc");

            if (sccmConfig.LogSettings != null)
            {
            this.LogFile = sccmConfig.LogSettings.LogFile;
            this.LogLevel = sccmConfig.LogSettings.LogLevel;
            this.MaxLogSize = sccmConfig.LogSettings.MaxLogFileSize;
            this.MaxLogFiles = sccmConfig.LogSettings.MaxLogFiles;
            this.TemporaryUserDirectory = sccmConfig.LogSettings.TemporaryUserFilePath;
            this.PrimaryUserVariable = sccmConfig.LogSettings.PrimaryUserVariable;
            }

            if (sccmConfig.SiteServerSettings != null)
            { 
            this.SiteServer = sccmConfig.SiteServerSettings.ServerName;
            this.SiteCode = sccmConfig.SiteServerSettings.SiteCode;
            this.DomainShortName = sccmConfig.SiteServerSettings.DomainShortName;
            this.UseIntune = sccmConfig.SiteServerSettings.UseIntune;
            this.AppDisplayName = sccmConfig.SiteServerSettings.AppDisplayName;
            this.AppId = sccmConfig.SiteServerSettings.AppId;
            this.TenantId = sccmConfig.SiteServerSettings.TenantId;
            this.GraphUrl = sccmConfig.SiteServerSettings.GraphUrl;
            this.SecretString = sccmConfig.SiteServerSettings.SecretString;
            }

            if (sccmConfig.SecuritySettings != null)
            {
                this.EncryptionKey = sccmConfig.SecuritySettings.EncryptionKey;
                this.EncryptionSalt = sccmConfig.SecuritySettings.EncryptionSalt;
                this.RequireEncryption = sccmConfig.SecuritySettings.RequireEncryption;
                this.TimestampToleranceSeconds = sccmConfig.SecuritySettings.TimestampToleranceSeconds;
            }
            else
            {
                this.EncryptionKey = "defaultKey"; // Fallback for tests
            }

            this.AdGroupNamePrefix = sccmConfig.SoftwareDeploymentSettings.SwGroupNamePrefix;

            foreach (OsDeploymentElement osDeployment in sccmConfig.OsDeployments)
            {
                this._osDeploymentCollections.Add(osDeployment.Identifier, osDeployment.CollectionName);
            }
        }
    }
}
