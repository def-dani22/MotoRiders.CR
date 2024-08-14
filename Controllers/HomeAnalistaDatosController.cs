using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;

namespace MotoRiders.CR.Controllers
{
    public class HomeAnalistaDatosController : Controller
    {
        // GET: HomeAnalistaDatos
        public ActionResult Index()
        {
            return View();
        }




        public static class AuditoriaHelper
        {
            //private static string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";
            private static string connectionString = "Data Source=138.59.135.33\\MSSQLSERVER2019;Initial Catalog=motoridersbd;User ID=motoridersbd;Password=Cmotoridersbd*2024;";

            public static void RegistrarAccion(string usuario, string accion, string detalles = null, string ipAddress = null)
            {
                string query = @"
            INSERT INTO Auditoria (Usuario, Accion, Detalles, FechaHora, IPAddress) 
            VALUES (@Usuario, @Accion, @Detalles, @FechaHora, @IPAddress)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Usuario", usuario);
                        command.Parameters.AddWithValue("@Accion", accion);
                        command.Parameters.AddWithValue("@Detalles", detalles ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@FechaHora", DateTime.Now);
                        command.Parameters.AddWithValue("@IPAddress", ipAddress ?? (object)DBNull.Value);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }



        [HttpPost]
        public ActionResult CerrarSesion()
        {
            try
            {
                // Obtener el email del usuario autenticado
                string email = User.Identity.Name;
                string ipAddress = Request.UserHostAddress; // Obtener la IP del usuario

                // Registrar acción de auditoría para el cierre de sesión
                AuditoriaHelper.RegistrarAccion(email, "Cierre de Sesión", "El usuario cerró sesión.", ipAddress);

                // Cerrar sesión
                FormsAuthentication.SignOut();
            }
            catch (Exception ex)
            {
                // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                throw new Exception("Error al cerrar sesión.", ex);
            }

            return RedirectToAction("InicioSesion", "Cuenta");
        }
    }
}