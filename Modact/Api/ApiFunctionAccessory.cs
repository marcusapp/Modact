using System.Text.Json;

namespace Modact
{
    public class ApiFunctionAccessory
    {
        public string ApiFunctionId { get; init; }
        public string ApiFunctionName { get; init; }
        public string ApiConnectId { get; set; }
        public DateTime ApiConnectStartTime { get; set; }
        public ApiFunctionMessage ApiFunctionMessage { get; set; } = new ApiFunctionMessage();

        [JsonIgnore]
        public ApiRequestInfo ApiRequestInfo { get; set; }
        [JsonIgnore]
        public AppSettings AppSettings { get; set; }
        [JsonIgnore]
        public DbHelperList? Databases { get; set; }
        [JsonIgnore]
        public DbHelper? LogDatabase { get; set; }
        [JsonIgnore]
        public UserToken? UserToken { get; set; }
        [JsonIgnore]
        public UserPermissionInsideFunction? UserPermission { get; set; }
        [JsonIgnore]
        public JsonSerializerOptions? JsonSerializerOptions { get; set; }

        public ApiFunctionAccessory()
        {
            this.ApiFunctionId = string.Empty;
            this.ApiFunctionName = string.Empty;
        }

        public ApiFunctionAccessory(string apiFunctionId, string apiFunctionName)
        {
            this.ApiFunctionId = apiFunctionId;
            this.ApiFunctionName = apiFunctionName;
        }

        public ApiMessage AddMessage(ApiMessageType type, string message, ApiMessageCode? code = null)
        {
            var msg = AddMessage(type, message, code.ToString());
            return msg;
        }

        public ApiMessage AddMessage(ApiMessageType type, string message, string? code)
        {
            var msg = new ApiMessage();
            msg.Type = type;
            msg.Code = code;
            msg.Message = message;
            msg.FunId = this.ApiFunctionId;
            ApiFunctionMessage.Messages.Add(msg);
            return msg;
        }

        public void Dispose()
        {
            this.Databases = null;
        }
    }
}