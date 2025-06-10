namespace Modact
{

    [Serializable]
    public class ApiRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public DateTime? RequestTime { get; set; }
        public RequestInfo? RequestInfo { get; set; }
        public bool? IsGetSession { get; set; }

        /// <summary>
        /// Action is some functions set to call and run sequentially in same transaction to commit.
        /// </summary>
        public List<ApiAction> Actions { get; set; }

        //public List<ApiAction> AsyncActions { get; set; } //pending implement

        /// <summary>
        /// Function - async processing to each function and both running non-transaction mode.
        /// </summary>
        public List<ApiFunction> Functions { get; set; }


        public ApiRequest() { }

    }

    [Serializable]
    public class RequestInfo
    {
        public string? AppId { get; set; }
        public string? AppName { get; set; }
        public string? AppSecret { get; set; }
        public string? AppVersion { get; set; }
        public string? MacAddress { get; set; }
        public string? MachineCode { get; set; }
        public string? MachineName { get; set; }
    }
}