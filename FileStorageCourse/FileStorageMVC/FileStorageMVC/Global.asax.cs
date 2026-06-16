using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FileStorageMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            if (exception == null)
            {
                return;
            }

            string message = exception.Message ?? string.Empty;
            bool isMaxRequestError =
                message.IndexOf("Maximum request length exceeded", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("Максимальная длина запроса", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isMaxRequestError)
            {
                return;
            }

            Server.ClearError();
            Response.Clear();
            Response.Redirect("~/File/UploadFile?error=maxsize", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
