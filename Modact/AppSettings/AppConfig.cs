namespace Modact
{
    public class AppConfig
    {
        public string? AppId { get; set; }
        public string? AppNodeId { get; set; }
        public Dictionary<string, string>? Module { get; set; }
        public string? AppInfoModule { get; set; } 
        public string? AppInfoNamespace { get; set; }
    }
}
