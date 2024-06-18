using MotoRiders.CR.Models;
using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MotoRiders.CR.Controllers
{
    public class CuentaController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registro(ClienteModel cliente)
        {
            if (ModelState.IsValid)
            {
                if (EmailRegistrado(cliente.email))
                {
                    ModelState.AddModelError("Email", "Este correo ya está registrado.");
                    return View(cliente);
                }

                InsertarCliente(cliente);

                return RedirectToAction("InicioSesion", "Cuenta");
            }

            return View(cliente);
        }

        private bool EmailRegistrado(string email)
        {
            string query = "SELECT COUNT(*) FROM Clientes WHERE email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private void InsertarCliente(ClienteModel cliente)
        {
            string query = "INSERT INTO Clientes (cedula, nombre, direccion, telefono, email, contraseña) " +
                           "VALUES (@Cedula, @Nombre, @Direccion, @Telefono, @Email, @Contraseña)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Cedula", cliente.cedula);
                    command.Parameters.AddWithValue("@Nombre", cliente.nombre);
                    command.Parameters.AddWithValue("@Direccion", cliente.direccion);
                    command.Parameters.AddWithValue("@Telefono", cliente.telefono);
                    command.Parameters.AddWithValue("@Email", cliente.email);
                    command.Parameters.AddWithValue("@Contraseña", cliente.contraseña);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public ActionResult InicioSesion()
        {
            return View();
        }

        [HttpPost]
        public ActionResult InicioSesion(string email, string contraseña)
        {
            if (VerificarCredenciales(email, contraseña))
            {
                // Configurar el ticket de autenticación manualmente
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1, // Versión del ticket
                    email, // Nombre del usuario asociado al ticket
                    DateTime.Now, // Fecha y hora de emisión
                    DateTime.Now.AddMinutes(30), // Fecha y hora de expiración
                    false, // Si la cookie debe ser persistente
                    String.Empty, // Datos de usuario (puede ser una cadena con información adicional)
                    FormsAuthentication.FormsCookiePath); // Ruta de la cookie

                // Encriptar el ticket
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                // Crear la cookie
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);

                TempData["Mensaje"] = "¡Inicio de sesión exitoso!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "El email o la contraseña son incorrectos.");
            return View();
        }



        private bool VerificarCredenciales(string email, string contraseña)
        {
            string query = "SELECT COUNT(*) FROM Clientes WHERE email = @Email AND contraseña = @Contraseña";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Contraseña", contraseña);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private ClienteModel ObtenerClienteDesdeSesion()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }

            string email = User.Identity.Name;
            string query = "SELECT idCliente, cedula, nombre, direccion, telefono FROM Clientes WHERE email = @Email";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ClienteModel cliente = new ClienteModel();
                            cliente.idCliente = (int)reader["idCliente"];
                            cliente.cedula = reader["cedula"].ToString();
                            cliente.nombre = reader["nombre"].ToString();
                            cliente.direccion = reader["direccion"].ToString();
                            cliente.telefono = reader["telefono"].ToString();
                            return cliente;
                        }
                        return null; // Si no se encuentra el cliente
                    }
                }
            }
        }

        public ActionResult MiPerfil()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("InicioSesion", "Cuenta");
            }

            ClienteModel cliente = ObtenerClienteDesdeSesion();
            if (cliente != null)
            {
                return View(cliente);
            }

            // Manejar el caso donde el cliente no se encontró
            return RedirectToAction("InicioSesion", "Cuenta");
        }


        [HttpPost]
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }













    }
}
