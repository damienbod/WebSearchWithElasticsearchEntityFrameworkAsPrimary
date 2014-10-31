using System.Collections.Generic;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.Models
{
	public class PagingTableResult<T>
	{
		public List<T> Items { get; set; }
		public long TotalCount { get; set; }
	}
}