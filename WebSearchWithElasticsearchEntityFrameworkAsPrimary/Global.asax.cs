using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebSearchWithElasticsearchEntityFrameworkAsPrimary.SearchProvider;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			var initializeSearchEngine = new InitializeSearchEngine();
			initializeSearchEngine.SaveToElasticsearchStateProvinceIfitDoesNotExist();

			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}
	}
}
