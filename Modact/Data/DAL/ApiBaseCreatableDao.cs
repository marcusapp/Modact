using Dapper.SimpleCRUD;
using Modact.Data.VO;
using System.Collections.Generic;
using System.Dynamic;

namespace Modact.Data.DAL
{
    public abstract class ApiBaseCreatableDao<TEntity> : ApiClass, ICreatableDao<TEntity> where TEntity : ICreatableDto
    {
        [Obsolete("This property is obsoleted, use appDB instead.")]
        protected DbHelper db { get; init; }

        public ApiBaseCreatableDao(ApiFunctionAccessory apiFunctionAccessory) : base(apiFunctionAccessory)
        {
            db = apiFunctionAccessory.Databases.AppDatabase;
        }

        public virtual object Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            DateTime now = DateTime.Now;
            entity.create_time = now;
            entity.create_time_utc = now.ToUniversalTime();
            if (ApiFunctionAccessory.UserToken != null)
            {
                entity.create_user_id = ApiFunctionAccessory.UserToken.UserId;
                entity.create_user_name = ApiFunctionAccessory.UserToken.UserName;
            }
            if (ApiFunctionAccessory.ApiConnectId != null)
            {
                entity.create_log_id = ApiFunctionAccessory.ApiConnectId;
            }

            return appDB.Connection().Insert<TEntity>(entity, appDB.Transaction());
        }
        public virtual IEnumerable<object> Add(IEnumerable<TEntity> entities, TEntity baseEntity)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            var idList = new List<object?>();

            foreach (var entity in entities)
            {
                idList.Add(Add(entity));
            }

            return idList;
        }

        public virtual TEntity? Get(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            return appDB.Connection().Get<TEntity>(id, appDB.Transaction());
        }
        [Permission("Get")]
        public virtual IEnumerable<TEntity>? GetList()
        {
            return this.GetList(null, "");
        }
        [Permission("Get")]
        public virtual IEnumerable<TEntity>? GetList(bool isActiveList)
        {
            return appDB.Connection().GetList<TEntity>(new { is_void = !isActiveList }, appDB.Transaction());
        }
        [Permission("Get")]
        public virtual IEnumerable<TEntity>? GetList(object? whereParams, string whereQuery)
        {
            if (string.IsNullOrEmpty(whereQuery))
            {
                return appDB.Connection().GetList<TEntity>(whereParams, appDB.Transaction());
            }
            return appDB.Connection().GetList<TEntity>(whereQuery, whereParams, appDB.Transaction());
        }
        [Permission("Get")]
        public virtual GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage)
        {
            var vo = new GetListPagedVo<TEntity>();
            if (rowsPerPage <= 0)
            {
                vo.PageNumber = 1;
                vo.TotalRows = appDB.Connection().RecordCount<TEntity>("", null, appDB.Transaction());
                vo.RowsPerPage = vo.TotalRows;
                vo.TotalPages = 1;
            }
            else
            {
                vo.PageNumber = pageNumber;
                vo.RowsPerPage = rowsPerPage;
                vo.TotalRows = appDB.Connection().RecordCount<TEntity>("", null, appDB.Transaction());
                vo.TotalPages = (int)Math.Ceiling((decimal)vo.TotalRows / vo.RowsPerPage);
            }
            vo.DataRows = appDB.Connection().GetListPaged<TEntity>(vo.PageNumber, vo.RowsPerPage, string.Empty, string.Empty, null, appDB.Transaction());
            return vo;
        }
        [Permission("Get")]
        public virtual GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage, string orderby)
        {
            var vo = new GetListPagedVo<TEntity>();
            if (rowsPerPage <= 0)
            {
                vo.PageNumber = 1;
                vo.TotalRows = appDB.Connection().RecordCount<TEntity>("", null, appDB.Transaction());
                vo.RowsPerPage = vo.TotalRows;
                vo.TotalPages = 1;
            }
            else
            {
                vo.PageNumber = pageNumber;
                vo.RowsPerPage = rowsPerPage;
                vo.TotalRows = appDB.Connection().RecordCount<TEntity>("", null, appDB.Transaction());
                vo.TotalPages = (int)Math.Ceiling((decimal)vo.TotalRows / vo.RowsPerPage);
            }
            vo.DataRows = appDB.Connection().GetListPaged<TEntity>(vo.PageNumber, vo.RowsPerPage, string.Empty, orderby, null, appDB.Transaction());
            return vo;
        }
        [Permission("Get")]
        public virtual GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage, string orderby, object whereConditions)
        {
            throw new NotImplementedException();
        }
        [Permission("Get")]
        public virtual GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage, string orderby, object? whereParams, string whereQuery)
        {
            var vo = new GetListPagedVo<TEntity>();
            if (rowsPerPage <= 0)
            {
                vo.PageNumber = 1;
                vo.TotalRows = appDB.Connection().RecordCount<TEntity>("", null, appDB.Transaction());
                vo.RowsPerPage = vo.TotalRows;
                vo.TotalPages = 1;
            }
            else
            {                
                vo.PageNumber = pageNumber;
                vo.RowsPerPage = rowsPerPage;
                vo.TotalRows = appDB.Connection().RecordCount<TEntity>(whereQuery, whereParams, appDB.Transaction());
                vo.TotalPages = (int)Math.Ceiling((decimal)vo.TotalRows / vo.RowsPerPage);
            }
            vo.DataRows = appDB.Connection().GetListPaged<TEntity>(vo.PageNumber, vo.RowsPerPage, whereQuery, orderby, whereParams, appDB.Transaction());
            return vo;
        }
    }
}
