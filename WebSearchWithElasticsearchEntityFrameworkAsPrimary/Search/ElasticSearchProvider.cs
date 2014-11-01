using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ElasticsearchCRUD;
using WebSearchWithElasticsearchEntityFrameworkAsPrimary.DomainModel;
using WebSearchWithElasticsearchEntityFrameworkAsPrimary.Models;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.Search
{
	public class ElasticsearchProvider : ISearchProvider, IDisposable
	{
		private const string ConnectionString = "http://localhost.fiddler:9200/";
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver;
		private readonly ElasticsearchContext _elasticsearchContext;
		private readonly EfModel _entityFrameworkContext;

		public ElasticsearchProvider()
		{
			_elasticsearchMappingResolver = new ElasticsearchMappingResolver();
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(Address), new ElasticsearchMappingAddress());
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(Address).BaseType, new ElasticsearchMappingAddress());
		    _elasticsearchContext = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver,true,true));
			_entityFrameworkContext = new EfModel();
		}

		public IEnumerable<T> QueryString<T>(string term) 
		{ 
			return _elasticsearchContext.Search<T>(BuildQueryStringSearch(term)).PayloadResult.ToList();
		}

		private string BuildQueryStringSearch(string term)
		{
			var names = "";
			if (term != null)
			{
				names = term.Replace("+", " OR *");
			}

			var buildJson = new StringBuilder();
			buildJson.AppendLine("{");
			buildJson.AppendLine(" \"query\": {");
			buildJson.AppendLine("   \"query_string\": {");
			buildJson.AppendLine("      \"query\": \"" + names + "*\"");
			buildJson.AppendLine("     }");
			buildJson.AppendLine("  }");
			buildJson.AppendLine("}");

			return buildJson.ToString();
		}

		public void AddUpdateDocument(Address address)
		{
			address.ModifiedDate = DateTime.UtcNow;
			address.rowguid = Guid.NewGuid();
			var entityAddress = _entityFrameworkContext.Address.Add(address);
			_entityFrameworkContext.SaveChanges();

			// we use the entity result with the proper ID
			_elasticsearchContext.AddUpdateDocument(entityAddress, entityAddress.AddressID, entityAddress.StateProvinceID);
			_elasticsearchContext.SaveChanges();
		}

		public void UpdateAddresses(long stateProvinceId, List<Address> addresses)
		{
			foreach (var item in addresses)
			{
				// if the parent has changed, the child needs to be deleted and created again. This in not required in this example
				var addressItem = _elasticsearchContext.SearchById<Address>(item.AddressID);
				// need to update a entity here
				var entityAddress = _entityFrameworkContext.Address.First(t => t.AddressID == addressItem.AddressID);

				if (entityAddress.StateProvinceID != addressItem.StateProvinceID)
				{
					_elasticsearchContext.DeleteDocument<Address>(addressItem.AddressID);
				}

				entityAddress.AddressLine1 = item.AddressLine1;
				entityAddress.AddressLine2 = item.AddressLine2;
				entityAddress.City = item.City;
				entityAddress.ModifiedDate = DateTime.UtcNow;
				entityAddress.PostalCode = item.PostalCode;
				item.rowguid = entityAddress.rowguid;
				item.ModifiedDate = DateTime.UtcNow;

				_elasticsearchContext.AddUpdateDocument(item, item.AddressID, item.StateProvinceID);
			}

			_entityFrameworkContext.SaveChanges();
			_elasticsearchContext.SaveChanges();
		}

		public void DeleteAddress(long addressId)
		{
			var address = new Address { AddressID = (int)addressId };
			_entityFrameworkContext.Address.Attach(address);
			_entityFrameworkContext.Address.Remove(address);
			_elasticsearchContext.DeleteDocument<Address>(addressId);

			_entityFrameworkContext.SaveChanges();
			_elasticsearchContext.SaveChanges();
		}

		public List<SelectListItem> GetAllStateProvinces()
		{
			var result = from element in _elasticsearchContext.Search<StateProvince>("").PayloadResult
						 select new SelectListItem
						 {
							 Text = string.Format("StateProvince: {0}, CountryRegionCode {1}", 
							 element.StateProvinceCode, element.CountryRegionCode), 
							 Value = element.StateProvinceID.ToString(CultureInfo.InvariantCulture)
						 };

			return result.ToList();
		}

		public PagingTableResult<Address> GetAllAddressesForStateProvince(string stateprovinceid, int jtStartIndex, int jtPageSize, string jtSorting)
		{
			var result = new PagingTableResult<Address>();
			var data = _elasticsearchContext.Search<Address>(
							BuildSearchForChildDocumentsWithIdAndParentType(
								stateprovinceid, 
								"stateprovince",
								jtStartIndex, 
								jtPageSize, 
								jtSorting)
						);

			result.Items = data.PayloadResult.ToList();
			result.TotalCount = data.TotalHits;
			return result;
		}

		// {
		//  "from": 0, "size": 10,
		//  "query": {
		//	"term": { "_parent": "parentdocument#7" }
		//  }
		// }
		private string BuildSearchForChildDocumentsWithIdAndParentType(object parentId, string parentType, int jtStartIndex, int jtPageSize, string jtSorting)
		{
			var sorts = jtSorting.Split(' ');
			// todo adding sorting...
			var buildJson = new StringBuilder();
			buildJson.AppendLine("{");
			buildJson.AppendLine("\"from\" : " + jtStartIndex + ", \"size\" : " + jtPageSize + ",");
			buildJson.AppendLine("\"query\": {");
			buildJson.AppendLine("\"term\": {\"_parent\": \"" + parentType + "#" + parentId + "\"}");
			buildJson.AppendLine("},");
			buildJson.AppendLine("\"sort\": { \"" + sorts[0].ToLower() + "\": { \"order\": \"" + sorts[1].ToLower() + "\" }}");
			 

			buildJson.AppendLine("}");

			return buildJson.ToString();
		}

		private bool isDisposed;
		public void Dispose()
		{
			if (!isDisposed)
			{
				isDisposed = true;
				_elasticsearchContext.Dispose();
				_entityFrameworkContext.Dispose();
			}
		}
	}
}