using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Services.Protocols;
using Swisscom.ConfigMgr.WebSvc.Encrypted.Config;
using System.Xml;

namespace Swisscom.ConfigMgr.WebSvc.Encrypted.Security
{
    public class TimestampValidationExtension : SoapExtension
    {
        public override void ProcessMessage(SoapMessage message)
        {
            if (message.Stage == SoapMessageStage.BeforeDeserialize)
            {
                Stream stream = message.Stream;
                stream.Position = 0;
                string body;
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                {
                    body = reader.ReadToEnd();
                }
                stream.Position = 0;

                // Extract timestamp node from SOAP XML
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(body);
                var tsNode = xmlDoc.SelectSingleNode("//*[local-name()='timestamp']");

                // Instantiate ConfigHandler and SecurityHandler for validation
                var configHandler = new ConfigHandler();
                var securityHandler = new SecurityHandler(configHandler);

                if (tsNode == null || !securityHandler.IsTimestampValid(tsNode.InnerText))
                {
                    HttpContext.Current.Response.StatusCode = 403;
                    HttpContext.Current.Response.Write("Invalid or expired timestamp");
                    HttpContext.Current.Response.End();
                }
            }
        }

        public override Stream ChainStream(Stream stream) => stream;
        public override object GetInitializer(Type serviceType) => null;
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute) => null;
        public override void Initialize(object initializer) { }
    }
}
