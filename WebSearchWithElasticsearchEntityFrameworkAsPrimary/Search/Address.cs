using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.Search
{
	public class Address
	{
		[Key]
		public int AddressID { get; set; }

		[Required]
		[StringLength(60)]
		public string AddressLine1 { get; set; }

		[StringLength(60)]
		public string AddressLine2 { get; set; }

		[Required]
		[StringLength(30)]
		public string City { get; set; }

		public int StateProvinceID { get; set; }

		[Required]
		[StringLength(15)]
		public string PostalCode { get; set; }

		public Guid rowguid { get; set; }

		public DateTime ModifiedDate { get; set; }
	}
}