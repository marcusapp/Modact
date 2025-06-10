using Dapper.SimpleCRUD;

namespace Modact.Data.DAL
{
    public abstract class ApiClassRelationalTableDao<TEntity> : ApiBaseVoidableDao<TEntity> where TEntity : IRelationalTableDto
    {
        [Obsolete("This property is obsoleted, use appDB instead.")]
        protected DbHelper db { get; init; }

        public ApiClassRelationalTableDao(ApiFunctionAccessory apiFunctionAccessory) : base(apiFunctionAccessory)
        {
            db = apiFunctionAccessory.Databases.AppDatabase;
        }

    }
}
