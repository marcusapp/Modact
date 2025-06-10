namespace Modact
{
    [Serializable]
    public class ApiMessage
    {
        public ApiMessageType Type { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public string Detail { get; set; }

        public string FunId { get; set; }

        public ApiMessage()
        {

        }

        public ApiMessage(ApiMessageType type)
        {
            Type = type;
        }
    }

    public enum ApiMessageType
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
        Success = 6
    }

    public enum ApiMessageCode
    {
        OK = 0,
        CANCELLED = 1,
        UNKNOWN = 2,
        INVALID_ARGUMENT = 3,
        DEADLINE_EXCEEDED = 4,
        NOT_FOUND = 5,
        ALREADY_EXISTS = 6,
        PERMISSION_DENIED = 7,
        RESOURCE_EXHAUSTED = 8,
        FAILED_PRECONDITION = 9,
        ABORTED = 10,
        OUT_OF_RANGE = 11,
        UNIMPLEMENTED = 12,
        INTERNAL = 13,
        UNAVAILABLE = 14,
        DATA_LOSS = 15,
        UNAUTHENTICATED = 16
    }
}