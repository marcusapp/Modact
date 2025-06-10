using System.Text.Json.Serialization;
using System.Text.Json;
using Modact.Data.Cache;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Modact.API
{
    internal class SessionAPI
    {
        public SessionAPI() { }
        public void RegisterAPI(WebApplication app)
        {
            app.MapGet("/session", async (HttpContext context) =>
            {
                ApiConnect? apiConnect = null;
                var result = new SessionResult();
                try
                {
                    apiConnect = new ApiConnect(new AppSettings(app.Configuration));
                    apiConnect.JsonSerializerOptions = apiConnect.AppSettings.GetJsonSerializerOptions();
                    apiConnect.ApiRequestReceiveInit(null)
                        .FillContextInfo(context)
                        .CacheInit();

                    if (!string.IsNullOrEmpty(apiConnect.AppSettings.DataConfig.AppDatabase))
                    {
                        apiConnect.CacheInit();
                    }
                    if (!apiConnect.IsValidAppSecret())
                    {
                        throw new UnauthorizedAccessException("API Key Unauthorized.");
                    }
                    if (apiConnect.AppSettings.IsUserTokenEnable())
                    {
                        if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
                        {
                            apiConnect.ApiResource.UserPermission = new UserPermission(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, null);
                            result.UserPermission = apiConnect.ApiResource.UserPermission.Permissions;
                        }
                        else
                        {
                            if (!apiConnect.IsValidUserToken(context.Request.Headers.Authorization))
                            {
                                if (apiConnect.UserTokenError == null) { apiConnect.UserTokenError = new List<string>(); }
                                apiConnect.UserTokenError.Add(context.Request.Headers.Authorization);
                                throw new UnauthorizedAccessException("User Unauthorized or Session Expired.");
                            }
                            apiConnect.ApiResource.UserToken = JwtTokenHelper.GetObjectInToken<UserToken>(context.Request.Headers.Authorization);
                            apiConnect.ApiResource.UserPermission = new UserPermission(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, apiConnect.ApiResource.UserToken);
                            //apiConnect.ApiResource.UserPermission.AppendPermission(new UserPermission(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, null));
                            result.UserPermission = apiConnect.ApiResource.UserPermission.Permissions;

                            try
                            {
                                if (apiConnect.ApiResource.UserToken != null && !string.IsNullOrEmpty(apiConnect.AppSettings.DataConfig.AppDatabase))
                                {
                                    apiConnect.LogTokenToAppDatabase(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, result.AppTime);
                                }
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        apiConnect.ApiResource.IsUserTokenEnable = false;
                    }
                    if (DatabaseCache.ConfigCache != null)
                    {
                        result.AppNode = DatabaseCache.AppNodeCache;
                        result.AppConfig = DatabaseCache.ConfigCache;
                    }
                }
                catch (Exception e)
                {
                    result.ErrorMessage = e.Message;
                }
                finally
                {
                    if (apiConnect != null)
                    {
                        apiConnect.FinallySession();
                    }
                }
                result.AppNode = DatabaseCache.AppNodeCache;

                await context.Response.WriteAsJsonAsync(result, apiConnect.JsonSerializerOptions);
            });

            app.MapPost("/session", async (HttpContext context) =>
            {
                ApiConnect? apiConnect = null;
                apiConnect = await apiConnect.NewProcess(app, context);
            });
        }
    }

    internal class SessionResult
    {
        public DateTime AppTime { get; set; }
        public List<AppNodeCache>? AppNode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> AppConfig { get; set; }
        public List<string> UserPermission { get; set; }
        public ApiResponse ApiResponse { get; set; }

        public SessionResult()
        {
            AppTime = DateTime.Now;
        }

    }
}