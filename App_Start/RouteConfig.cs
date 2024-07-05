using System.Web.Mvc;
using System.Web.Routing;

namespace MotoRiders.CR
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Cuenta", action = "InicioSesion", id = UrlParameter.Optional }
            );
            // Ruta personalizada para ForgotPassword
            routes.MapRoute(
                name: "ForgotPassword",
                url: "Cuenta/OlvideMiContrasena",
                defaults: new { controller = "Cuenta", action = "OlvideMiContrasena" }
            );

            routes.MapRoute(
                name: "ResetPassword",
                url: "Cuenta/RestablecerContrasena",
                defaults: new { controller = "Cuenta", action = "RestablecerContrasena" }
            );
        }
    }
}
