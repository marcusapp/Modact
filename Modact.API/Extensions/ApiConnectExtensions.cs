using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Modact.Data.Cache;
using Modact.Data.Models;

namespace Modact.API
{
    public static class ApiConnectExtensions
    {
        public static async Task<ApiConnect> NewProcess(this ApiConnect? apiConnect, WebApplication app, HttpContext context)
        {
            var requestBody = await context.Request.GetRawBodyAsync(Encoding.UTF8);
            try
            {
                apiConnect = new ApiConnect(new AppSettings(app.Configuration));
                apiConnect.ApiRequestReceiveInit(JsonSerializer.Deserialize<ApiRequest>(requestBody, apiConnect.JsonSerializerOptions))
                    .FillContextInfo(context)
                    .CacheInit();

                if (!apiConnect.IsValidDevice())
                {
                    throw new ApiMessageException("Device Unauthorized." + Environment.NewLine + 
                        "IP: " + apiConnect.ApiRequestInfo.RequestIP + Environment.NewLine +
                        "Name: " + apiConnect.ApiRequestInfo.RequestMachineName + Environment.NewLine +
                        "Mac: " + apiConnect.ApiRequestInfo.RequestMacAddress + Environment.NewLine +
                        "Code: " + apiConnect.ApiRequestInfo.RequestMachineCode
                        , "[MSG]Action.Unauthorized.Device");
                }

                if (!apiConnect.IsValidAppSecret())
                {
                    throw new ApiMessageException("API Key Unauthorized.", "[MSG]Action.Unauthorized.ApiKey");
                }

                string? userAuthToken = context.Request.Headers.Authorization;
                if (!apiConnect.IsValidUserToken(userAuthToken))
                {
                    throw new ApiMessageException("User Unauthorized or Session Expired.", "[MSG]Action.Unauthorized.UserToken");
                }

                if (!apiConnect.IsValidDeviceUser())
                {
                    throw new ApiMessageException("User Unauthorized To Device.", "[MSG]Action.Unauthorized.DeviceUser");
                }
                apiConnect.GetDevicePermission();
                apiConnect.InvokeActionsFunctions();
                apiConnect.Success = true;
                apiConnect.AppendSessionInfo();
                await context.Response.WriteAsJsonAsync(apiConnect.ApiResponse, apiConnect.JsonSerializerOptions);
            }
            catch (Exception e)
            {
                ApiResponse? response = null;
                if (apiConnect != null)
                {
                    apiConnect.FatalMessageAdd(e.ToString());
                    if (apiConnect.ApiRequest == null) { apiConnect.RequestBody = requestBody.ToString(); }
                    if (apiConnect.ApiResponse != null) { response = apiConnect.ApiResponse; }
                }

                if (response == null)
                {
                    response = (apiConnect.ApiRequest == null) ? new ApiResponse(Ulid.NewUlid().ToString(), string.Empty) : new ApiResponse(Ulid.NewUlid().ToString(), apiConnect.ApiRequest.RequestId);
                    var appSettings = new AppSettings(app.Configuration);
                    response.AppId = appSettings.AppConfig.AppId;
                    response.AppVersion = appSettings.AppVersionString;
                }

                var errMsg = new ApiMessage(ApiMessageType.Fatal);
                if (e is ApiMessageException) { errMsg.Code = ((ApiMessageException)e).Code ?? string.Empty; }
                else { errMsg.Code = ApiMessageCode.UNKNOWN.ToString(); }
                errMsg.Message = e.Message;
                errMsg.Detail = e.ToString();
                response.AddMessage(errMsg);

                await context.Response.WriteAsJsonAsync(response, apiConnect.AppSettings.GetJsonSerializerOptions());
            }
            finally
            {
                if (apiConnect != null)
                {
                    if (apiConnect.ApiRequest == null) 
                    { 
                        apiConnect.FinallyAction();
                    }
                    else
                    {
                        if (apiConnect.ApiRequest.IsGetSession == true)
                        {
                            apiConnect.FinallySession();
                        }
                        else
                        {
                            apiConnect.FinallyAction();
                        }
                    }
                }
            }
            return apiConnect;
        }
        public static ApiConnect FillContextInfo(this ApiConnect apiConnect, HttpContext context)
        {
            if (apiConnect.ApiRequest.RequestInfo == null)
            {
                apiConnect.ApiRequestInfo = new ApiRequestInfo
                {
                    RequestToUrl = context.Request.Url(),
                    RequestHttpMethod = context.Request.Method,
                    RequestAgent = context.Request.ClientUserAgent(),
                    RequestIP = context.Request.ClientIP(),
                    RequestOriginUrl = context.Request.Origin(),
                    RequestClientType = context.Request.ClientType(),
                    RequestAppId = context.Request.ClientAppId() ?? string.Empty,
                    RequestAppName = context.Request.ClientAppName() ?? string.Empty,
                    RequestAppVersion = context.Request.ClientAppVersion() ?? string.Empty,
                    RequestMacAddress = context.Request.ClientMacAddress() ?? string.Empty,
                    RequestMachineName = context.Request.ClientMachineName() ?? string.Empty,
                    RequestMachineCode = context.Request.ClientMachineCode() ?? string.Empty,
                    RequestAppSecret = context.Request.ClientAppSecret() ?? string.Empty,
                };
            }
            else
            {
                apiConnect.ApiRequestInfo = new ApiRequestInfo
                {
                    RequestToUrl = context.Request.Url(),
                    RequestHttpMethod = context.Request.Method,
                    RequestAgent = context.Request.ClientUserAgent(),
                    RequestIP = context.Request.ClientIP(),
                    RequestOriginUrl = context.Request.Origin(),
                    RequestClientType = context.Request.ClientType(),
                    RequestAppId = apiConnect.ApiRequest.RequestInfo.AppId ?? string.Empty,
                    RequestAppName = apiConnect.ApiRequest.RequestInfo.AppName ?? string.Empty,
                    RequestAppVersion = apiConnect.ApiRequest.RequestInfo.AppVersion ?? string.Empty,
                    RequestAppSecret = apiConnect.ApiRequest.RequestInfo.AppSecret ?? string.Empty,
                    RequestMacAddress = apiConnect.ApiRequest.RequestInfo.MacAddress ?? string.Empty,
                    RequestMachineName = apiConnect.ApiRequest.RequestInfo.MachineName ?? string.Empty,
                    RequestMachineCode = apiConnect.ApiRequest.RequestInfo.MachineCode ?? string.Empty,
                };
            }
            return apiConnect;
        }
        public static ApiConnect CacheInit(this ApiConnect apiConnect)
        {
            if (string.IsNullOrEmpty(apiConnect.AppSettings.DataConfig.AppDatabase))
            {
                return apiConnect;
            }
            if (!DatabaseCache.IsInited)
            {
                if (apiConnect.ApiResource.DatabasesNonTransactional != null)
                {
                    if (apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase != null)
                    {
                        DatabaseCache.Refresh(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase);
                    }
                }
                
            }
            return apiConnect;
        }
        public static ApiConnect InvokeActionsFunctions(this ApiConnect apiConnect)
        {
            List<string> funIdList = new List<string>();
            var taskFunctions = new List<Task>();
            if (apiConnect.ApiRequest.Functions != null)
            {
                apiConnect.ApiRequest.Functions.ForEach((f) =>
                {
                    try
                    {
                        f.ApiFunctionAccessory = new(f.Id, f.Fun);
                        f.ApiFunctionAccessory.ApiConnectId = apiConnect.Id;
                        f.ApiFunctionAccessory.ApiConnectStartTime = apiConnect.StartTime;
                        f.ApiFunctionAccessory.JsonSerializerOptions = apiConnect.JsonSerializerOptions;
                        f.ParseTypeAndMethod(apiConnect.AppSettings.AppConfig.Module);
                        if (funIdList.Contains(f.Id))
                        {
                            throw new Exception($"Function ID duplicated: {f.Id}");
                        }
                        funIdList.Add(f.Id);
                        if (apiConnect.ApiResource.IsUserTokenEnable)
                        {
                            if (!apiConnect.ApiResource.UserPermission.IsGrantedPermission(f.Fun, f.AttributePermissionList))
                            {
                                throw new UnauthorizedAccessException($"No user permission: [{f.Id}]{f.Fun}");
                            }
                            if (!apiConnect.ApiResource.UserPermission.IsGrantedDevicePermission(f.Fun, f.AttributePermissionList))
                            {
                                throw new UnauthorizedAccessException($"No device permission: [{f.Id}]{f.Fun}");
                            }
                            f.ApiFunctionAccessory.UserPermission = new UserPermissionInsideFunction(apiConnect.ApiResource.UserPermission, f.ApiFunctionModuleKey);
                        }
                        else
                        {
                            f.ApiFunctionAccessory.UserPermission = new UserPermissionInsideFunction(new UserPermission(isEnableUserPermission: false), f.ApiFunctionModuleKey);
                        }
                        //if (!apiConnect.AppSettings.GetModactConfigValue<bool>("ModactConfig:ApiKeyOption:ApiKeyEnable") && string.IsNullOrEmpty(f.ApiFunctionModuleKey))
                        //{
                        //    throw new UnauthorizedAccessException($"Modact build-in API function is disabled: [{f.Id}]{f.Fun}");
                        //}
                        f.ApiFunctionAccessory.AppSettings = apiConnect.AppSettings;
                        f.ApiFunctionAccessory.ApiRequestInfo = apiConnect.ApiRequestInfo;
                        f.ApiFunctionAccessory.Databases = apiConnect.ApiResource.DatabasesNonTransactional;
                        if (apiConnect.ApiResource.DatabasesNonTransactional != null)
                        {
                            f.ApiFunctionAccessory.LogDatabase = apiConnect.ApiResource.DatabasesNonTransactional.LogDatabase;
                        }
                        f.ApiFunctionAccessory.UserToken = apiConnect.ApiResource.UserToken;                        
                        taskFunctions.Add(f.InvokeAync());
                    }
                    catch (ApiNoMessageException e)
                    {

                    }
                    catch (Exception e)
                    {
                        //if (f.ApiFunctionResult != null) // [Question] If error, data return null or error message?
                        //{
                        //f.ApiFunctionResult.Data = e.Message;
                        //}
                        if (f.ApiFunctionAccessory != null)
                        {
                            if (f.ApiFunctionAccessory.ApiFunctionMessage != null)
                            {
                                var errMsg = new ApiMessage();
                                errMsg.Type = ApiMessageType.Error;
                                errMsg.Code = ApiMessageCode.UNKNOWN.ToString();
                                errMsg.Message = e.Message;
                                errMsg.Detail = e.ToString();
                                errMsg.FunId = f.Id;
                                f.ApiFunctionAccessory.ApiFunctionMessage.Messages.Add(errMsg);
                            }
                        }
                    }
                });
            }

            if (apiConnect.ApiRequest.Actions != null)
            {
                string currentFunId = string.Empty;
                apiConnect.ApiRequest.Actions.ForEach((a) =>
                {
                    if (!a.IsAsync && a.IsTrans)
                    {
                        try
                        {
                            if (a.Functions != null)
                            {
                                Dictionary<string, ApiFunctionResult> funTransactionResults = new Dictionary<string, ApiFunctionResult>();
                                a.Functions.ForEach((f) =>
                                {
                                    currentFunId = f.Id;
                                    apiConnect.ApiResponse.AddResult(f.Id, new ApiFunctionResult());
                                    f.ApiFunctionAccessory = new(f.Id, f.Fun);
                                    f.ApiFunctionAccessory.ApiConnectId = apiConnect.Id;
                                    f.ApiFunctionAccessory.ApiConnectStartTime = apiConnect.StartTime;
                                    f.ApiFunctionAccessory.JsonSerializerOptions = apiConnect.JsonSerializerOptions;
                                    f.ParseTypeAndMethod(apiConnect.AppSettings.AppConfig.Module);
                                    if (funIdList.Contains(f.Id))
                                    {
                                        throw new Exception($"Function ID duplicated: {f.Id}");
                                    }
                                    funIdList.Add(f.Id);
                                    if (apiConnect.ApiResource.IsUserTokenEnable)
                                    {
                                        if (!apiConnect.ApiResource.UserPermission.IsGrantedPermission(f.Fun, f.AttributePermissionList))
                                        {
                                            throw new UnauthorizedAccessException($"No user permission: [{f.Id}]{f.Fun}");
                                        }
                                        if (!apiConnect.ApiResource.UserPermission.IsGrantedDevicePermission(f.Fun, f.AttributePermissionList))
                                        {
                                            throw new UnauthorizedAccessException($"No device permission: [{f.Id}]{f.Fun}");
                                        }
                                        f.ApiFunctionAccessory.UserPermission = new UserPermissionInsideFunction(apiConnect.ApiResource.UserPermission, f.ApiFunctionModuleKey);
                                    }
                                    else
                                    {
                                        f.ApiFunctionAccessory.UserPermission = new UserPermissionInsideFunction(new UserPermission(isEnableUserPermission: false), f.ApiFunctionModuleKey);
                                    }
                                    f.ApiFunctionAccessory.AppSettings = apiConnect.AppSettings;
                                    f.ApiFunctionAccessory.ApiRequestInfo = apiConnect.ApiRequestInfo;
                                    f.ApiFunctionAccessory.Databases = apiConnect.ApiResource.DatabasesTransactional;
                                    if (apiConnect.AppSettings.IsLogToDatabaseEnable())
                                    {
                                        f.ApiFunctionAccessory.LogDatabase = apiConnect.ApiResource.DatabasesNonTransactional.LogDatabase;
                                    }
                                    f.ApiFunctionAccessory.UserToken = apiConnect.ApiResource.UserToken;
                                    foreach (var kp in f.Params)
                                    {
                                        if (kp.Key.ToLower() == "@fun")
                                        {
                                            f.Params[kp.Key] = apiConnect.ApiResponse.Results[kp.Value.ToString()].Data;
                                        }
                                    }
                                    f.Invoke();
                                    funTransactionResults[f.Id] = f.ApiFunctionResult;
                                    apiConnect.ApiResponse.Messages.AddRange(f.ApiFunctionAccessory.ApiFunctionMessage.Messages);
                                    f.ApiFunctionAccessory.Dispose();
                                    if (!f.Success) 
                                    {
                                        foreach (var fun in a.Functions)
                                        {
                                            fun.Success = false;
                                        }
                                        foreach (var funResult in funTransactionResults)
                                        {
                                            funResult.Value.Success = false;
                                            funResult.Value.Data = "(Rollback due to fail of FunId: " + f.Id + ")";
                                        }
                                        foreach (var kp in funTransactionResults)
                                        {
                                            apiConnect.ApiResponse.Results[kp.Key] = kp.Value;
                                        }
                                        throw new ApiNoMessageException(); 
                                    }
                                });
                                apiConnect.ApiResource.DatabasesTransactional.CommitAll();
                                foreach (var kp in funTransactionResults)
                                {
                                    apiConnect.ApiResponse.Results[kp.Key] = kp.Value;
                                }

                            }
                        }
                        catch (ApiNoMessageException e)
                        {
                            apiConnect.ApiResource.DatabasesTransactional.RollbackAll();
                        }
                        catch (ApiMessageException e)
                        {
                            var errMsg = new ApiMessage();
                            errMsg.Type = ApiMessageType.Error;
                            errMsg.Code = e.Code ?? string.Empty;
                            errMsg.Message = e.Message;
                            errMsg.Detail = e.ToString();
                            errMsg.FunId = currentFunId;
                            apiConnect.ApiResponse.AddMessage(errMsg);
                            //apiConnect.ApiResponse.Results[currentFunId].Data = e.Message; // [Question] If error, data return null or error message?
                            apiConnect.ApiResource.DatabasesTransactional.RollbackAll();
                        }
                        catch (Exception e)
                        {
                            var errMsg = new ApiMessage();
                            errMsg.Type = ApiMessageType.Error;
                            errMsg.Code = ApiMessageCode.UNKNOWN.ToString();
                            errMsg.Message = e.Message;
                            errMsg.Detail = e.ToString();
                            errMsg.FunId = currentFunId;
                            apiConnect.ApiResponse.AddMessage(errMsg);
                            //apiConnect.ApiResponse.Results[currentFunId].Data = e.Message; // [Question] If error, data return null or error message?
                            apiConnect.ApiResource.DatabasesTransactional.RollbackAll();
                        }                        
                    }
                    else if (!a.IsAsync && !a.IsTrans)
                    {
                        if (a.Functions != null)
                        {
                            a.Functions.ForEach((f) =>
                            {
                                try
                                {
                                    currentFunId = f.Id;
                                    apiConnect.ApiResponse.AddResult(f.Id, new ApiFunctionResult());
                                    f.ApiFunctionAccessory = new(f.Id, f.Fun);
                                    f.ApiFunctionAccessory.ApiConnectId = apiConnect.Id;
                                    f.ApiFunctionAccessory.ApiConnectStartTime = apiConnect.StartTime;
                                    f.ApiFunctionAccessory.JsonSerializerOptions = apiConnect.JsonSerializerOptions;
                                    f.ParseTypeAndMethod(apiConnect.AppSettings.AppConfig.Module);
                                    if (funIdList.Contains(f.Id))
                                    {
                                        throw new Exception($"Function ID duplicated: {f.Id}");
                                    }
                                    funIdList.Add(f.Id);
                                    if (apiConnect.ApiResource.IsUserTokenEnable)
                                    {
                                        if (!apiConnect.ApiResource.UserPermission.IsGrantedPermission(f.Fun, f.AttributePermissionList))
                                        {
                                            throw new UnauthorizedAccessException($": [{f.Id}]{f.Fun}");
                                        }
                                        f.ApiFunctionAccessory.UserPermission = new UserPermissionInsideFunction(apiConnect.ApiResource.UserPermission, f.ApiFunctionModuleKey);
                                    }
                                    else
                                    {
                                        f.ApiFunctionAccessory.UserPermission = new UserPermissionInsideFunction(new UserPermission(isEnableUserPermission: false), f.ApiFunctionModuleKey);
                                    }
                                    f.ApiFunctionAccessory.AppSettings = apiConnect.AppSettings;
                                    f.ApiFunctionAccessory.ApiRequestInfo = apiConnect.ApiRequestInfo;
                                    f.ApiFunctionAccessory.Databases = apiConnect.ApiResource.DatabasesNonTransactional;
                                    if (apiConnect.AppSettings.IsLogToDatabaseEnable())
                                    {
                                        f.ApiFunctionAccessory.LogDatabase = apiConnect.ApiResource.DatabasesNonTransactional.LogDatabase;
                                    }
                                    f.ApiFunctionAccessory.UserToken = apiConnect.ApiResource.UserToken;
                                    foreach (var kp in f.Params)
                                    {
                                        if (kp.Key.ToLower() == "@fun")
                                        {
                                            f.Params[kp.Key] = apiConnect.ApiResponse.Results[kp.Value.ToString()].Data;
                                        }
                                    }
                                    f.Invoke();
                                    apiConnect.ApiResponse.Results[f.Id] = f.ApiFunctionResult;
                                    apiConnect.ApiResponse.Messages.AddRange(f.ApiFunctionAccessory.ApiFunctionMessage.Messages);
                                    f.ApiFunctionAccessory.Dispose();
                                }
                                catch (ApiNoMessageException e)
                                {
                                    apiConnect.ApiResource.DatabasesTransactional.RollbackAll();
                                }
                                catch (ApiMessageException e)
                                {
                                    var errMsg = new ApiMessage();
                                    errMsg.Type = ApiMessageType.Error;
                                    errMsg.Code = e.Code ?? string.Empty;
                                    errMsg.Message = e.Message;
                                    errMsg.Detail = e.ToString();
                                    errMsg.FunId = currentFunId;
                                    apiConnect.ApiResponse.AddMessage(errMsg);
                                    //apiConnect.ApiResponse.Results[currentFunId].Data = e.Message; // [Question] If error, data return null or error message?
                                    apiConnect.ApiResource.DatabasesTransactional.RollbackAll();
                                }
                                catch (Exception e)
                                {
                                    var errMsg = new ApiMessage();
                                    errMsg.Type = ApiMessageType.Error;
                                    errMsg.Code = ApiMessageCode.UNKNOWN.ToString();
                                    errMsg.Message = e.Message;
                                    errMsg.Detail = e.ToString();
                                    errMsg.FunId = currentFunId;
                                    apiConnect.ApiResponse.AddMessage(errMsg);
                                    //apiConnect.ApiResponse.Results[currentFunId].Data = e.Message; // [Question] If error, data return null or error message?
                                    apiConnect.ApiResource.DatabasesTransactional.RollbackAll();
                                }
                            });
                        }
                    }
                    else if (a.IsAsync && a.IsTrans)
                    { 
                        //pending implement
                    }
                    else if (a.IsAsync && !a.IsTrans)
                    {
                        //pending implement
                    }
                });
            }

            if (apiConnect.ApiRequest.Functions != null)
            {
                Task.WaitAll(taskFunctions.ToArray());
                apiConnect.ApiRequest.Functions.ForEach(f =>
                {
                    apiConnect.ApiResponse.AddResult(id: f.Id, apiResult: f.ApiFunctionResult);
                    apiConnect.ApiResponse.AddMessages(f.ApiFunctionAccessory.ApiFunctionMessage.Messages);
                    f.ApiFunctionAccessory.Dispose();
                });
            }

            return apiConnect;
        }
        public static bool IsValidAppSecret(this ApiConnect apiConnect)
        {
            if (!apiConnect.AppSettings.IsApiKeyEnable()) { return true; }

            var appId = apiConnect.ApiRequestInfo.RequestAppId;
            var appSecret = apiConnect.ApiRequestInfo.RequestAppSecret;
            if (appId == null) { return false; }
            if (appSecret == null) { appSecret = string.Empty; }

            string? secret = null;
            try
            {
                secret = apiConnect.AppSettings.GetAppSecret(appId);
            }
            catch
            {
                return false;
            }

            if (secret == appSecret)
            {
                return true;
            }

            return false;
        }
        public static bool IsValidUserToken(this ApiConnect apiConnect, string? authorization)
        {
            if (!apiConnect.AppSettings.IsUserTokenEnable()) 
            {
                apiConnect.ApiResource.IsUserTokenEnable = false;
                return true; 
            }

            if (string.IsNullOrEmpty(authorization))
            {
                apiConnect.ApiResource.UserPermission = new UserPermission(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, null);
                return true;
            }
            else
            {
                var confiJwtList = apiConnect.AppSettings.GetConfigJwtList();
                if (confiJwtList.Count <= 0)
                {
                    return false;
                }

                bool validSuccess = false;
                foreach (var confiJwt in confiJwtList)
                {
                    try
                    {
                        if (JwtTokenHelper.IsValidToken(authorization, confiJwt))
                        {
                            validSuccess = true;
                            break;
                        }
                    }
                    catch { }
                }

                if (validSuccess)
                {
                    apiConnect.ApiResource.UserToken = JwtTokenHelper.GetObjectInToken<UserToken>(authorization);
                    apiConnect.ApiResource.UserPermission = new UserPermission(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, apiConnect.ApiResource.UserToken);
                    //apiConnect.ApiResource.UserPermission.AppendPermission(new UserPermission(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, null)); //Cancel append GUEST permission
                }
                else
                {
                    if (apiConnect.UserTokenError == null) { apiConnect.UserTokenError = new List<string>(); }
                    apiConnect.UserTokenError.Add(authorization);
                }
                return validSuccess;
            }

        }
        public static bool IsValidDevice(this ApiConnect apiConnect)
        {
            if (!apiConnect.AppSettings.IsDeviceControlEnable())
            {
                return true;
            }

            var client_ip = apiConnect.ApiRequestInfo.RequestIP;
            var client_mac = apiConnect.ApiRequestInfo.RequestMacAddress;
            var client_name = apiConnect.ApiRequestInfo.RequestMachineName;
            var client_code = apiConnect.ApiRequestInfo.RequestMachineCode;

            if (apiConnect.ApiResource.DatabasesNonTransactional == null) { return false; }
            if (apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase == null) { return false; }

            var device = DeviceTerminal.Get(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, client_ip, client_mac, client_name, client_code);

            if (device == null) { return false; }

            apiConnect.ApiRequestInfo.RequestDeviceId = device.device_id;

            if (device.is_black_list == false) { return true; }

            return false;
        }
        public static bool IsValidDeviceUser(this ApiConnect apiConnect)
        {
            if (!apiConnect.AppSettings.IsDeviceControlEnable())
            {
                return true;
            }
            if (!apiConnect.AppSettings.IsDeviceControlUserEnable())
            {
                return true;
            }
            if (!apiConnect.AppSettings.IsUserTokenEnable())
            {
                apiConnect.ApiResource.IsUserTokenEnable = false;
                return true;
            }
            if (string.IsNullOrEmpty(apiConnect.ApiRequestInfo.RequestDeviceId)) { return false; }
            if (apiConnect.ApiResource.UserPermission == null) { return false; }
            if (apiConnect.ApiResource.UserPermission.IsAdmin()) { return true; }
            if (apiConnect.ApiResource.UserPermission.IsGuest()) { return true; }
            if (apiConnect.ApiResource.UserPermission.Roles == null) { return false; }
            if (apiConnect.ApiResource.UserPermission.Roles.Count == 0) { return false; }

            var deviceRoles = DeviceTerminal.GetDeviceUserRole(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, apiConnect.ApiRequestInfo.RequestDeviceId);
            if (deviceRoles == null) { return false ; }
            if (deviceRoles.Count == 0) { return false; }

            HashSet<string> deviceRolesHash = new HashSet<string>(deviceRoles);
            foreach (var item in apiConnect.ApiResource.UserPermission.Roles)
            {
                if (deviceRolesHash.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }
        public static ApiConnect GetDevicePermission(this ApiConnect apiConnect)
        {
            if (!apiConnect.AppSettings.IsDeviceControlEnable()) { return apiConnect; }
            if (!apiConnect.AppSettings.IsDeviceControlPermissionEnable()) { return apiConnect; }
            if (string.IsNullOrEmpty(apiConnect.ApiRequestInfo.RequestDeviceId)) { return apiConnect; }
            if (apiConnect.ApiResource.UserPermission == null) { return apiConnect; }
            if (apiConnect.ApiResource.UserPermission.IsAdmin()) { return apiConnect; }
            if (apiConnect.ApiResource.UserPermission.Permissions == null) { return apiConnect; }
            if (apiConnect.ApiResource.UserPermission.Permissions.Count == 0) { return apiConnect; }

            var devicePermissions = DeviceTerminal.GetDevicePermission(apiConnect.ApiResource.DatabasesNonTransactional.AppDatabase, apiConnect.ApiRequestInfo.RequestDeviceId);
            if (devicePermissions == null) { return apiConnect; }
            if (devicePermissions.Count == 0) { return apiConnect; }

            apiConnect.ApiResource.UserPermission.IsEnableDevicePermission = true;
            apiConnect.ApiResource.UserPermission.DevicePermissions = devicePermissions;
            return apiConnect;
        }
        public static ApiConnect AppendSessionInfo(this ApiConnect apiConnect)
        {
            if (apiConnect.ApiRequest.IsGetSession == null) { return apiConnect; }
            if (apiConnect.ApiRequest.IsGetSession != true) { return apiConnect; }
            if (DatabaseCache.ConfigCache != null)
            {
                if (apiConnect.ApiResponse.Session == null) { apiConnect.ApiResponse.Session = new(); }
                apiConnect.ApiResponse.Session.Add("AppNode", DatabaseCache.AppNodeCache);
                apiConnect.ApiResponse.Session.Add("AppConfig", DatabaseCache.ConfigCache);
                apiConnect.ApiResponse.Session.Add("UserPermission", apiConnect.ApiResource.UserPermission.Permissions);
            }

            return apiConnect;
        }

    }
}
