using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using MiniMe.Models;
using System;
using System.Configuration;

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
            routes.MapRoute("Shorten", "Shorten/{id}", new { controller = "Home", action = "Shorten", id = UrlParameter.Optional });
            routes.MapRoute("GetDestination", "{id}", new { controller = "Home", action = "GetDestination", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            Database.SetInitializer<MiniMeContext>(new MiniMeInitializer());

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //Purges any links not used within the number of days specified. 
            //TODO: Fix logic to work with SQL CE
            //PurgeOldLinks(Convert.ToInt32(ConfigurationManager.AppSettings["DaysToKeepShortCodes"]));
        }

        /// <summary>
        /// Removes any short codes from the database if they haven't been used in X days.
        /// </summary>
        /// <param name="MaxAgeInDays"></param>
        protected void PurgeOldLinks(int MaxAgeInDays)
        {
            DateTime ExpireDate = DateTime.Now.AddDays(-MaxAgeInDays);
            MiniMeContext db = new MiniMeContext();
            string SQL = @"DELETE FROM Links WHERE LastAccessed <= '" + ExpireDate + "'";
            db.Database.ExecuteSqlCommand(SQL);
        }
    }
}