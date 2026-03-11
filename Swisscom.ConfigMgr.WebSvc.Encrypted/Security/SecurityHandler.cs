using System;
using Swisscom.ConfigMgr.Library.Util;
using Swisscom.ConfigMgr.WebSvc.Encrypted.Config;
using System.Globalization;

namespace Swisscom.ConfigMgr.WebSvc.Encrypted.Security
{
    public class SecurityHandler
    {
        private readonly ScsCrypto _scsCrypto;
        private readonly string _encryptionKey;
        private readonly string _encryptionSalt;
        private readonly int _timestampToleranceSeconds;

        public SecurityHandler(ConfigHandler configHandler)
        {
            if (configHandler == null)
                throw new ArgumentNullException(nameof(configHandler));

            _encryptionKey = configHandler.EncryptionKey;
            _encryptionSalt = configHandler.EncryptionSalt;
            _timestampToleranceSeconds = configHandler.TimestampToleranceSeconds;

            if (string.IsNullOrWhiteSpace(_encryptionKey))
                throw new ApplicationException("Encryption key is not configured");

            if (!string.IsNullOrWhiteSpace(_encryptionSalt))
                _scsCrypto = new ScsCrypto(_encryptionKey, _encryptionSalt);
            else
                _scsCrypto = new ScsCrypto(_encryptionKey);
        }


        public string DecryptPayload(string encryptedPayload)
        {
            if (string.IsNullOrEmpty(encryptedPayload))
                throw new ArgumentException("Encrypted payload cannot be empty");

            try
            {
                return _scsCrypto.Decrypt(encryptedPayload);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Decryption failed: {ex.Message}", ex);
            }
        }

        public string EncryptPayload(string plainTextPayload)
        {
            if (string.IsNullOrEmpty(plainTextPayload))
            {
                return string.Empty;
            }

            try
            {
                return _scsCrypto.Encrypt(plainTextPayload);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Encryption failed: {ex.Message}", ex);
            }
        }

        public static bool IsEncrypted(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            string trimmed = payload.Trim();

            return !trimmed.StartsWith("<");
        }

        public bool IsTimestampValid(string timestamp)
        {
            if (DateTime.TryParse(timestamp, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime ts))
            {
                var now = DateTime.UtcNow;
                var diff = Math.Abs((now - ts).TotalSeconds);
                return diff <= _timestampToleranceSeconds;
            }
            return false;
        }
    }
}
