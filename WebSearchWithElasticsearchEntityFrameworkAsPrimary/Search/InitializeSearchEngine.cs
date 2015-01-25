using System;
using System.Diagnostics;
using System.Linq;
using ElasticsearchCRUD;
using ElasticsearchCRUD.Tracing;
using WebSearchWithElasticsearchEntityFrameworkAsPrimary.DomainModel;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.Search
{
	public class InitializeSearchEngine
	{
		private readonly Stopwatch _stopwatch = new Stopwatch();

		public void SaveToElasticsearchStateProvinceIfitDoesNotExist()
		{
			IElasticsearchMappingResolver elasticsearchMappingResolver = new ElasticsearchMappingResolver();
			using (var elasticSearchContext = new ElasticsearchContext("http://localhost:9200/", new ElasticsearchSerializerConfiguration(elasticsearchMappingResolver, true, true)))
			{
				if (!elasticSearchContext.IndexTypeExists<Address>())
				{
					elasticSearchContext.TraceProvider = new ConsoleTraceProvider();
					using (var databaseEfModel = new EfModel())
					{
						int pointer = 0;
						const int interval = 20;
						bool firstRun = true;
						int length = databaseEfModel.StateProvince.Count();

						while (pointer < length)
						{
							_stopwatch.Start();
							var collection =
								databaseEfModel.StateProvince.OrderBy(t => t.StateProvinceID)
									.Skip(pointer)
									.Take(interval)
									.ToList<StateProvince>();
							_stopwatch.Stop();
							Console.WriteLine("Time taken for select {0} Address: {1}", interval, _stopwatch.Elapsed);
							_stopwatch.Reset();

							_stopwatch.Start();
							foreach (var item in collection)
							{
								var ee = item.CountryRegion.Name;
								elasticSearchContext.AddUpdateDocument(item, item.StateProvinceID);
							}

							if (firstRun)
							{
								elasticSearchContext.SaveChangesAndInitMappings();
								firstRun = false;
							}
							else
							{
								elasticSearchContext.SaveChanges();
							}

							_stopwatch.Stop();
							Console.WriteLine("Time taken to insert {0} Address documents: {1}", interval, _stopwatch.Elapsed);
							_stopwatch.Reset();
							pointer = pointer + interval;
							Console.WriteLine("Transferred: {0} items", pointer);
						}
					}
				}
			}
		}

	}
}