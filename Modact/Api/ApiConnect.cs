using Modact.Data.Models;
using System.Diagnostics;
using System.Timers;
using Dapper.SimpleCRUD;
using System.Net;
using System.Net.NetworkInformation;

namespace Modact
{
    public class ApiConnect
    {
        public string Id { get; }
        public string ParentApiConnectId { get; set; }
        public bool Success { get; set; }
        public List<string> FatalMessage { get; set; }
        public bool IsSendRequest { get; set; }
        public ApiRequestInfo ApiRequestInfo { get; set; }
        public string? ResponseAppId { get; set; }
        public string? ResponseAppVersion { get; set; }
        public DateTime StartTime { get; }
        public double ProcessSecond { get; set; }
        public double StartCpuUsage { get; }
        public double EndCpuUsage { get; set; }
        public double StartMemoryUsageMB { get; }
        public double EndMemoryUsageMB { get; set; }
        public string MachineName { get; }

        public ApiRequest ApiRequest { get; set; }
        public string? RequestBody { get; set; }
        public ApiResponse ApiResponse { get; set; }
        public string? ResponseBody { get; set; }

        [JsonIgnore]
        public AppSettings AppSettings { get; set; }
        [JsonIgnore]
        public ApiResource ApiResource { get; set; }
        [JsonIgnore]
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
        [JsonIgnore]
        public List<string> UserTokenError { get; set; }

        public ApiConnect(AppSettings appSettings)
        {
            this.Id = Ulid.NewUlid().ToString();
            this.Success = false;
            this.StartTime = DateTime.Now;
            this.StartCpuUsage = 0;
            this.StartMemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
            this.MachineName = AppInfo.MachineName;
            this.AppSettings = appSettings;
            this.JsonSerializerOptions = appSettings.GetJsonSerializerOptions();
        }

        public ApiConnect ApiRequestReceiveInit(ApiRequest? apiRequest)
        {
            this.ParentApiConnectId = string.Empty;
            this.IsSendRequest = false;

            if (this.AppSettings != null)
            {
                this.ResponseAppId = this.AppSettings.AppConfig.AppId;
                this.ResponseAppVersion = this.AppSettings.AppVersionString;
                if (this.AppSettings.DataConfig != null)
                {
                    this.ApiResource = new();
                    if (this.AppSettings.DataConfig.Database != null)
                    {
                        this.ApiResource.DatabasesTransactional = new(this.AppSettings.DataConfig.Database, true, this.AppSettings.DataConfig.AppDatabase, this.AppSettings.DataConfig.LogDatabase);
                        this.ApiResource.DatabasesNonTransactional = new(this.AppSettings.DataConfig.Database, false, this.AppSettings.DataConfig.AppDatabase, this.AppSettings.DataConfig.LogDatabase);
                    }
                }
            }
            this.ApiRequest = apiRequest;
            this.ApiResponse = new(Ulid.NewUlid().ToString(), apiRequest.RequestId);

            return this;
        }
        public ApiConnect ApiRequestSendOutInit(ApiRequest apiRequest, string requestToUrl, string parentApiConnectId = "")
        {
            this.ParentApiConnectId = parentApiConnectId;
            this.IsSendRequest = true;
            this.ApiRequestInfo = new ApiRequestInfo
            {
                RequestToUrl = requestToUrl,
                RequestHttpMethod = "POST",
                RequestAgent = AppInfo.AppName + " " + AppInfo.AppVersionString,
                RequestMachineName = AppInfo.MachineName,
                RequestClientType = "API",
                RequestMacAddress = AppInfo.GetAllMacAddress(NetworkInterfaceType.Ethernet).FirstOrDefault(),
                RequestAppId = this.AppSettings.AppConfig.AppId,
                RequestAppName = AppInfo.AppName,
                RequestAppVersion = AppInfo.AppVersionString
            };
            this.ResponseAppId = string.Empty;
            this.ResponseAppVersion = string.Empty;

            this.ApiRequest = apiRequest;
            this.ApiRequest.RequestId = Ulid.NewUlid().ToString();

            if (this.AppSettings.DataConfig != null)
            {
                this.ApiResource = new();
                if (this.AppSettings.DataConfig.Database != null)
                {
                    this.ApiResource.DatabasesTransactional = new(this.AppSettings.DataConfig.Database, true, this.AppSettings.DataConfig.AppDatabase, this.AppSettings.DataConfig.LogDatabase);
                    this.ApiResource.DatabasesNonTransactional = new(this.AppSettings.DataConfig.Database, false, this.AppSettings.DataConfig.AppDatabase, this.AppSettings.DataConfig.LogDatabase);
                }
            }
            return this;
        }

        public async Task<ApiResponse?> HttpPostAsync(string? authorization = null)
        {
            if (!IsSendRequest) { return null; }
            if (this.ApiRequest == null) { return null; }
            if (string.IsNullOrEmpty(this.ApiRequestInfo.RequestToUrl)) { return null; }

            string requestJson = JsonSerializer.Serialize(this.ApiRequest);
            HttpClient httpClient = HttpClientHelper.GetClient(authorization, this.ApiRequestInfo);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpResponseMessage response = await httpClient.PostAsync(this.ApiRequestInfo.RequestToUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                ApiResponse apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, this.AppSettings.GetJsonSerializerOptions());
                this.Success = true;
                this.FinallyAction();
                return apiResponse;
            }

            return null;
        }

        public void FatalMessageAdd(string message)
        {
            this.FatalMessage = this.FatalMessage ?? new List<string>();
            this.FatalMessage.Add(message);
        }

        public ApiConnect FinallyAction()
        {
            DateTime nowTime = DateTime.Now;
            this.ProcessSecond = (nowTime - this.StartTime).TotalSeconds;
            this.EndMemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

            if (this.ApiRequest.IsGetSession ?? false)
            {
                if (this.ApiResource != null) { this.ApiResource.Dispose(); }
                return this;
            }

            try
            {
                if (this.ApiResource.UserToken != null && !string.IsNullOrEmpty(AppSettings.DataConfig.AppDatabase))
                {
                    LogTokenToAppDatabase(ApiResource.DatabasesNonTransactional.AppDatabase);
                }
            }
            catch (Exception ex)
            {
                FatalMessageAdd("[Exception]LogTokenToAppDatabase:" + ex.ToString());
                this.LogToFile("[Exception]LogTokenToAppDatabase:" + ex.ToString(), LogTypeShort.ERR);
            }

            try
            {
                if (AppSettings.IsLogToDatabaseEnable() == true)
                {
                    LogToDatabase();
                }
            }
            catch (Exception ex) 
            {
                FatalMessageAdd("[Exception]LogToDatabase:" + ex.ToString());
                this.LogToFile("[Exception]LogToDatabase:" + ex.ToString(), LogTypeShort.ERR);
            }

            try
            {
                if (AppSettings.IsLogToFileEnable() == true)
                {
                    LogToFile();
                }
            }
            catch (Exception ex)
            {
                FatalMessageAdd("[Exception]LogToFile:" + ex.ToString());
            }
            

            if (this.ApiResource != null)
            {
                this.ApiResource.Dispose();
            }
            return this;
        }

        public void FinallySession()
        {
            DateTime nowTime = DateTime.Now;
            this.ProcessSecond = (nowTime - this.StartTime).TotalSeconds;
            this.EndMemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

            if (this.ApiResource != null)
            {
                this.ApiResource.Dispose();
            }
        }

        #region LogtoFile
        private static LogToFile? _globalLogFile;
        private static System.Timers.Timer? _globalLogTimer;
        private static Queue<LogApiConnectMessage>? _globalLogQueue;
        private void LogToFile()
        {
            if (AppSettings.IsLogOnlyError() == true && Success && this.FatalMessage == null) { return; }

            if (_globalLogFile == null) { _globalLogFile = new LogToFile(AppSettings.GetModactConfigValue<string>("ModactConfig:LogOption:LogModactApiOption:LogToFilePath"), "modact-apiconnect", true); }
            var log = new LogApiConnectMessage();
            log.LogTime = DateTime.Now;

            if (Success) {
                log.LogType = LogTypeShort.INF; }
            else {
                log.LogType = LogTypeShort.ERR; } 

            log.Message = JsonSerializer.Serialize(this, JsonSerializerOptions);

            if (_globalLogQueue == null) { _globalLogQueue = new Queue<LogApiConnectMessage>(); }
            _globalLogQueue.Enqueue(log);

            if (_globalLogTimer == null)
            {
                _globalLogTimer = new System.Timers.Timer();
                _globalLogTimer.Interval = 1000;
                _globalLogTimer.AutoReset = true;
                _globalLogTimer.Elapsed += new System.Timers.ElapsedEventHandler(LogToFileTimerElapsed);
                _globalLogTimer.Start();
            }
        }

        public void LogToFile(object obj, LogTypeShort logTypeShort)
        {

            if (_globalLogFile == null) { _globalLogFile = new LogToFile(AppSettings.GetModactConfigValue<string>("ModactConfig:LogOption:LogModactApiOption:LogToFilePath"), "modact-apiconnect", true); }
            var log = new LogApiConnectMessage();
            log.LogTime = DateTime.Now;
            log.LogType = logTypeShort;
            log.Message = JsonSerializer.Serialize(obj, JsonSerializerOptions);

            if (_globalLogQueue == null) { _globalLogQueue = new Queue<LogApiConnectMessage>(); }
            _globalLogQueue.Enqueue(log);

            if (_globalLogTimer == null)
            {
                _globalLogTimer = new System.Timers.Timer();
                _globalLogTimer.Interval = 1000;
                _globalLogTimer.AutoReset = true;
                _globalLogTimer.Elapsed += new System.Timers.ElapsedEventHandler(LogToFileTimerElapsed);
                _globalLogTimer.Start();
            }
        }
        private static void LogToFileTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_globalLogQueue == null) { return; }

            if (_globalLogQueue.Count == 0) { return; }

            if (_globalLogTimer != null)
            {
                _globalLogTimer.Stop();
            }            

            LogApiConnectMessage log;
            while (_globalLogQueue.TryDequeue(out log))
            {
                if (_globalLogFile != null && log != null)
                {
                    _globalLogFile.AddLine(log.LogType, log.Message, log.LogTime);
                }
            }

            if (_globalLogTimer != null)
            {
                _globalLogTimer.Start();
            }
            
        }
        #endregion

        #region LogtoDatabase
        private void LogToDatabase()
        {
            if (AppSettings.IsLogToDatabaseEnable() != true) { return; }

            if (this.ApiResource.DatabasesNonTransactional == null) { return; }

            if (this.ApiResource.DatabasesNonTransactional.LogDatabase == null) { return; }

            var logToDb = new LogToDatabase(this.ApiResource.DatabasesNonTransactional.LogDatabase, this);

            logToDb.WriteToDatabase();
            logToDb.Dispose();
        }

        public void LogTokenToAppDatabase(DbHelper appDB, DateTime? seesionTime = null)
        {
            if (this.ApiResource == null) { return; }
            if (this.ApiResource.UserToken == null) { return; }

            if (appDB == null) { return; }

            var logToken = new DTO_modm_token_user();
            logToken.user_id = this.ApiResource.UserToken.UserId;
            logToken.user_name = this.ApiResource.UserToken.UserName;
            logToken.user_group = this.ApiResource.UserToken.UserGroup.ToStringSepartor();
            logToken.last_auth_id = this.ApiResource.UserToken.AuthId;
            logToken.last_auth_time = this.ApiResource.UserToken.AuthTime;
            logToken.last_token_id = this.ApiResource.UserToken.TokenId;
            logToken.last_log_id = this.Id;
            if (seesionTime != null)
            {
                logToken.last_session_time = seesionTime;
            }
            int record = appDB.Connection().RecordCount<DTO_modm_token_user>(new { user_id = this.ApiResource.UserToken.UserId });
            if (record > 0)
            {
                logToken.modify_time = DateTime.Now;
                appDB.Connection().UpdateWithoutNull(logToken);
            }
            else
            {
                logToken.create_time = DateTime.Now;
                appDB.Connection().Insert(logToken);
            }

        }

        #endregion
    }

    public class LogApiConnectMessage
    {
        public DateTime LogTime { get; set; }
        public LogTypeShort LogType { get; set; }
        public string Message { get; set; }
    }
}
