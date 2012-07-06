using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using MiniMe.Models;

namespace MiniMe
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute("Default", "", new { controller = "Home", action = "Index" });
            routes.MapRoute("Shorten", "Shorten", new { controller = "Home", action = "Shorten" });
            //routes.MapRoute("Shorten", "Shorten/{id}", new { controller = "Home", action = "Shorten", id = UrlParameter.Optional });   
          //  routes.MapRoute("Link", "{controller}/{action}/{id}", new { controller = "Link", action = "Index", id = UrlParameter.Optional });
            //routes.MapRoute("Account", "{controller}
            routes.MapRoute("GetDestination", "{id}", new { controller = "Home", action = "GetDestination", id = UrlParameter.Optional });
            

        }

        protected void Application_Start()
        {
            Database.SetInitializer<LinkDBContext>(new LinkInitializer());
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}