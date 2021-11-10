using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Sid;
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Surname;
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Email;
        }
    }
}
