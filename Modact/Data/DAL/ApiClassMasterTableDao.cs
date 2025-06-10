using Dapper.SimpleCRUD;
using static Dapper.SqlMapper;

namespace Modact.Data.DAL
{
    public abstract class ApiClassMasterTableDao<TEntity> : ApiBaseUpdatableDao<TEntity> where TEntity : IMasterTableDto
    {
        [Obsolete("This property is obsoleted, use appDB instead.")]
        protected DbHelper db { get; init; }

        public ApiClassMasterTableDao(ApiFunctionAccessory apiFunctionAccessory) : base(apiFunctionAccessory)
        {
            db = apiFunctionAccessory.Databases.AppDatabase;
        }

    }
}
