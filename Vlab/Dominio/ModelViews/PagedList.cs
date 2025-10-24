using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.ModelViews
{
    public class PagedList<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PagedList(List<T> items, int totalCount, int page)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
        }
    }
}