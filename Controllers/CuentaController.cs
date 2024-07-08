﻿using MotoRiders.CR.Models;
using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

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

                cliente.contraseña = EncryptionHelper.Encrypt(cliente.contraseña);
                InsertarCliente(cliente);

                return RedirectToAction("InicioSesion", "Cuenta");
            }

            return View(cliente);
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
            if (new Random().Next(2) == 0)
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
            // Encriptar los datos sensibles
            string encryptedPreguntaSeguridad1 = EncryptionHelper.Encrypt(cliente.preguntaSeguridad1);
            string encryptedRespuestaSeguridad1 = EncryptionHelper.Encrypt(cliente.respuestaSeguridad1);
            string encryptedPreguntaSeguridad2 = EncryptionHelper.Encrypt(cliente.preguntaSeguridad2);
            string encryptedRespuestaSeguridad2 = EncryptionHelper.Encrypt(cliente.respuestaSeguridad2);

            string query = "INSERT INTO Clientes (cedula, nombre, direccion, telefono, email, contraseña, preguntaSeguridad1, respuestaSeguridad1, preguntaSeguridad2, respuestaSeguridad2) " +
                           "VALUES (@Cedula, @Nombre, @Direccion, @Telefono, @Email, @Contraseña, @PreguntaSeguridad1, @RespuestaSeguridad1, @PreguntaSeguridad2, @RespuestaSeguridad2)";
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
                    command.Parameters.AddWithValue("@PreguntaSeguridad1", encryptedPreguntaSeguridad1);
                    command.Parameters.AddWithValue("@RespuestaSeguridad1", encryptedRespuestaSeguridad1);
                    command.Parameters.AddWithValue("@PreguntaSeguridad2", encryptedPreguntaSeguridad2);
                    command.Parameters.AddWithValue("@RespuestaSeguridad2", encryptedRespuestaSeguridad2);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


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
                return RedirectToAction("Index", "Home");
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
            FormsAuthentication.SignOut();
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
        }

        private string EncryptPassword(string password)
            {
                // Utiliza EncryptionHelper para cifrar la contraseña
                return EncryptionHelper.Encrypt(password);
            }
    }
}