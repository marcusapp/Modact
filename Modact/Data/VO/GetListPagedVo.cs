using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modact.Data.VO
{
    public class GetListPagedVo<TEntity> where TEntity : IDto
    {
        public int PageNumber { get; set; } = 0;
        public int RowsPerPage { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int TotalRows { get; set; } = 0;
        public IEnumerable<TEntity>? DataRows { get; set; }
    }

    public class GetListPagedVo
    {
        public int PageNumber { get; set; } = 0;
        public int RowsPerPage { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int TotalRows { get; set; } = 0;
        public IEnumerable<object>? DataRows { get; set; }
    }
}
