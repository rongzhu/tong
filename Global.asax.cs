using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace tongbro
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
		protected void Application_Start(Object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();

            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            //web api 2
            GlobalConfiguration.Configure(WebApiConfig.Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
			var u = Request.Url;
			var url = u.ToString().ToLower();
			if (!Request.IsLocal && !u.Host.StartsWith("www."))
				Response.Redirect(u.ToString().Replace(u.Host, "www." + u.Host));
            //else if (!url.Contains("/login") && !url.Contains("/expense") && !url.Contains("/costparse.aspx") && Request.Cookies["tongbrospreview"] == null)
            //    Response.Redirect("/login");

            if (Request.Url.AbsolutePath.StartsWith("/ng/")) HttpContext.Current.RewritePath("~/ng/index.html");
        }

		protected void Application_Error(Object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();
		}

    }
}
