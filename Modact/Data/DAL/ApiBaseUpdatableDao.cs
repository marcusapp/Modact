using Dapper.SimpleCRUD;

namespace Modact.Data.DAL
{
    public abstract class ApiBaseUpdatableDao<TEntity> : ApiBaseVoidableDao<TEntity>, IUpdatableDao<TEntity> where TEntity : IUpdatableDto
    {
        [Obsolete("This property is obsoleted, use appDB instead.")]
        protected DbHelper db { get; init; }

        public ApiBaseUpdatableDao(ApiFunctionAccessory apiFunctionAccessory) : base(apiFunctionAccessory)
        {
            db = apiFunctionAccessory.Databases.AppDatabase;
        }

        public virtual int Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            DateTime now = DateTime.Now;
            entity.modify_time = now;
            entity.modify_time_utc = now.ToUniversalTime();
            if (ApiFunctionAccessory.UserToken != null)
            {
                entity.modify_user_id = ApiFunctionAccessory.UserToken.UserId;
                entity.modify_user_name = ApiFunctionAccessory.UserToken.UserName;
            }
            if (ApiFunctionAccessory.ApiConnectId != null)
            {
                entity.modify_log_id = ApiFunctionAccessory.ApiConnectId;
            }

            return appDB.Connection().UpdateWithoutNull(entity, appDB.Transaction());
        }
        public virtual int Update(IEnumerable<TEntity> entities, TEntity baseEntity)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            int affectedRows = 0;
            foreach (var entity in entities)
            {
                int? rows = Update(entity);
                if (rows != null) { affectedRows += (int)rows; }
            }
            return affectedRows;
        }
    }
}
