using System.Configuration;

namespace Swisscom.ConfigMgr.WebSvc.Encrypted.Config
{
    public class SiteServerSettingsElement : ConfigurationElement
    {
        [ConfigurationProperty("serverName", IsKey = true, IsRequired = true)]
        public string ServerName
        {
            get { return (string)base["serverName"]; }
            set { base["serverName"] = value; }
        }

        [ConfigurationProperty("siteCode", IsRequired = true)]
        public string SiteCode
        {
            get { return (string)base["siteCode"]; }
            set { base["siteCode"] = value; }
        }

        [ConfigurationProperty("domainShortName", IsRequired = true)]
        public string DomainShortName
        {
            get { return (string)base["domainShortName"]; }
            set { base["domainShortName"] = value; }
        }

        [ConfigurationProperty("useIntune", IsRequired = true)]
        public bool UseIntune
        {
            get { return (bool)base["useIntune"]; }
            set { base["useIntune"] = value; }
        }

        [ConfigurationProperty("appDisplayName", IsRequired = true)]
        public string AppDisplayName
        {
            get { return (string)base["appDisplayName"]; }
            set { base["appDisplayName"] = value; }
        }

        [ConfigurationProperty("appId", IsRequired = true)]
        public string AppId
        {
            get { return (string)base["appId"]; }
            set { base["appId"] = value; }
        }

        [ConfigurationProperty("tenantId", IsRequired = true)]
        public string TenantId
        {
            get { return (string)base["tenantId"]; }
            set { base["tenantId"] = value; }
        }

        [ConfigurationProperty("graphUrl", IsRequired = true)]
        public string GraphUrl
        {
            get { return (string)base["graphUrl"]; }
            set { base["graphUrl"] = value; }
        }

        [ConfigurationProperty("secretString", IsRequired = true)]
        public string SecretString
        {
            get { return (string)base["secretString"]; }
            set { base["secretString"] = value; }
        }
    }

    public class SecuritySettingsElement : ConfigurationElement
    {
        [ConfigurationProperty("encryptionKey", IsRequired = true)]
        public string EncryptionKey
        {
            get { return (string)base["encryptionKey"]; }
            set { base["encryptionKey"] = value; }
        }

        [ConfigurationProperty("encryptionSalt", IsRequired = true)]
        public string EncryptionSalt
        {
            get { return (string)base["encryptionSalt"]; }
            set { base["encryptionSalt"] = value; }
        }

        [ConfigurationProperty("requireEncryption", IsRequired = false, DefaultValue = false)]
        public bool RequireEncryption
        {
            get { return (bool)base["requireEncryption"]; }
            set { base["requireEncryption"] = value; }
        }

        [ConfigurationProperty("timestampToleranceSeconds", IsRequired = false, DefaultValue = 30)]
        public int TimestampToleranceSeconds
        {
            get { return (int)base["timestampToleranceSeconds"]; }
            set { base["timestampToleranceSeconds"] = value; }
        }
    }

    public class SoftwareDeploymentSettingsElement : ConfigurationElement
    {
        [ConfigurationProperty("swAdGroupNamePrefix", IsRequired = true)]
        public string SwGroupNamePrefix
        {
            get { return (string)base["swAdGroupNamePrefix"]; }
            set { base["swAdGroupNamePrefix"] = value; }
        }
    }

    public class LogSettingsElement : ConfigurationElement
    {
        [ConfigurationProperty("logFile", IsRequired = true)]
        public string LogFile
        {
            get { return (string)base["logFile"]; }
            set { base["logFile"] = value; }
        }

        [ConfigurationProperty("logLevel", IsRequired = true)]
        public int LogLevel
        {
            get { return (int)base["logLevel"]; }
            set { base["logLevel"] = value; }
        }

        [ConfigurationProperty("maxLogFileSize", IsRequired = true)]
        public int MaxLogFileSize
        {
            get { return (int)base["maxLogFileSize"]; }
            set { base["maxLogFileSize"] = value; }
        }

        [ConfigurationProperty("maxLogFiles", IsRequired = true)]
        public int MaxLogFiles
        {
            get { return (int)base["maxLogFiles"]; }
            set { base["maxLogFiles"] = value; }
        }

        [ConfigurationProperty("temporaryUserFilePath", IsRequired = true)]
        public string TemporaryUserFilePath
        {
            get { return (string)base["temporaryUserFilePath"]; }
            set { base["temporaryUserFilePath"] = value; }
        }

        [ConfigurationProperty("primaryUserVariable", IsRequired = true)]
        public string PrimaryUserVariable
        {
            get { return (string)base["primaryUserVariable"]; }
            set { base["primaryUserVariable"] = value; }
        }
    }

    public class OsDeploymentElement : ConfigurationElement
    {
        [ConfigurationProperty("identifier", IsKey = true, IsRequired = true)]
        public string Identifier
        {
            get { return (string)base["identifier"]; }
            set { base["identifier"] = value; }
        }

        [ConfigurationProperty("collectionName", IsRequired = true)]
        public string CollectionName
        {
            get { return (string)base["collectionName"]; }
            set { base["collectionName"] = value; }
        }
    }

    [ConfigurationCollection(typeof(OsDeploymentElement))]
    public class OsDeploymentCollection : ConfigurationElementCollection
    {
        public OsDeploymentElement this[int index]
        {
            get { return (OsDeploymentElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public new OsDeploymentElement this[string key]
        {
            get { return (OsDeploymentElement)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null) BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new OsDeploymentElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OsDeploymentElement)element).Identifier;
        }
    }

    public class SccmWebSvcConfig : ConfigurationSection
    {
        [ConfigurationProperty("osDeployments")]
        [ConfigurationCollection(typeof(OsDeploymentCollection))]
        public OsDeploymentCollection OsDeployments
        {
            get { return (OsDeploymentCollection)this["osDeployments"]; }
        }

        [ConfigurationProperty("siteServerSettings")]
        public SiteServerSettingsElement SiteServerSettings
        {
            get { return (SiteServerSettingsElement)this["siteServerSettings"]; }
        }

        [ConfigurationProperty("logSettings")]
        public LogSettingsElement LogSettings
        {
            get { return (LogSettingsElement)this["logSettings"]; }
        }

        [ConfigurationProperty("softwareDeploymentSettings")]
        public SoftwareDeploymentSettingsElement SoftwareDeploymentSettings
        {
            get { return (SoftwareDeploymentSettingsElement)this["softwareDeploymentSettings"]; }
        }

        [ConfigurationProperty("securitySettings")]
        public SecuritySettingsElement SecuritySettings
        {
            get { return (SecuritySettingsElement)this["securitySettings"]; }
        }
    }
}
