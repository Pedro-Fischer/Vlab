using System.Collections.Generic;
using Vlab.Dominio.ModelViews;
using Xunit;

namespace Vlab.Testes
{
    public class PagedListTests
    {
        [Fact]
        public void PagedList_Constructor_SetsProperties()
        {
            var items = new List<string> { "a", "b" };
            var paged = new PagedList<string>(items, totalCount: 50, page: 3);

            Assert.Equal(3, paged.Page);
            Assert.Equal(50, paged.TotalCount);
            Assert.Equal(items, paged.Items);
        }
    }
}
