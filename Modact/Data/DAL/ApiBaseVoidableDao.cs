using Dapper.SimpleCRUD;

namespace Modact.Data.DAL
{
    public abstract class ApiBaseVoidableDao<TEntity> : ApiBaseCreatableDao<TEntity>, IVoidableDao<TEntity> where TEntity : IVoidableDto
    {
        [Obsolete("This property is obsoleted, use appDB instead.")]
        protected DbHelper db { get; init; }

        public ApiBaseVoidableDao(ApiFunctionAccessory apiFunctionAccessory) : base(apiFunctionAccessory)
        {
            db = apiFunctionAccessory.Databases.AppDatabase;
        }

        public virtual int Void(string id)
        {
            return Void(id, string.Empty);
        }
        public virtual int Void(string id, string? remark)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            if (remark == null)
            {
                remark = string.Empty;
            }

            var entity = Get(id);

            DateTime now = DateTime.Now;
            entity.is_void = true;
            entity.void_switch_time = now;
            entity.void_switch_time_utc = now.ToUniversalTime();
            entity.void_remark = remark;
            if (ApiFunctionAccessory.UserToken != null)
            {
                entity.void_switch_user_id = ApiFunctionAccessory.UserToken.UserId;
                entity.void_switch_user_name = ApiFunctionAccessory.UserToken.UserName;
            }
            if (ApiFunctionAccessory.ApiConnectId != null)
            {
                entity.void_switch_log_id = ApiFunctionAccessory.ApiConnectId;
            }
            return appDB.Connection().Update(entity, appDB.Transaction());
        }
        public virtual int Void(IEnumerable<string> idList, string? remark, string? column)
        {
            if (idList == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            switch (column.ToLower())
            {
                case "id":
                    var a = "id";
                    break;
                case "create_log_id":
                    var b = "create_log_id";
                    break;
            }

            int count = 0;
            foreach (var id in idList)
            {
                count += Void(id, remark);
            }

            return count;
        }
        public virtual int VoidUndo(string id)
        {
            return VoidUndo(id, string.Empty);
        }
        public virtual int VoidUndo(string id, string? remark)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            if (remark == null)
            {
                remark = string.Empty;
            }

            var entity = Get(id);

            DateTime now = DateTime.Now;
            entity.is_void = false;
            entity.void_switch_time = now;
            entity.void_switch_time_utc = now.ToUniversalTime();
            entity.void_remark = remark;
            if (ApiFunctionAccessory.UserToken != null)
            {
                entity.void_switch_user_id = ApiFunctionAccessory.UserToken.UserId;
                entity.void_switch_user_name = ApiFunctionAccessory.UserToken.UserName;
            }
            if (ApiFunctionAccessory.ApiConnectId != null)
            {
                entity.void_switch_log_id = ApiFunctionAccessory.ApiConnectId;
            }
            return appDB.Connection().Update(entity, appDB.Transaction());
        }
        public virtual int VoidUndo(IEnumerable<string> idList, string? remark, string? column)
        {
            if (idList == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            switch (column.ToLower())
            {
                case "id":
                    var a = "id";
                    break;
                case "create_log_id":
                    var b = "create_log_id";
                    break;
            }

            int count = 0;
            foreach (var id in idList)
            {
                count += VoidUndo(id, remark);
            }

            return count;
        }
    }
}
