namespace Modact
{
    public class ApiRequestInfo
    {
        public string? RequestToUrl { get; init; } = string.Empty;
        public string? RequestHttpMethod { get; init; } = string.Empty;
        public string? RequestAppId { get; init; } = string.Empty;
        public string? RequestAppName { get; init; } = string.Empty;
        public string? RequestAppSecret { get; init; } = string.Empty;
        public string? RequestAppVersion { get; init; } = string.Empty;
        public string? RequestMacAddress { get; init; } = string.Empty;
        public string? RequestMachineCode { get; init; } = string.Empty;
        public string? RequestAgent { get; init; } = string.Empty;
        public string? RequestClientType { get; init; } = string.Empty;
        public string? RequestIP { get; init; } = string.Empty;
        public string? RequestMachineName { get; init; } = string.Empty;
        public string? RequestOriginUrl { get; init; } = string.Empty;
        public string? RequestDeviceId { get; set; } = null;

    }
}
