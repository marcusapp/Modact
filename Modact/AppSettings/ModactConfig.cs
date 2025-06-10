namespace Modact
{
    public class ModactConfig
    {
        public string? AuthenCoreUrl { get; set; }
        public bool? MaintenanceMode { get; set; }
        public string? ModifyTime { get; set; }

        public ApiKeyOption? ApiKeyOption { get; set; }
        public DateTimeToStringFormatClass? DateTimeToStringFormatOption { get; set; }
        public LogOption? LogOption { get; set; }
        public NotifyOption? NotifyOption { get; set; }
        public TokenOption? TokenOption { get; set; }
    }

    public class DateTimeToStringFormatClass
    {
        public string? Date { get; set; } = "yyyy-MM-dd";
        public string? Time { get; set; } = "HH:mm:ss";
        public string? DateTime { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public string? DateTimeMillisecond { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";
    }

    public enum DateTimeToStringFormat
    {
        Date = 0,
        Time = 1,
        DateTime = 2,
        DateTimeMillisecond = 3,
    }

    public class ApiKeyOption
    {
        public bool? ApiKeyEnable { get; set; }
        public Dictionary<string, string>? ApiKeyList { get; set; }
    }

    public class LogOption
    {
        public LogModactOption? LogModactOption { get; set; }
        public LogModactApiOption? LogModactApiOption { get; set; }
    }
    public class LogModactOption
    {
        public bool? LogToFile { get; set; }
        public string? LogToFilePath { get; set; }
    }
    public class LogModactApiOption
    {
        public bool? LogToFileEnable { get; set; }
        public string? LogToFilePath { get; set; }
        public bool? LogToDatabaseEnable { get; set; }
        public bool? LogToDatabaseFailThenLogToFile { get; set; }
        public bool? LogOnlyError { get; set; }
        public bool? LogWithInputData { get; set; }
        public bool? LogWithOutputData { get; set; }
        public Dictionary<string, int[]>? LogMaskParamsIndex { get; set; }
    }

    public class TokenOption
    {
        public bool? UserTokenEnable { get; set; }
        public Dictionary<string, ConfigJwt>? Jwt { get; set; }
    }
    public class ConfigJwt
    {
        public string? Audience { get; set; }
        public string? Issuer { get; set; }
        public int? ExpireMinute { get; set; }
        public bool? IsAsymmetric { get; set; }
        public string? Secret { get; set; }
        public string? PrivateKey { get; set; }
        public string? PublicKey { get; set; }
    }

    public class NotifyOption
    {
        public string? Message { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }
}
