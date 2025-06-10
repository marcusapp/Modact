using Dapper.SimpleCRUD;

namespace Modact.Data.DAL
{
    public abstract class ApiClassTransactionTableDao<TEntity> : ApiBaseUpdatableDao<TEntity> where TEntity : ITransactionDto
    {
        [Obsolete("This property is obsoleted, use appDB instead.")]
        protected DbHelper db { get; init; }

        public ApiClassTransactionTableDao(ApiFunctionAccessory apiFunctionAccessory) : base(apiFunctionAccessory)
        {
            db = apiFunctionAccessory.Databases.AppDatabase;
        }

    }
}
