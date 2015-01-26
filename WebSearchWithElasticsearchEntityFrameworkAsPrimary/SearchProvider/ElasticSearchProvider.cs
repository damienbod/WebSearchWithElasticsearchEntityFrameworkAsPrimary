using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ElasticsearchCRUD;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Queries;
using ElasticsearchCRUD.Model.SearchModel.Sorting;
using WebSearchWithElasticsearchEntityFrameworkAsPrimary.DomainModel;
using WebSearchWithElasticsearchEntityFrameworkAsPrimary.Models;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.SearchProvider
{
	public class ElasticsearchProvider : ISearchProvider, IDisposable
	{
		private const string ConnectionString = "http://localhost:9200/";
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver;
		private readonly ElasticsearchContext _elasticsearchContext;
		private readonly EfModel _entityFrameworkContext;

		public ElasticsearchProvider()
		{
			_elasticsearchMappingResolver = new ElasticsearchMappingResolver();
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(Address), new ElasticsearchMappingAddress());
		    _elasticsearchContext = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver,true,true));
			_entityFrameworkContext = new EfModel();
		}

		public IEnumerable<T> QueryString<T>(string term)
		{
			var results = _elasticsearchContext.Search<T>(BuildQueryStringSearch(term));
			return results.PayloadResult.Hits.HitsResult.Select(t =>t.Source).ToList();
		}

		private Search BuildQueryStringSearch(string term)
		{
			var names = "";
			if (term != null)
			{
				names = term.Replace("+", " OR *");
			}

			var search = new Search
			{
				Query = new Query(new QueryStringQuery(names + "*"))
			};

			return search;
		}

		public void AddUpdateDocument(Address address)
		{
			address.ModifiedDate = DateTime.UtcNow;
			address.rowguid = Guid.NewGuid();
			var entityAddress = _entityFrameworkContext.Address.Add(address);
			_entityFrameworkContext.SaveChanges();

			// we use the entity result with the proper ID
			_elasticsearchContext.AddUpdateDocument(entityAddress, entityAddress.AddressID, new RoutingDefinition{ ParentId = entityAddress.StateProvinceID});
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

				_elasticsearchContext.AddUpdateDocument(item, item.AddressID,  new RoutingDefinition{ ParentId =item.StateProvinceID});
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
			var result = from element in _elasticsearchContext.Search<StateProvince>("").PayloadResult.Hits.HitsResult
						 select new SelectListItem
						 {
							 Text = string.Format("StateProvince: {0}, CountryRegionCode {1}",
							 element.Source.StateProvinceCode, element.Source.CountryRegionCode),
							 Value = element.Source.StateProvinceID.ToString(CultureInfo.InvariantCulture)
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

			result.Items = data.PayloadResult.Hits.HitsResult.Select(t => t.Source).ToList();
			result.TotalCount = data.PayloadResult.Hits.Total;
			return result;
		}

		// {
		//  "from": 0, "size": 10,
		//  "query": {
		//	"term": { "_parent": "parentdocument#7" }
		//  },
		//  "sort": { "city" : { "order": "desc" } }"
		// }
		private Search BuildSearchForChildDocumentsWithIdAndParentType(object parentId, string parentType, int jtStartIndex, int jtPageSize, string jtSorting)
		{
			var search = new Search
			{
				From = jtStartIndex,
				Size = jtPageSize,
				Query = new Query(new TermQuery("_parent", parentType + "#" + parentId))	
			};

			var sorts = jtSorting.Split(' ');
			if (sorts.Length == 2)
			{
				var order = OrderEnum.asc;
				if (sorts[1].ToLower() == "desc")
				{
					order = OrderEnum.desc;
				}

				search.Sort = CreateSortQuery(sorts[0].ToLower(), order);
			}
			return search;
		}

		public SortHolder CreateSortQuery(string sort, OrderEnum order)
		{
			return new SortHolder(
				new List<ISort>
				{
					new SortStandard(sort)
					{
						Order = order
					}
				}
			);
		}


		private bool _isDisposed;
		public void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				_elasticsearchContext.Dispose();
				_entityFrameworkContext.Dispose();
			}
		}
	}
}