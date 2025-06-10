namespace Modact
{
    public class ApiClass : IDisposable
    {
        private bool _disposed = false;
        protected ApiFunctionAccessory ApiFunctionAccessory { get; set; }
        protected DbHelper appDB { get; init; }
        protected DbHelper logDB { get; init; }

        public ApiClass(ApiFunctionAccessory ApiFunctionAccessory)
        {
            this.ApiFunctionAccessory = ApiFunctionAccessory;
            this.appDB = ApiFunctionAccessory.Databases.AppDatabase;
            this.logDB = ApiFunctionAccessory.Databases.LogDatabase;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) { return; }

            this.ApiFunctionAccessory = null;

            _disposed = true;
        }

        ~ApiClass()
        {
            Dispose(false);
        }
    }
}