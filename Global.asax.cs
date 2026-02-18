using System;
using System.Web;
using System.Web.Security;
using System.Security.Principal;

public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
    {
        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        if (authCookie == null) return;

        FormsAuthenticationTicket ticket;
        try
        {
            ticket = FormsAuthentication.Decrypt(authCookie.Value);
        }
        catch
        {
            return;
        }

        if (ticket == null || ticket.Expired) return;

        // El rol va en UserData
        var role = string.IsNullOrWhiteSpace(ticket.UserData) ? "Cliente" : ticket.UserData;

        var identity = new FormsIdentity(ticket);
        var principal = new GenericPrincipal(identity, new[] { role });

        HttpContext.Current.User = principal;
        System.Threading.Thread.CurrentPrincipal = principal;
    }
}
