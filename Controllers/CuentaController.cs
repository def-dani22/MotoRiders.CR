using MotoRiders.CR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MotoRiders.CR.Controllers
{
    public class CuentaController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        // Método para consultar los datos de una persona por cédula
        [HttpGet]
        public JsonResult ConsultarPersona(string cedula)
        {
            string apiUrl = "http://localhost:5001/consultar_persona?cedula=" + cedula;

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(apiUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content.ReadAsStringAsync().Result;
                    var persona = JsonConvert.DeserializeObject<PersonaModel>(data);
                    return Json(persona, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { message = "Persona no encontrada" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        [HttpGet]
        public JsonResult ObtenerProvincias(int idPais)
        {
            List<SelectListItem> provincias = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT idProvincia, nombre FROM Provincia WHERE idPais = @idPais", conn);
                cmd.Parameters.AddWithValue("@idPais", idPais);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    provincias.Add(new SelectListItem
                    {
                        Value = reader["idProvincia"].ToString(),
                        Text = reader["nombre"].ToString()
                    });
                }
            }

            return Json(provincias, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ObtenerCantones(int idProvincia)
        {
            List<SelectListItem> cantones = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT idCanton, nombre FROM Canton WHERE idProvincia = @idProvincia", conn);
                cmd.Parameters.AddWithValue("@idProvincia", idProvincia);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cantones.Add(new SelectListItem
                    {
                        Value = reader["idCanton"].ToString(),
                        Text = reader["nombre"].ToString()
                    });
                }
            }

            return Json(cantones, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult ObtenerDistritos(int idCanton)
        {
            List<SelectListItem> distritos = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT idDistrito, nombre FROM Distrito WHERE idCanton = @idCanton", conn);
                cmd.Parameters.AddWithValue("@idCanton", idCanton);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    distritos.Add(new SelectListItem
                    {
                        Value = reader["idDistrito"].ToString(),
                        Text = reader["nombre"].ToString()
                    });
                }
            }

            return Json(distritos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Registro()
        {
            var paises = new List<SelectListItem>();
            string query = "SELECT idPais, nombre FROM Pais";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            paises.Add(new SelectListItem
                            {
                                Value = reader["idPais"].ToString(),
                                Text = reader["nombre"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Paises = new SelectList(paises, "Value", "Text");
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

                // Generar y enviar el token de verificación
                string token = TokenGenerator.GenerarTokensSeguridad(10);
                string encryptedToken = EncryptionHelper.Encrypt(token);

                try
                {
                    // Enviar el correo con el token
                    EnviarCorreoVerificacion(cliente.email, token);

                    // Guardar los datos del cliente temporalmente en TempData
                    TempData["Cedula"] = cliente.cedula;
                    TempData["Nombre"] = cliente.nombre;
                    TempData["Direccion"] = cliente.direccion;
                    TempData["Telefono"] = cliente.telefono;
                    TempData["Email"] = cliente.email;
                    TempData["Contraseña"] = cliente.contraseña;
                    TempData["PreguntaSeguridad1"] = cliente.preguntaSeguridad1;
                    TempData["RespuestaSeguridad1"] = cliente.respuestaSeguridad1;
                    TempData["PreguntaSeguridad2"] = cliente.preguntaSeguridad2;
                    TempData["RespuestaSeguridad2"] = cliente.respuestaSeguridad2;
                    //TempData["IdPais"] = cliente.IdPais;
                    //TempData["IdProvincia"] = cliente.idProvincia;
                    //TempData["IdCanton"] = cliente.idCanton;
                    //TempData["IdDistrito"] = cliente.idDistrito;
                    TempData["TokenVerificacion"] = encryptedToken;

                    // Redirigir a la vista de verificación de correo
                    return RedirectToAction("VerificarCorreo");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocurrió un error al enviar el correo de verificación: " + ex.Message);
                    return View(cliente);
                }
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








        // Método para insertar un nuevo cliente y asignarle el rol "User"
        private void InsertarCliente(ClienteModel cliente)
        {
            string query = @"
        INSERT INTO Clientes (cedula, nombre, direccion, telefono, email, contraseña, preguntaSeguridad1, respuestaSeguridad1, preguntaSeguridad2, respuestaSeguridad2) 
        VALUES (@Cedula, @Nombre, @Direccion, @Telefono, @Email, @Contraseña, @PreguntaSeguridad1, @RespuestaSeguridad1, @PreguntaSeguridad2, @RespuestaSeguridad2);
        SELECT SCOPE_IDENTITY();
    ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Cedula", cliente.cedula);
                        command.Parameters.AddWithValue("@Nombre", cliente.nombre);
                        command.Parameters.AddWithValue("@Direccion", cliente.direccion);
                        command.Parameters.AddWithValue("@Telefono", cliente.telefono);
                        command.Parameters.AddWithValue("@Email", cliente.email);
                        command.Parameters.AddWithValue("@Contraseña", cliente.contraseña);
                        command.Parameters.AddWithValue("@PreguntaSeguridad1", cliente.preguntaSeguridad1);
                        command.Parameters.AddWithValue("@RespuestaSeguridad1", cliente.respuestaSeguridad1);
                        command.Parameters.AddWithValue("@PreguntaSeguridad2", cliente.preguntaSeguridad2);
                        //command.Parameters.AddWithValue("@RespuestaSeguridad2", cliente.respuestaSeguridad2);
                        //command.Parameters.AddWithValue("@IdPais", cliente.IdPais);
                        //command.Parameters.AddWithValue("@IdProvincia", cliente.idProvincia);
                        //command.Parameters.AddWithValue("@IdCanton", cliente.idCanton);
                        //command.Parameters.AddWithValue("@IdDistrito", cliente.idDistrito);

                        // Ejecutar la inserción y obtener el ID del cliente
                        int idCliente = Convert.ToInt32(command.ExecuteScalar());

                        // Asignar el rol "User" al nuevo cliente
                        AsignarRol(idCliente, "User", connection);
                    }
                }
                catch (Exception ex)
                {
                    // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                    throw new Exception("Error al insertar cliente y obtener ID.", ex);
                }
            }
        }



        // Método para verificar si el cliente tiene un rol específico
        private bool ClienteTieneRol(string email, string rolNombre)
        {
            string query = @"
            SELECT COUNT(*) 
            FROM UsuarioRoles ur 
            JOIN Clientes c ON ur.idUsuario = c.idCliente 
            JOIN Roles r ON ur.idRol = r.idRol 
            WHERE c.email = @Email AND r.nombre = @RolNombre
        ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@RolNombre", rolNombre);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Método para asignar un rol específico a un cliente
        private void AsignarRol(int idCliente, string rolNombre, SqlConnection connection)
        {
            string query = @"
            INSERT INTO UsuarioRoles (idUsuario, idRol) 
            SELECT @IdCliente, idRol 
            FROM Roles 
            WHERE nombre = @RolNombre
        ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdCliente", idCliente);
                command.Parameters.AddWithValue("@RolNombre", rolNombre);
                command.ExecuteNonQuery();
            }
        }


        public ActionResult VerificarCorreo()
        {
            return View();
        }



        [HttpPost]
        public ActionResult VerificarCorreo(string token)
        {
            if (TempData["TokenVerificacion"] == null)
            {
                return RedirectToAction("Registro");
            }

            string encryptedToken = TempData["TokenVerificacion"].ToString();

            if (token.Equals(EncryptionHelper.Decrypt(encryptedToken), StringComparison.OrdinalIgnoreCase))
            {
                // Token correcto, registrar el cliente
                try
                {
                    ClienteModel clienteTemporal = new ClienteModel
                    {
                        cedula = TempData["Cedula"]?.ToString(),
                        nombre = TempData["Nombre"]?.ToString(),
                        direccion = TempData["Direccion"]?.ToString(),
                        telefono = TempData["Telefono"]?.ToString(),
                        email = TempData["Email"]?.ToString(),
                        contraseña = EncryptionHelper.Encrypt(TempData["Contraseña"]?.ToString()),
                        preguntaSeguridad1 = TempData["PreguntaSeguridad1"]?.ToString(),
                        respuestaSeguridad1 = TempData["RespuestaSeguridad1"]?.ToString(),
                        preguntaSeguridad2 = TempData["PreguntaSeguridad2"]?.ToString(),
                        respuestaSeguridad2 = TempData["RespuestaSeguridad2"]?.ToString(),
                        //IdPais = Convert.ToInt32(TempData["IdPais"]),
                        //idProvincia = Convert.ToInt32(TempData["IdProvincia"]),
                        //idCanton = Convert.ToInt32(TempData["IdCanton"]),
                        //idDistrito = Convert.ToInt32(TempData["IdDistrito"])
                    };

                    InsertarCliente(clienteTemporal);
                    TempData["Message"] = "Registro exitoso. Ahora puede iniciar sesión.";
                    //return RedirectToAction("InicioSesion", "Cuenta");
                    return RedirectToAction("Confirmacion");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocurrió un error al registrar el cliente: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "El token de verificación es incorrecto.");
            }

            return View();
        }



        public ActionResult Confirmacion()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }


        //verificartoken2FA
        private void EnviarCorreoVerificacion(string email, string token)
        {
            string remitente = "estebangomez1015@gmail.com";
            string contraseña = "dzmoqbkgfzfqquwf";
            string asunto = "Confirmar Registro";
            string cuerpo = $"Para registrarse en MotoRiders, ingrese el siguiente código: {token}";

            try
            {
                using (SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    clienteSmtp.EnableSsl = true;
                    clienteSmtp.Credentials = new NetworkCredential(remitente, contraseña);

                    using (MailMessage mensaje = new MailMessage(remitente, email, asunto, cuerpo))
                    {
                        mensaje.IsBodyHtml = true;
                        clienteSmtp.Send(mensaje);
                    }
                }

                // Registrar acción de auditoría para el envío de correo de verificación
                string ipAddress = Request.UserHostAddress; // Obtener la IP del usuario
                AuditoriaHelper.RegistrarAccion(email, "Correo de Verificación Enviado", $"Se envió un correo de verificación con el token: {token}", ipAddress);
            }
            catch (Exception ex)
            {
                // Manejar la excepción adecuadamente (log, mens  aje al usuario, etc.)
                throw new Exception("Error al enviar el correo de verificación.", ex);
            }
        }






















        public ActionResult ResponderPreguntaSeguridad()
        {
            if (TempData["Email"] == null)
            {
                return RedirectToAction("OlvideMiContrasena");
            }

            string email = TempData["Email"].ToString();
            int idCliente = ObtenerIdClientePorEmail(email);

            // Seleccionar aleatoriamente entre PreguntaSeguridad1 y PreguntaSeguridad2
            string preguntaSeleccionada;
            string pregunta;
            var random = new Random();
            if (random.Next(2) == 0)
            {
                pregunta = ObtenerPreguntaSeguridad1(idCliente);
                preguntaSeleccionada = "RespuestaSeguridad1";
            }
            else
            {
                pregunta = ObtenerPreguntaSeguridad2(idCliente);
                preguntaSeleccionada = "RespuestaSeguridad2";
            }

            // Almacenar la pregunta seleccionada y la columna de la respuesta en TempData
            TempData["PreguntaSeguridad"] = pregunta;
            TempData["PreguntaSeleccionada"] = preguntaSeleccionada;
            TempData["Email"] = email;

            ViewBag.Pregunta = pregunta;
            ViewBag.Email = email;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResponderPreguntaSeguridad(string respuesta)
        {
            if (TempData["Email"] == null || TempData["PreguntaSeleccionada"] == null)
            {
                return RedirectToAction("OlvideMiContrasena");
            }

            string email = TempData["Email"].ToString();
            string preguntaSeleccionada = TempData["PreguntaSeleccionada"].ToString();
            int idCliente = ObtenerIdClientePorEmail(email);

            string respuestaAlmacenada = ObtenerRespuestaSeguridad(email, preguntaSeleccionada);

            if (respuesta.Equals(respuestaAlmacenada, StringComparison.OrdinalIgnoreCase))
            {
                // Respuesta correcta, generar y enviar token
                string token = TokenGenerator.GenerarTokensSeguridad(10);
                string encryptedToken = EncryptionHelper.Encrypt(token);

                try
                {
                    GuardarToken(idCliente, encryptedToken, DateTime.Now);
                    EnviarCorreoRecuperacion(email, token);
                    TempData["Message"] = "Se ha enviado un correo con las instrucciones para recuperar su contraseña.";
                    return RedirectToAction("RecuperarContrasena", new { token = token });
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Ocurrió un error al enviar el correo de recuperación: " + ex.Message;
                }
            }
            else
            {
                ViewBag.ErrorMessage = "La respuesta de seguridad es incorrecta.";
            }

            // Almacenar la pregunta y el email de nuevo en ViewBag para mostrar en la vista
            ViewBag.Pregunta = TempData["PreguntaSeguridad"].ToString();
            ViewBag.Email = email;

            return View();
        }

        private string ObtenerPreguntaSeguridad1(int idCliente)
        {
            string pregunta = null;
            string query = "SELECT PreguntaSeguridad1 FROM Clientes WHERE idCliente = @IdCliente";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCliente", idCliente);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pregunta = reader["PreguntaSeguridad1"].ToString();
                        }
                    }
                }
            }

            return pregunta;
        }

        private string ObtenerPreguntaSeguridad2(int idCliente)
        {
            string pregunta = null;
            string query = "SELECT PreguntaSeguridad2 FROM Clientes WHERE idCliente = @IdCliente";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCliente", idCliente);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pregunta = reader["PreguntaSeguridad2"].ToString();
                        }
                    }
                }
            }

            return pregunta;
        }

        private string ObtenerRespuestaSeguridad(string email, string preguntaSeleccionada)
        {
            string respuesta = null;
            string query = $"SELECT {preguntaSeleccionada} FROM Clientes WHERE email = @Email";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            respuesta = reader[preguntaSeleccionada].ToString();
                        }
                    }
                }
            }

            return respuesta;
        }

        //private const int MaxIntentosFallidos = 3; // Máximo de intentos fallidos permitidos
        //    private const int TiempoBloqueoMinutos = 2; // Tiempo de bloqueo en minutos

        //    // Método para mostrar la vista de inicio de sesión
        //    public ActionResult InicioSesion()
        //    {
        //        return View();
        //    }

        //// Método para procesar el inicio de sesión
        //[HttpPost]
        //public ActionResult InicioSesion(string email, string contraseña)
        //{
        //    // Verificar si el usuario está bloqueado
        //    if (UsuarioBloqueado(email))
        //    {
        //        AuditoriaHelper.RegistrarAccion(email, "Intento de Inicio de Sesión - Usuario Bloqueado", $"El usuario está bloqueado. Inténtalo nuevamente después de {TiempoBloqueoMinutos} minutos.");
        //        ModelState.AddModelError("", $"El usuario está bloqueado. Inténtalo nuevamente después de {TiempoBloqueoMinutos} minutos.");
        //        return View();
        //    }

        //    string encryptedPassword = EncryptPassword(contraseña); // Cifrar la contraseña ingresada

        //    if (VerificarCredenciales(email, encryptedPassword))
        //    {
        //        // Reiniciar los intentos fallidos
        //        ReiniciarIntentosFallidos(email);

        //        // Configurar el ticket de autenticación manualmente
        //        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
        //            1, // Versión del ticket
        //            email, // Nombre del usuario asociado al ticket
        //            DateTime.Now, // Fecha y hora de emisión
        //            DateTime.Now.AddMinutes(30), // Fecha y hora de expiración
        //            false, // Si la cookie debe ser persistente
        //            String.Empty, // Datos de usuario (puede ser una cadena con información adicional)
        //            FormsAuthentication.FormsCookiePath); // Ruta de la cookie

        //        // Encriptar el ticket
        //        string encryptedTicket = FormsAuthentication.Encrypt(ticket);

        //        // Crear la cookie
        //        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
        //        Response.Cookies.Add(authCookie);

        //        // Obtener el rol del usuario
        //        string rol = ObtenerRolUsuario(email);
        //        // Registrar acción de auditoría de inicio de sesión exitoso
        //        AuditoriaHelper.RegistrarAccion(email, "Inicio de Sesión Exitoso", "El usuario inició sesión correctamente.");


        //        // Redirigir según el rol del usuario
        //        if (rol == "Admin")
        //        {
        //            return RedirectToAction("Index", "HomeAnalistaDatos");
        //        }
        //        else if (rol == "User")
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }

        //        TempData["Mensaje"] = "¡Inicio de sesión exitoso!";
        //    }

        //    // Incrementar los intentos fallidos y verificar si se ha alcanzado el máximo
        //    IncrementarIntentosFallidos(email);
        //    int intentosRestantes = MaxIntentosFallidos - ObtenerIntentosFallidos(email);

        //    if (intentosRestantes > 0)
        //    {
        //        AuditoriaHelper.RegistrarAccion(email, "Intento de Inicio de Sesión Fallido", $"El email o la contraseña son incorrectos. Te quedan {intentosRestantes} intentos.");
        //        ModelState.AddModelError("", $"El email o la contraseña son incorrectos. Te quedan {intentosRestantes} intentos.");
        //    }
        //    else
        //    {
        //        BloquearUsuario(email);
        //        AuditoriaHelper.RegistrarAccion(email, "Usuario Bloqueado", $"El usuario ha sido bloqueado por {TiempoBloqueoMinutos} minutos debido a múltiples intentos fallidos.");
        //        ModelState.AddModelError("", $"El usuario ha sido bloqueado por {TiempoBloqueoMinutos} minutos debido a múltiples intentos fallidos.");
        //    }

        //    return View();
        //}


        //// Método para verificar las credenciales del usuario
        //private bool VerificarCredenciales(string email, string contraseña)
        //{
        //    string query = "SELECT COUNT(*) FROM Clientes WHERE email = @Email AND contraseña = @Contraseña";
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("@Email", email);
        //            command.Parameters.AddWithValue("@Contraseña", contraseña);
        //            connection.Open();
        //            int count = (int)command.ExecuteScalar();
        //            return count > 0;
        //        }
        //    }
        //}


        private const int MaxIntentosFallidos = 3; // Máximo de intentos fallidos permitidos
        private const int TiempoBloqueoMinutos = 2; // Tiempo de bloqueo en minutos

        // Método para mostrar la vista de inicio de sesión
        public ActionResult InicioSesion()
        {
            return View();
        }

        // Método para procesar el inicio de sesión
        [HttpPost]
        public ActionResult InicioSesion(string email, string contraseña)
        {
            // Verificar si el usuario está bloqueado
            if (UsuarioBloqueado(email))
            {
                ModelState.AddModelError("", $"El usuario está bloqueado. Inténtalo nuevamente después de {TiempoBloqueoMinutos} minutos.");
                return View();
            }

            string encryptedPassword = EncryptPassword(contraseña); // Cifrar la contraseña ingresada

            if (VerificarCredenciales(email, encryptedPassword))
            {
                // Reiniciar los intentos fallidos
                ReiniciarIntentosFallidos(email);

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




                // Generar y guardar el token
                string token = TokenGenerator.GenerarTokensSeguridad(10); // Genera un token de 10 caracteres
                string encryptedToken = EncryptionHelper.Encrypt(token); // Cifrar el token antes de guardarlo
                GuardarToken(ObtenerIdClientePorEmail(email), encryptedToken, DateTime.Now);

                // Enviar el token por correo electrónico
                EnviarCorreoRecuperacion2FA(email, token);

                // Registrar auditoría de inicio de sesión exitoso
                try
                {
                    AuditoriaHelper.RegistrarAccion(email, "Inicio de Sesión Exitoso", $"El usuario inició sesión correctamente el {DateTime.Now}.");
                }
                catch (Exception ex)
                {
                    // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                    throw new Exception("Error al registrar auditoría de inicio de sesión exitoso.", ex);
                }

                // Redirigir a la vista para verificar el token
                return RedirectToAction("VerificarToken2FA", new { email = email });
            }

            // Incrementar los intentos fallidos y verificar si se ha alcanzado el máximo
            IncrementarIntentosFallidos(email);
            int intentosRestantes = MaxIntentosFallidos - ObtenerIntentosFallidos(email);

            if (intentosRestantes > 0)
            {
                ModelState.AddModelError("", $"El email o la contraseña son incorrectos. Te quedan {intentosRestantes} intentos.");
            }
            else
            {
                BloquearUsuario(email);
                ModelState.AddModelError("", $"El usuario ha sido bloqueado por {TiempoBloqueoMinutos} minutos debido a múltiples intentos fallidos.");
            }

            return View();
        }

        // Método para verificar las credenciales del usuario
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


        // Método para obtener el rol del usuario por email
        private string ObtenerRolUsuario(string email)
        {
            string query = "SELECT R.nombre FROM Roles R INNER JOIN UsuarioRoles UR ON R.idRol = UR.idRol INNER JOIN Clientes C ON UR.idUsuario = C.idCliente WHERE C.email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    string rol = (string)command.ExecuteScalar();
                    return rol;
                }
            }
        }


        // Método para verificar si el usuario está bloqueado
        private bool UsuarioBloqueado(string email)
        {
            DateTime? fechaBloqueo = ObtenerFechaBloqueo(email);

            if (fechaBloqueo.HasValue && fechaBloqueo > DateTime.Now)
            {
                return true; // Usuario bloqueado
            }

            return false; // Usuario no bloqueado
        }

        // Método para obtener la fecha de bloqueo del usuario desde la base de datos
        private DateTime? ObtenerFechaBloqueo(string email)
        {
            string query = "SELECT tiempoBloqueo FROM Clientes WHERE email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return (DateTime)result;
                    }
                    return null;
                }
            }
        }

        // Método para bloquear al usuario
        private void BloquearUsuario(string email)
        {
            DateTime fechaDesbloqueo = DateTime.Now.AddMinutes(TiempoBloqueoMinutos);
            string query = "UPDATE Clientes SET tiempoBloqueo = @tiempoBloqueo WHERE email = @Email";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tiempoBloqueo", fechaDesbloqueo);
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            try
            {
                // Registrar la acción de bloqueo en la tabla de auditoría
                AuditoriaHelper.RegistrarAccion(email, "Bloqueo de Usuario", "El usuario fue bloqueado debido a múltiples intentos fallidos.");
            }
            catch (Exception ex)
            {
                // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                throw new Exception("Error al registrar auditoría para el bloqueo de usuario.", ex);
            }
        }


        // Método para incrementar los intentos fallidos
        private void IncrementarIntentosFallidos(string email)
        {
            int intentosActuales = ObtenerIntentosFallidos(email);
            intentosActuales++;

            if (intentosActuales > MaxIntentosFallidos)
            {
                BloquearUsuario(email); // Bloquear usuario si se superan los intentos fallidos
            }
            else
            {
                string query = "UPDATE Clientes SET IntentosFallidos = @IntentosFallidos WHERE email = @Email";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IntentosFallidos", intentosActuales);
                        command.Parameters.AddWithValue("@Email", email);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // Método para reiniciar los intentos fallidos
        private void ReiniciarIntentosFallidos(string email)
        {
            string query = "UPDATE Clientes SET IntentosFallidos = 0, tiempoBloqueo = NULL WHERE email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método para obtener los intentos fallidos actuales
        private int ObtenerIntentosFallidos(string email)
        {
            string query = "SELECT IntentosFallidos FROM Clientes WHERE email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return (int)result;
                    }
                    return 0;
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





        //Controlador del recuperar contraseña.  
        public static class EncryptionHelper
        {
            private static readonly byte[] Key = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };
            private static readonly byte[] IV = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };

            public static string Encrypt(string plainText)
            {
                byte[] encryptedBytes;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            encryptedBytes = msEncrypt.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(encryptedBytes);
            }

            public static string Decrypt(string cipherText)
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }


        public class TokenGenerator
        {
            public static string GenerateToken(int length)
            {
                const string allowedChars = "123456789";
                byte[] randomBytes = new byte[length];

                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(randomBytes);
                }

                char[] chars = new char[length];
                int allowedCharsCount = allowedChars.Length;

                for (int i = 0; i < length; i++)
                {
                    chars[i] = allowedChars[randomBytes[i] % allowedCharsCount];
                }

                return new string(chars);
            }

            public static string GenerarTokensSeguridad(int length)
            {
                const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                byte[] randomBytes = new byte[length];

                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(randomBytes);
                }

                char[] chars = new char[length];
                int allowedCharsCount = allowedChars.Length;

                for (int i = 0; i < length; i++)
                {
                    chars[i] = allowedChars[randomBytes[i] % allowedCharsCount];
                }

                return new string(chars);
            }
        }



        public ActionResult VerificarToken()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerificarToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "Token inválido.";
                return View();
            }

            string email = ObtenerEmailDesdeToken(token);
            if (email != null)
            {
                TempData["Token"] = token; // Guardar el token temporalmente
                return RedirectToAction("RecuperarContrasena");
            }
            else
            {
                ViewBag.ErrorMessage = "El token es inválido o ha expirado.";
                return View();
            }
        }



        public ActionResult VerificarToken2FA()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerificarToken2FA(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "Token inválido.";
                return View();
            }

            string email = ObtenerEmailDesdeToken(token);

            if (email != null)
            {
                TempData["Token"] = token; // Guardar el token temporalmente

                // Obtener el rol del usuario
                string rol = ObtenerRolUsuario(email);

                // Redirigir según el rol del usuario
                if (rol == "Admin")
                {
                    return RedirectToAction("Index", "HomeAnalistaDatos");
                }
                else if (rol == "User")
                {
                    return RedirectToAction("Index", "Home");
                }

                // Si el rol no coincide con ninguno, redirigir a una vista de error o una vista por defecto
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "El token es inválido o ha expirado.";
                return View();
            }
        }



        // GET: Cuenta/OlvideMiContrasena
        public ActionResult OlvideMiContrasena()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OlvideMiContrasena(string email)
        {
            int idCliente = ObtenerIdClientePorEmail(email);
            if (idCliente != 0)
            {
                // Seleccionar al azar una pregunta de seguridad
                Random random = new Random();
                int preguntaIndex = random.Next(1, 3); // 1 o 2

                string pregunta = preguntaIndex == 1 ? ObtenerPreguntaSeguridad1(idCliente) : ObtenerPreguntaSeguridad2(idCliente);
                if (pregunta == null)
                {
                    ViewBag.ErrorMessage = "No se encontraron preguntas de seguridad asociadas a este correo electrónico.";
                    return View();
                }

                // Guardar la pregunta seleccionada en TempData
                TempData["PreguntaSeguridad"] = pregunta;
                TempData["Email"] = email;

                // Redirigir a la vista de responder pregunta de seguridad
                return RedirectToAction("ResponderPreguntaSeguridad");
            }
            else
            {
                ViewBag.ErrorMessage = "El correo electrónico proporcionado no está registrado.";
            }

            return View();
        }

        private int ObtenerIdClientePorEmail(string email)
        {
            string query = "SELECT idCliente FROM Clientes WHERE email = @Email";
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
                    return 0;
                }
            }
        }

        public ActionResult RecuperarContrasena()
        {
            if (TempData["Token"] == null)
            {
                return RedirectToAction("VerificarToken");
            }

            ViewBag.Token = TempData["Token"].ToString();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasena(string token, string newPassword, string confirmNewPassword)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "Token inválido.";
                return View();
            }

            if (newPassword != confirmNewPassword)
            {
                ViewBag.ErrorMessage = "Las contraseñas no coinciden.";
                return View();
            }

            if (!EsContrasenaValida(newPassword))
            {
                ViewBag.ErrorMessage = "La contraseña debe tener entre 14 y 20 caracteres, incluir al menos una letra minúscula, una mayúscula, un dígito y un carácter especial.";
                return View();
            }

            string email = ObtenerEmailDesdeToken(token);
            if (email != null)
            {
                ActualizarContrasena(email, newPassword);
                ViewBag.Message = "Su contraseña ha sido actualizada con éxito.";
            }
            else
            {
                ViewBag.ErrorMessage = "El token es inválido o ha expirado.";
            }

            return View();
        }

        private bool EsContrasenaValida(string contraseña)
        {
            // La contraseña debe tener entre 14 y 20 caracteres
            if (contraseña.Length < 14 || contraseña.Length > 20)
            {
                return false;
            }

            // La contraseña debe tener al menos una letra minúscula, una mayúscula, un dígito y un carácter especial
            bool tieneMinuscula = contraseña.Any(c => char.IsLower(c));
            bool tieneMayuscula = contraseña.Any(c => char.IsUpper(c));
            bool tieneDigito = contraseña.Any(c => char.IsDigit(c));
            bool tieneCaracterEspecial = contraseña.Any(c => "!@#$%^&*()_+<>?[]{}|".Contains(c));

            return tieneMinuscula && tieneMayuscula && tieneDigito && tieneCaracterEspecial;
        }

        private void ActualizarContrasena(string email, string newPassword)
        {
            string query = "UPDATE Clientes SET contraseña = @NuevaContrasena WHERE email = @Email";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@NuevaContrasena", EncryptionHelper.Encrypt(newPassword)); // Asegúrate de cifrar la nueva contraseña

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                // Marcar el token como usado
                MarcarTokenComoUsado(email);

                // Registrar acción de auditoría para la actualización de contraseña
                string ipAddress = Request.UserHostAddress; // Obtener la IP del usuario
                AuditoriaHelper.RegistrarAccion(email, "Actualización de Contraseña", "Se actualizó la contraseña del usuario.", ipAddress);
            }
            catch (Exception ex)
            {
                // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                throw new Exception("Error al actualizar la contraseña.", ex);
            }
        }

        private void MarcarTokenComoUsado(string email)
        {
            string query = "UPDATE Tokens SET Estado = 1 WHERE IdCliente = (SELECT idCliente FROM Clientes WHERE email = @Email) AND Estado = 0";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void GuardarToken(int idCliente, string token, DateTime fechaCreacion)
        {
            string query = "INSERT INTO Tokens (IdCliente, Token, FechaCreacion, Estado) VALUES (@IdCliente, @Token, @FechaCreacion, 0)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCliente", idCliente);
                    command.Parameters.AddWithValue("@Token", token);
                    command.Parameters.AddWithValue("@FechaCreacion", fechaCreacion);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private string ObtenerEmailDesdeToken(string token)
        {
            string query = "SELECT c.email, t.FechaCreacion FROM Tokens t INNER JOIN Clientes c ON t.IdCliente = c.idCliente WHERE t.Token = @Token AND t.Estado = 0";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", EncryptionHelper.Encrypt(token)); // Asegúrate de cifrar el token

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DateTime fechaCreacion = (DateTime)reader["FechaCreacion"];
                            if (DateTime.Now.Subtract(fechaCreacion).TotalMinutes <= 30) // Verifica si el token no ha expirado (30 minutos de validez)
                            {
                                return reader["email"].ToString();
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void EnviarCorreoRecuperacion(string email, string token)
        {
            string remitente = "estebangomez1015@gmail.com";
            string contraseña = "dzmoqbkgfzfqquwf";
            string asunto = "Recuperación de contraseña";
            string cuerpo = $"Para recuperar su contraseña, use el siguiente token: {token}";

            try
            {
                using (SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    clienteSmtp.EnableSsl = true;
                    clienteSmtp.Credentials = new NetworkCredential(remitente, contraseña);

                    using (MailMessage mensaje = new MailMessage(remitente, email, asunto, cuerpo))
                    {
                        mensaje.IsBodyHtml = true;
                        clienteSmtp.Send(mensaje);
                    }
                }

                // Registrar acción de auditoría para el envío de correo de recuperación de contraseña
                string ipAddress = Request.UserHostAddress; // Obtener la IP del usuario
                AuditoriaHelper.RegistrarAccion(email, "Correo de Recuperación de Contraseña Enviado", $"Se envió un correo de recuperación de contraseña con el token: {token}", ipAddress);
            }
            catch (Exception ex)
            {
                // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                throw new Exception("Error al enviar el correo de recuperación de contraseña.", ex);
            }
        }

        private void EnviarCorreoRecuperacion2FA(string email, string token)
        {
            string remitente = "estebangomez1015@gmail.com";
            string contraseña = "dzmoqbkgfzfqquwf";
            string asunto = "Doble factor de autenticación";
            string cuerpo = $"Para confirmar el inicio de sesión, ingrese el siguiente código: {token}";

            try
            {
                using (SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    clienteSmtp.EnableSsl = true;
                    clienteSmtp.Credentials = new NetworkCredential(remitente, contraseña);

                    using (MailMessage mensaje = new MailMessage(remitente, email, asunto, cuerpo))
                    {
                        mensaje.IsBodyHtml = true;
                        clienteSmtp.Send(mensaje);
                    }
                }

                // Registrar acción de auditoría para el envío de correo de recuperación 2FA
                string ipAddress = Request.UserHostAddress; // Obtener la IP del usuario
                AuditoriaHelper.RegistrarAccion(email, "Correo de Recuperación 2FA Enviado", $"Se envió un correo de recuperación 2FA con el token: {token}", ipAddress);
            }
            catch (Exception ex)
            {
                // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                throw new Exception("Error al enviar el correo de recuperación 2FA.", ex);
            }
        }


        private string EncryptPassword(string password)
        {
            // Utiliza EncryptionHelper para cifrar la contraseña
            return EncryptionHelper.Encrypt(password);
        }

        public static class AuditoriaHelper
        {
            private static string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

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
    }
}