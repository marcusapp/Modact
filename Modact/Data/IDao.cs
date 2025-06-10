using Modact.Data.VO;

namespace Modact
{
    public interface ICreatableDao<TEntity> where TEntity : IDto
    {
        object Add(TEntity entity);
        IEnumerable<object> Add(IEnumerable<TEntity> entities, TEntity baseEntity);

        TEntity? Get(string id);
        IEnumerable<TEntity>? GetList();
        IEnumerable<TEntity>? GetList(bool isActiveList);
        IEnumerable<TEntity>? GetList(object? whereParams, string whereQuery);
        GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage);
        GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage, string orderby);
        GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage, string orderby, object whereConditions);
        GetListPagedVo<TEntity>? GetListPaged(int pageNumber, int rowsPerPage, string orderby, object? whereParams, string whereQuery);

    }

    public interface IVoidableDao<TEntity> : ICreatableDao<TEntity> where TEntity : IDto
    {
        public int Void(string id);
        public int Void(string id, string? remark);
        public int Void(IEnumerable<string> idList, string? remark, string? column);
        public int VoidUndo(string id);
        public int VoidUndo(string id, string? remark);
        public int VoidUndo(IEnumerable<string> idList, string? remark, string? column);
    }

    public interface IUpdatableDao<TEntity> : ICreatableDao<TEntity> where TEntity : IDto
    {
        int Update(TEntity entity);
        int Update(IEnumerable<TEntity> entities, TEntity baseEntity);
    }
}
