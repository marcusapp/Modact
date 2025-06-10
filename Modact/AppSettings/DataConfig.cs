namespace Modact
{
    public class DataConfig
    {
        public string? AppDatabase { get; set; }
        public string? LogDatabase { get; set; }
        public Dictionary<string, DatabaseConnectionConfig>? Database { get; set; }

    }

}
