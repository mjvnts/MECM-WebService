using System;
using System.Web.Services.Protocols;
using System.IO;
using System.Text;
using System.Web;
using Swisscom.ConfigMgr.WebSvc.Encrypted.Config;

namespace Swisscom.ConfigMgr.WebSvc.Encrypted.Security
{
    public class TransportSecurityExtension : SoapExtension
    {
        private Stream _originalStream;
        private MemoryStream _processedStream;
        private SecurityHandler _securityHandler;
        private bool _wasRequestEncrypted = false;

        public override void ProcessMessage(SoapMessage message)
        {
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeDeserialize:
                    ProcessRequest();
                    break;

                case SoapMessageStage.AfterSerialize:
                    ProcessResponse();
                    break;
            }
        }

        private void ProcessRequest()
        {
            try
            {
                _originalStream.Position = 0;
                using (var reader = new StreamReader(_originalStream, Encoding.UTF8))
                {
                    string rawPayload = reader.ReadToEnd();

                    // Load configuration
                    var configHandler = new ConfigHandler();
                    bool requireEncryption = configHandler.RequireEncryption;

                    // Switch logic based on config
                    if (requireEncryption && !SecurityHandler.IsEncrypted(rawPayload))
                    {
                        // Only encrypted requests allowed, but received unencrypted
                        HttpContext.Current.Response.StatusCode = 403;
                        HttpContext.Current.Response.Write("Only encrypted requests are allowed");
                        HttpContext.Current.Response.End();
                        return;
                    }

                    // Decide: encrypted or not?
                    _wasRequestEncrypted = SecurityHandler.IsEncrypted(rawPayload);

                    if (_wasRequestEncrypted)
                    {
                        if (_securityHandler == null)
                        {
                            _securityHandler = new SecurityHandler(configHandler);
                        }

                        string decryptedXml = _securityHandler.DecryptPayload(rawPayload);
                        byte[] decryptedBytes = Encoding.UTF8.GetBytes(decryptedXml);

                        _processedStream.Write(decryptedBytes, 0, decryptedBytes.Length);
                    }
                    else
                    {
                        // Pass through unencrypted
                        byte[] originalBytes = Encoding.UTF8.GetBytes(rawPayload);
                        _processedStream.Write(originalBytes, 0, originalBytes.Length);
                    }

                    _processedStream.Position = 0;
                }
            }
            catch (Exception ex)
            {
                WriteErrorToProcessedStream($"Request error: {ex.Message}");
            }
        }

        private void ProcessResponse()
        {
            try
            {
                _processedStream.Position = 0;
                using (var reader = new StreamReader(_processedStream, Encoding.UTF8))
                {
                    string responseXml = reader.ReadToEnd();

                    if (_wasRequestEncrypted)
                    {
                        // Encrypt response (if request was encrypted)
                        if (_securityHandler == null)
                        {
                            var configHandler = new ConfigHandler();
                            _securityHandler = new SecurityHandler(configHandler);
                        }

                        string encryptedResponse = _securityHandler.EncryptPayload(responseXml);

                        var utf8WithoutBom = new UTF8Encoding(false);
                        using (var writer = new StreamWriter(_originalStream, utf8WithoutBom, 1024, true))
                        {
                            writer.Write(encryptedResponse);
                            writer.Flush();
                        }
                    }
                    else
                    {
                        // Unencrypted response
                        var utf8WithoutBom = new UTF8Encoding(false);
                        using (var writer = new StreamWriter(_originalStream, utf8WithoutBom, 1024, true))
                        {
                            writer.Write(responseXml);
                            writer.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var utf8WithoutBom = new UTF8Encoding(false);
                using (var writer = new StreamWriter(_originalStream, utf8WithoutBom, 1024, true))
                {
                    writer.Write($"Response processing error: {ex.Message}");
                    writer.Flush();
                }
            }
        }

        private void WriteErrorToProcessedStream(string error)
        {
            string errorXml = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <soap:Fault>
      <faultcode>Server</faultcode>
      <faultstring>{error}</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>";

            byte[] errorBytes = Encoding.UTF8.GetBytes(errorXml);
            _processedStream.Write(errorBytes, 0, errorBytes.Length);
            _processedStream.Position = 0;
        }

        public override Stream ChainStream(Stream stream)
        {
            _originalStream = stream;
            _processedStream = new MemoryStream();
            return _processedStream;
        }

        public override object GetInitializer(Type serviceType) => null;
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute) => null;
        public override void Initialize(object initializer) { }
    }
}
