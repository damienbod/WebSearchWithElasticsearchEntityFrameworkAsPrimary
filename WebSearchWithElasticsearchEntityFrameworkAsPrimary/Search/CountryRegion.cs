using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.Search
{
	public class CountryRegion
	{
		[Key]
		[StringLength(3)]
		public string CountryRegionCode { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		public DateTime ModifiedDate { get; set; }
	}
}