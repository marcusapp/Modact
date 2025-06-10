namespace Modact
{

    [Serializable]
    public class UserToken
    {
        public string? AuthId { get; set; }
        public DateTime? AuthTime { get; set; }
        public string? TokenId { get; set; }
        public UserTokenType TokenType { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public List<string>? UserGroup { get; set; }
        public Dictionary<string, object>? UserMeta { get; set; }
        public string? DeviceType { get; set; }
        public string? IP { get; set; }
        public string? MacAddress { get; set; }
        public string? MachineId { get; set; }
        public string? MachineName { get; set; }

        public UserToken()
        {

        }
    }

    public enum UserTokenType
    {
        Access = 0,
        Refresh = 1,
    }

}