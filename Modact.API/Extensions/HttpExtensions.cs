using System.Net;
using System.Text;

namespace Modact.API
{
    public static class HttpExtensions
    {
        public static string? ClientAppId(this HttpRequest httpRequest)
        {
            var value = httpRequest.Headers["X-Modact-AppId"].ToString();
            
            if (string.IsNullOrEmpty(value))
            {
                value = httpRequest.Headers["X-App-Id"].ToString();
            }

            return value;
        }

        public static string? ClientAppName(this HttpRequest httpRequest)
        {
            var value = httpRequest.Headers["X-Modact-AppName"].ToString();

            if (string.IsNullOrEmpty(value))
            {
                value = httpRequest.Headers["X-App-Name"].ToString();
            }

            return value;
        }

        public static string? ClientAppSecret(this HttpRequest httpRequest)
        {
            var value = httpRequest.Headers["X-Modact-AppSecret"].ToString();

            if (string.IsNullOrEmpty(value))
            {
                value = httpRequest.Headers["X-App-Secret"].ToString();
            }

            return value;          
        }

        public static string? ClientAppVersion(this HttpRequest httpRequest)
        {
            var value = httpRequest.Headers["X-Modact-AppVersion"].ToString();

            if (string.IsNullOrEmpty(value))
            {
                value = httpRequest.Headers["X-Modact-AppVersion"].ToString();
            }

            return value;
        }

        public static string? ClientMacAddress(this HttpRequest httpRequest)
        {
            var value = httpRequest.Headers["X-Modact-MacAddress"].ToString();

            if (string.IsNullOrEmpty(value))
            {
                value = httpRequest.Headers["X-App-MacAddress"].ToString();
            }

            return value;
        }

        public static string? ClientMachineCode(this HttpRequest httpRequest)
        {
            var value = httpRequest.Headers["X-Modact-MachineCode"].ToString();

            return value;

        }

        public static string? ClientMachineName(this HttpRequest httpRequest)
        {
            var value = httpRequest.Headers["X-Modact-MachineName"].ToString();

            if (string.IsNullOrEmpty(value))
            {
                value = httpRequest.ClientName();
            }

            return value;
        }

        public static string? ClientType(this HttpRequest httpRequest)
        {
            try
            {
                if (httpRequest.ClientUserAgent().Contains("MODACTINSIDEIDENTITYWIN"))
                {
                    return "WIN";
                }
                else if (httpRequest.ClientUserAgent().Contains("MODACTINSIDEIDENTITYAPK"))
                {
                    return "APK";
                }
                else if (httpRequest.ClientUserAgent().Contains("MODACTINSIDEIDENTITYIOS"))
                {
                    return "IOS";
                }
                else if (httpRequest.ClientUserAgent().Contains("MODACTINSIDEIDENTITYMAC"))
                {
                    return "MAC";
                }
                else if (httpRequest.ClientUserAgent().Contains("MODACTINSIDEIDENTITYLNX"))
                {
                    return "LNX";
                }
            }
            catch
            {
                return "WEB";
            }
            return "WEB";
        }

        public static string? ClientUserAgent(this HttpRequest httpRequest)
        {
            try
            {
                return httpRequest.Headers["User-Agent"].ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string? ClientIP(this HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                return null;
            }
            try
            {
                string ip = httpRequest.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ip))
                {
                    return httpRequest.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                }
                else
                {
                    return ip;
                }
            }
            catch
            {
                try
                {
                    return httpRequest.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string? ClientName(this HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                return null;
            }
            try
            {
                string host = httpRequest.Headers["X-Forwarded-Host"].FirstOrDefault();
                if (string.IsNullOrEmpty(host))
                {
                    IPHostEntry ipHost = Dns.GetHostEntry(IPAddress.Parse(ClientIP(httpRequest)));
                    List<string> hostName = ipHost.HostName.ToString().Split('.').ToList();
                    return hostName.First();
                }
                else
                {
                    return host;
                }

            }
            catch
            {
                return null;
            }

        }

        public static string? Origin(this HttpRequest httpRequest)
        {
            try
            {
                return httpRequest.Headers["Origin"].ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string? Url(this HttpRequest httpRequest)
        {
            try
            {
                var url = new StringBuilder();
                url.Append(httpRequest.Scheme);
                url.Append("://");
                url.Append(httpRequest.Host);
                url.Append(httpRequest.PathBase);
                url.Append(httpRequest.Path);
                url.Append(httpRequest.QueryString);
                return url.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string? UrlWithHttpMethod(this HttpRequest httpRequest)
        {
            try
            {
                var url = new StringBuilder();
                url.Append("[");
                url.Append(httpRequest.Method);
                url.Append("]");
                url.Append(httpRequest.Url());
                return url.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetRawBodyAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (!request.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                request.EnableBuffering();
            }

            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            request.Body.Position = 0;

            return body;
        }

        public static async Task<string> GetRawBodyAsync(this HttpResponse response, Encoding encoding = null)
        {
            response.Body.Position = 0;

            using var reader = new StreamReader(response.Body, encoding ?? Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            response.Body.Position = 0;

            return body;
        }
    }
}
