using MotoRiders.CR.Models;
using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;

namespace MotoRiders.CR.Controllers
{
    public class CuentaController : Controller
    {
        // Cadena de conexión hacia tu base de datos
        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        // GET: Cuenta/Registro
        public ActionResult Registro()
        {
            return View();
        }

        // POST: Cuenta/Registro
        [HttpPost]
        public ActionResult Registro(ClienteModel cliente)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el email ya está registrado
                if (EmailRegistrado(cliente.email))
                {
                    ModelState.AddModelError("Email", "Este correo ya está registrado.");
                    return View(cliente);
                }

                // Guardar el cliente en la base de datos
                InsertarCliente(cliente);

                // Redirigir al usuario a la página de inicio de sesión
                return RedirectToAction("InicioSesion", "Cuenta");
            }

            return View(cliente);
        }

        // Método para verificar si el email ya está registrado
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

        // Método para insertar un nuevo cliente en la base de datos
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

        // GET: Cuenta/InicioSesion
        public ActionResult InicioSesion()
        {
            return View();
        }

        // POST: Cuenta/InicioSesion
        [HttpPost]
        public ActionResult InicioSesion(string email, string contraseña)
        {
            // Verificar las credenciales del cliente
            if (VerificarCredenciales(email, contraseña))
            {
                // Iniciar sesión exitosa
                FormsAuthentication.SetAuthCookie(email, false);
                return RedirectToAction("Index", "Home"); // Redirigir a la página principal después del inicio de sesión
            }

            // Credenciales incorrectas, mostrar mensaje de error
            ModelState.AddModelError("", "El email o la contraseña son incorrectos.");
            return View();
        }

        // Método para verificar las credenciales del cliente
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

        // Método para obtener el ID del cliente a partir del email almacenado en la sesión
        private int ObtenerIdClienteDesdeSesion()
        {
            string email = User.Identity.Name; // Obtener el email del usuario autenticado
            string query = "SELECT Id FROM Clientes WHERE email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    return -1; // En caso de no encontrar el cliente, podrías manejar este caso según tu lógica
                }
            }
        }

        // POST: Cuenta/CerrarSesion
        [HttpPost]
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Inicio", "Home"); // Redirigir a la página de inicio después de cerrar sesión
        }
    }
}
