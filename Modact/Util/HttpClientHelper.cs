using System.Net.Http;
using System.Net;

namespace Modact
{
    public class HttpClientHelper
    {
        private static HttpClient _client;

        private static readonly object _lock = new object();

        public static HttpClient GetClient(string? authorization = null, ApiRequestInfo? apiRequestInfo = null) //[authorization] may use array for next version
        {

            try
            {
                if (_client == null)
                {
                    lock (_lock)
                    {
                        if (_client == null)
                        {

                            HttpClientHandler handler = new HttpClientHandler();
                            handler.UseProxy = false;
                            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                            
                            //ignore cert error start - only for dev
                            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };
                            //ignore cert error end - only for dev

                            _client = new HttpClient(handler);
                            _client.DefaultRequestHeaders.Connection.Add("keep-alive");
                            _client.Timeout = new TimeSpan(0, 0, 20);
                            if (apiRequestInfo != null)
                            {
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAgent)) { _client.DefaultRequestHeaders.Add("User-Agent", apiRequestInfo.RequestAgent); }
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppId)) { _client.DefaultRequestHeaders.Add("X-App-Id", apiRequestInfo.RequestAppId); }//Obsolete in future
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppName)) { _client.DefaultRequestHeaders.Add("X-App-Name", apiRequestInfo.RequestAppName); }//Obsolete in future
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppSecret)) { _client.DefaultRequestHeaders.Add("X-App-Secret", apiRequestInfo.RequestAppSecret); }//Obsolete in future
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppVersion)) { _client.DefaultRequestHeaders.Add("X-App-Version", apiRequestInfo.RequestAppVersion); }//Obsolete in future
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppId)) { _client.DefaultRequestHeaders.Add("X-Modact-AppId", apiRequestInfo.RequestAppId); }
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppName)) { _client.DefaultRequestHeaders.Add("X-Modact-AppName", apiRequestInfo.RequestAppName); }
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppSecret)) { _client.DefaultRequestHeaders.Add("X-Modact-AppSecret", apiRequestInfo.RequestAppSecret); }
                                if (!string.IsNullOrEmpty(apiRequestInfo.RequestAppVersion)) { _client.DefaultRequestHeaders.Add("X-Modact-AppVersion", apiRequestInfo.RequestAppVersion); }
                            }
                            
                            if (!string.IsNullOrEmpty(authorization))
                            {
                                _client.DefaultRequestHeaders.Add("Authorization", authorization);
                            }
                            return _client;
                        }
                    }
                }
                return _client;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
