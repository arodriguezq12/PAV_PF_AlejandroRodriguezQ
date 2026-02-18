using System;
using System.Security.Principal;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace PAV_PF_AlejandroRodriguezQ
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = "name";
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || string.IsNullOrWhiteSpace(authCookie.Value))
            {
                return;
            }

            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }

            if (ticket == null || ticket.Expired)
            {
                return;
            }

            var role = string.IsNullOrWhiteSpace(ticket.UserData) ? "Cliente" : ticket.UserData;
            var identity = new FormsIdentity(ticket);
            var principal = new GenericPrincipal(identity, new[] { role });

            HttpContext.Current.User = principal;
            Context.User = principal;
            System.Threading.Thread.CurrentPrincipal = principal;
        }
    }
}
