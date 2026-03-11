using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Swisscom.ConfigMgr.WebSvc.Encrypted.Helpers
{
    [Serializable]
    public class GenericSoapResponse<T>
    {
        public bool Success { get; set; }

        public T Data { get; set; }

        [XmlElement(IsNullable = true)]
        public string Error { get; set; }

        public string Timestamp { get; set; }

         // Only serialise data field if not null
        public bool ShouldSerializeData() => Data != null;
        public bool ShouldSerializeError() => !string.IsNullOrEmpty(Error);
    }

    public static class SoapResponseHelper
    {
        public static GenericSoapResponse<T> Success<T>(T data = default)
        {
            return new GenericSoapResponse<T>
            {
                Success = true,
                Data = data,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        public static GenericSoapResponse<T> Failure<T>(string error)
        {
            return new GenericSoapResponse<T>
            {
                Success = false,
                Error = error,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
    }
}
