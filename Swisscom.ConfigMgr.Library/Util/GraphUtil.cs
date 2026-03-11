using Azure.Identity;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.DeviceManagement.ComanagedDevices.Item.Users;
using Microsoft.Graph.Beta.Models;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Swisscom.ConfigMgr.Library.Util
{
    public class GraphUtil : IDisposable
    {
        private static IConfidentialClientApplication _msalApp;
        private static readonly object _msalLock = new object();

        private readonly HttpClient _httpClient = null;
        private readonly string _token = string.Empty;
        private bool _isDisposed;

        public string AppId { get; private set; }
        public string AppDisplayName { get; private set; }
        public string TenantId { get; private set; }
        public string GraphUrl { get; private set; }
        public string SecretString { get; private set; }

        public enum WebMethod
        {
            Get,
            Post,
            Put,
            Delete
        }

        public GraphUtil(string appDisplayName, string appId, string tenantId, string graphUrl, string secretString)
        {
            this._isDisposed = false;

            // Values from app registration
            this.AppId = appId;
            this.TenantId = tenantId;
            this.GraphUrl = graphUrl;
            this.AppDisplayName = appDisplayName;
            this.SecretString = secretString;

            // /.default scope, and pre-configure your permissions on the
            // app registration in Azure. An administrator must grant consent
            // to those permissions beforehand.
            // this.scopes = new[] { "https://graph.microsoft.com/.default" };
            // this._graphClient = this.GetGraphClient();

            this._httpClient = new HttpClient();
            this._httpClient.BaseAddress = new Uri(this.GraphUrl);
            this._token = this.GetAccessToken(this.TenantId, this.AppId, this.SecretString)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        ~ GraphUtil()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                if (disposing)
                {
                    if (this._httpClient != null) this._httpClient.Dispose();
                }
            }

            this._isDisposed = true;
        }
        
        // INTUNE METHODS

        public string GetIntuneDeviceIdByName(string name)
        {
            var content = string.Empty;
            var request = this.GetWebRequest(WebMethod.Get, "deviceManagement/managedDevices?$filter=(deviceName eq '" + name + "')");
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    var streamReader = new System.IO.StreamReader(responseStream, true);
                    content = streamReader.ReadToEnd();
                }
            }

            var devices = JsonConvert.DeserializeObject<ManagedDeviceCollectionResponse>(content);
            foreach (var device in devices.Value)
            {
                return device.Id;
            }

            return string.Empty;
        }

        public string GetPrimaryUser(string deviceId)
        {
            var content = string.Empty;
            var request = this.GetWebRequest(WebMethod.Get, "deviceManagement/managedDevices/" + deviceId + "/users");
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    var streamReader = new System.IO.StreamReader(responseStream, true);
                    content = streamReader.ReadToEnd();
                }
            }

            var users = JsonConvert.DeserializeObject<UserCollectionResponse>(content);
            foreach (var user in users.Value)
            {
                return user.UserPrincipalName;
            }
            return string.Empty;
        }

        public bool SetPrimaryUser(string deviceId, string userId)
        {
            var retVal = false;
            var request = this.GetWebRequest(WebMethod.Post, "deviceManagement/managedDevices('" + deviceId + "')/users/$ref");
            var body = "{ \"@odata.id\":  \"" + this.GraphUrl + "users/" + userId + "\"}";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] data = encoding.GetBytes(body);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var content = this.GetResponse(request);
            if (string.IsNullOrEmpty(content)) retVal = true;
            return retVal;
        }

        public bool IsDeviceCoManaged(string deviceId)
        {
            var request = this.GetWebRequest(
                WebMethod.Get,
                "deviceManagement/managedDevices/" + deviceId + "?$select=id,managementAgent,deviceEnrollmentType"
            );

            var content = this.GetResponse(request);

            var device = JsonConvert.DeserializeObject<ManagedDevice>(content);

            if (device != null)
            {
                // Co-Management detection via ManagementAgent enum
                if (device.ManagementAgent == ManagementAgentType.ConfigurationManagerClientMdm)
                {
                    return true;
                }
                // Additionally check via DeviceEnrollmentType
                if (device.DeviceEnrollmentType == DeviceEnrollmentType.WindowsCoManagement)
                {
                    return true;
                }
            }

            return false;
        }

        public bool SetDeviceCategory(string deviceId, string deviceCategoryName)
        {
            var retVal = false;

            // 1. Retrieve category ID
            var catRequest = this.GetWebRequest(WebMethod.Get, "deviceManagement/deviceCategories?$select=id,displayName");
            var catContent = this.GetResponse(catRequest);

            var catColl = JsonConvert.DeserializeObject<DeviceCategoryCollectionResponse>(catContent);
            var category = catColl?.Value?.FirstOrDefault(c =>
                c.DisplayName != null && c.DisplayName.Equals(deviceCategoryName, StringComparison.OrdinalIgnoreCase));
            if (category == null)
                throw new InvalidOperationException($"Device Category '{deviceCategoryName}' not found in Intune!");

            // 2. Set the Device Category on the Managed Device
            var putEndpoint = $"deviceManagement/managedDevices/{deviceId}/deviceCategory/$ref";
            var putRequest = this.GetWebRequest(WebMethod.Put, putEndpoint);

            // Set @odata.id body to reference the Category object
            var body = "{ \"@odata.id\": \"" + this.GraphUrl + "deviceManagement/deviceCategories/" + category.Id + "\"}";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] data = encoding.GetBytes(body);
            putRequest.ContentLength = data.Length;

            try
            {
                using (var stream = putRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var putContent = this.GetResponse(putRequest);
                // The endpoint typically returns an empty body on success
                if (string.IsNullOrEmpty(putContent)) retVal = true;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse &&
                    errorResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    retVal = false; // Device or category not found
                }
                else
                {
                    throw; // Andere Fehler weiterwerfen
                }
            }
            return retVal;
        }

        public bool DeleteIntuneDevice(string deviceId)
        {
            var request = this.GetWebRequest(WebMethod.Delete, "deviceManagement/managedDevices/" + deviceId);
            try
            {
                var content = this.GetResponse(request);
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse && errorResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return false; // Device already gone
                }
                throw;
            }
        }


        // ENTRA METHODS

        public string GetUserIdByUpn(string userPrincipalName)
        {
            var request = this.GetWebRequest(WebMethod.Get, "users?$filter=(userPrincipalName eq '" + userPrincipalName + "')");
            var content = this.GetResponse(request);

            var users = JsonConvert.DeserializeObject<UserCollectionResponse>(content);
            foreach (var user in users.Value)
            {
                return user.Id;
            }

            return string.Empty;
        }

        public string GetUserIdBySamAccountName(string samAccountName)
        {
            var request = this.GetWebRequest(WebMethod.Get, "users?$filter=onPremisesSamAccountName eq '" + samAccountName + "'&$count=true", true);
            var content = this.GetResponse(request);

            var users = JsonConvert.DeserializeObject<UserCollectionResponse>(content);
            foreach (var user in users.Value)
            {
                return user.Id;
            }

            return string.Empty;
        }

        public string GetEntraDeviceIdByName(string name)
        {
            var content = string.Empty;
            var request = this.GetWebRequest(WebMethod.Get, "devices?$filter=displayName eq '" + Uri.EscapeDataString(name) + "'&$select=id");
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    var streamReader = new System.IO.StreamReader(responseStream, true);
                    content = streamReader.ReadToEnd();
                }
            }

            var devices = JsonConvert.DeserializeObject<DeviceCollectionResponse>(content);
            foreach (var device in devices.Value)
            {
                return device.Id;
            }

            return string.Empty;
        }

        public string GetGroupIdByName(string groupName)
        {
            var request = this.GetWebRequest(WebMethod.Get, "groups?$filter=displayName eq '" + Uri.EscapeDataString(groupName) + "'&$select=id");
            var content = this.GetResponse(request);

            var groups = JsonConvert.DeserializeObject<GroupCollectionResponse>(content);
            foreach (var group in groups.Value)
            {
                return group.Id;
            }
            return string.Empty;
        }

        public bool IsDeviceMemberOfGroup(string deviceId, string groupId)
        {
            var request = this.GetWebRequest(WebMethod.Get, "groups/" + groupId + "/members?$select=id");
            var content = this.GetResponse(request);

            var members = JsonConvert.DeserializeObject<DirectoryObjectCollectionResponse>(content);
            return members.Value.Any(m => m.Id == deviceId);
        }

        public bool AddDeviceToGroup(string deviceId, string groupId)
        {
            var retVal = false;
            var request = this.GetWebRequest(WebMethod.Post, "groups/" + groupId + "/members/$ref");
            var body = "{ \"@odata.id\": \"" + this.GraphUrl + "directoryObjects/" + deviceId + "\"}";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] data = encoding.GetBytes(body);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var content = this.GetResponse(request);
            if (string.IsNullOrEmpty(content)) retVal = true;
            return retVal;
        }

        public bool RemoveDeviceFromGroup(string deviceId, string groupId)
        {
            var retVal = false;
            var request = this.GetWebRequest(WebMethod.Delete, "groups/" + groupId + "/members/" + deviceId + "/$ref");
            try
            {
                var content = this.GetResponse(request);
                if (string.IsNullOrEmpty(content)) retVal = true;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse && errorResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    retVal = false; 
                }
                else
                {
                    throw; 
                }
            }
            return retVal;
        }

        public bool IsUserMemberOfGroup(string userId, string groupId)
        {
            var request = this.GetWebRequest(WebMethod.Get, "groups/" + groupId + "/members?$select=id");
            var content = this.GetResponse(request);
            var members = JsonConvert.DeserializeObject<DirectoryObjectCollectionResponse>(content);
            return members.Value.Any(m => m.Id == userId);
        }

        public bool AddUserToGroup(string userId, string groupId)
        {
            var retVal = false;
            var request = this.GetWebRequest(WebMethod.Post, "groups/" + groupId + "/members/$ref");
            var body = "{ \"@odata.id\": \"" + this.GraphUrl + "directoryObjects/" + userId + "\"}";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] data = encoding.GetBytes(body);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var content = this.GetResponse(request);
            if (string.IsNullOrEmpty(content)) retVal = true;
            return retVal;
        }

        public bool RemoveUserFromGroup(string userId, string groupId)
        {
            var retVal = false;
            var request = this.GetWebRequest(WebMethod.Delete, "groups/" + groupId + "/members/" + userId + "/$ref");
            try
            {
                var content = this.GetResponse(request);
                if (string.IsNullOrEmpty(content)) retVal = true;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse && errorResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    retVal = false;
                }
                else
                {
                    throw;
                }
            }
            return retVal;
        }



        private string GetResponse(HttpWebRequest request)
        {
            var content = string.Empty;

            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new System.IO.StreamReader(responseStream, true))
                    {
                        content = streamReader.ReadToEnd();
                    }
                }
            }

            return content;
        }

        private HttpWebRequest GetWebRequest(WebMethod webMethod, string urlSubString, bool advancedQuery = false)
        {
            var url = this.GraphUrl + urlSubString;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.PreAuthenticate = true;
            switch (webMethod)
            {
                case WebMethod.Get:
                    request.Method = "GET";
                    request.Accept = "application/json";
                    break;
                case WebMethod.Post:
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    break;
                case WebMethod.Put:
                    request.Method = "PUT";
                    request.ContentType = "application/json";
                    break;
                case WebMethod.Delete:
                    request.Method = "DELETE";
                    break;
                default:
                    throw new ArgumentException("Unsupported web method: " + webMethod);
            }
            request.Headers.Add("Authorization", "Bearer " + this._token);
            if (advancedQuery) request.Headers.Add("ConsistencyLevel", "eventual");

            return request;
        }

        private async Task<string> GetAccessToken(string tenantId, string clientId, string clientSecret)
        {
            var app = GetOrCreateMsalApp(tenantId, clientId, clientSecret);
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var tokenResult = await app.AcquireTokenForClient(scopes).ExecuteAsync().ConfigureAwait(false);
            return tokenResult.AccessToken;
        }

        private static IConfidentialClientApplication GetOrCreateMsalApp(string tenantId, string clientId, string clientSecret)
        {
            if (_msalApp != null)
                return _msalApp;

            lock (_msalLock)
            {
                if (_msalApp != null)
                    return _msalApp;

                _msalApp = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithTenantId(tenantId)
                    .Build();

                return _msalApp;
            }
        }
    }
}
