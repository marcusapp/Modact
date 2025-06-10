using System.Data.SqlClient;

namespace Modact
{
    public class DatabaseConnectionConfig
    {
        private string _connectionString;
        public DatabaseType Type { get; set; }
        public string? EncryptString { get; set; }
        public string? EncryptString2 { get; set; }
        public string? EncryptString3 { get; set; }
        public string? EncryptString4 { get; set; }
        public string? EncryptString5 { get; set; }
        public string? EncryptString6 { get; set; }
        public string? EncryptString7 { get; set; }
        public string? EncryptString8 { get; set; }
        public string? EncryptString9 { get; set; }

        public string ConnectionString { 
            get 
            {
                return _connectionString;
            }
            set
            {
                _connectionString = DecryptConnectionString(value);
            }
        }

        private string DecryptConnectionString(string connectionString)
        {
            connectionString = connectionString.Replace("{EncryptString}", GetPlainString(EncryptString));
            connectionString = connectionString.Replace("{EncryptString2}", GetPlainString(EncryptString2));
            connectionString = connectionString.Replace("{EncryptString3}", GetPlainString(EncryptString3));
            connectionString = connectionString.Replace("{EncryptString4}", GetPlainString(EncryptString4));
            connectionString = connectionString.Replace("{EncryptString5}", GetPlainString(EncryptString5));
            connectionString = connectionString.Replace("{EncryptString6}", GetPlainString(EncryptString6));
            connectionString = connectionString.Replace("{EncryptString7}", GetPlainString(EncryptString7));
            connectionString = connectionString.Replace("{EncryptString8}", GetPlainString(EncryptString8));
            connectionString = connectionString.Replace("{EncryptString9}", GetPlainString(EncryptString9));
            return connectionString;
        }

        private string GetPlainString(string? encryptString)
        {
            if (string.IsNullOrEmpty(encryptString)) { return string.Empty; }
            try
            {
                return EncryptSymmetric.DecryptToString(encryptString, "DatabasePassword");
            }
            catch
            {
                return encryptString;
            }
        }
    }

}
