using Dapper.SimpleCRUD;

namespace Modact.Data.DAL
{
    public class Dao<TEntity> where TEntity : IDto
    {
        protected DbHelper db { get; init; }

        public Dao(DbHelper dbHelper)
        {
            db = dbHelper;
        }

        public virtual void Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            db.Connection().Insert<TEntity>(entity, db.Transaction());
        }
        public virtual void Add(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            foreach (var entity in entities)
            {
                Add(entity);
            }
        }
        public virtual int? Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            return db.Connection().Update(entity, db.Transaction());
        }
        public virtual int? Update(IEnumerable<TEntity> entities)
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
        public virtual TEntity Get(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(TEntity));
            }

            return db.Connection().Get<TEntity>(id, db.Transaction());
        }
        public virtual IEnumerable<TEntity> GetList()
        {
            return this.GetList(null);
        }
        public virtual IEnumerable<TEntity> GetList(object? whereConditions)
        {
            return db.Connection().GetList<TEntity>(whereConditions, db.Transaction());
        }

    }
}
