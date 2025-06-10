namespace Modact
{
    public class ApiResource
    {
        public DbHelperList DatabasesTransactional { get; set; }
        public DbHelperList DatabasesNonTransactional { get; set; }
        public bool IsUserTokenEnable { get; set; } = true;
        public UserToken UserToken { get; set; }
        public UserPermission UserPermission { get; set; }

        public ApiResource() { }

        public void Dispose()
        {
            if ( this.DatabasesTransactional != null)
            {
                this.DatabasesTransactional.DisposeAll();
            }
            if (this.DatabasesNonTransactional != null)
            {
                this.DatabasesNonTransactional.DisposeAll();
            }
        }
    }
}
