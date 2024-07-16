using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;


namespace MotoRiders.CR.Controllers
{
    public class CotizacionPersonalizadaController : Controller
    {
        // Cadena de conexión hacia tu base de datos
        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        // GET: CotizacionPersonalizada/Nueva
        public ActionResult Nueva()
        {
            // Obtener listas para los dropdowns desde la base de datos
            ViewBag.TipoCotizacionList = ObtenerListaTipoCotizacion();
            ViewBag.ProductoList = ObtenerListaProductos();
            ViewBag.EstadoCivilList = ObtenerListaEstadoCivil();
            ViewBag.TipoViviendaList = ObtenerListaTipoVivienda();

            // Crear modelo para la vista
            var modelo = new Cotizacion();
            return View(modelo);
        }

        // POST: CotizacionPersonalizada/Nueva
        [HttpPost]
        public ActionResult Nueva(Cotizacion cotizacion)
        {
            if (ModelState.IsValid)
            {
                // Obtener el ID del cliente desde la sesión
                int idCliente = ObtenerIdClienteDesdeSesion();

                // Guardar la cotización en la base de datos
                GuardarCotizacion(cotizacion, idCliente);

                // Redirigir a una página de confirmación u otra acción deseada
                return RedirectToAction("Index", "Home");
            }

            // Si el modelo no es válido, regresar a la vista con los datos y errores
            ViewBag.TipoCotizacionList = ObtenerListaTipoCotizacion();
            ViewBag.ProductoList = ObtenerListaProductos();
            ViewBag.EstadoCivilList = ObtenerListaEstadoCivil();
            ViewBag.TipoViviendaList = ObtenerListaTipoVivienda();

            return View(cotizacion);
        }

        // Método para obtener el ID del cliente a partir del email almacenado en la sesión
        private int ObtenerIdClienteDesdeSesion()
        {
            string email = User.Identity.Name; // Obtener el email del usuario autenticado
            string query = "SELECT IdCliente FROM Clientes WHERE email = @Email";
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
                    return -1; // En caso de no encontrar el cliente, manejar este caso según tu lógica
                }
            }
        }

        // Método para guardar la cotización en la base de datos
        private void GuardarCotizacion(Cotizacion cotizacion, int idCliente)
        {
            string query = "INSERT INTO Cotizaciones (IdTipoCotizacion, IdCliente, IdProducto, EstadoCivil, TipoVivienda, TiempoDomicilio, EmpresaTrabaja, TiempoEmpresa, SalarioMensualAproximado, NumeroEmpresa) " +
                           "VALUES (@IdTipoCotizacion, @IdCliente, @IdProducto, @EstadoCivil, @TipoVivienda, @TiempoDomicilio, @EmpresaTrabaja, @TiempoEmpresa, @SalarioMensualAproximado, @NumeroEmpresa)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdTipoCotizacion", cotizacion.IdTipoCotizacion);
                    command.Parameters.AddWithValue("@IdCliente", idCliente);
                    command.Parameters.AddWithValue("@IdProducto", cotizacion.IdProducto);
                    command.Parameters.AddWithValue("@EstadoCivil", cotizacion.EstadoCivil);
                    command.Parameters.AddWithValue("@TipoVivienda", cotizacion.TipoVivienda);
                    command.Parameters.AddWithValue("@TiempoDomicilio", cotizacion.TiempoDomicilio);
                    command.Parameters.AddWithValue("@EmpresaTrabaja", cotizacion.EmpresaTrabaja);
                    command.Parameters.AddWithValue("@TiempoEmpresa", cotizacion.TiempoEmpresa);
                    command.Parameters.AddWithValue("@SalarioMensualAproximado", cotizacion.SalarioMensualAproximado);
                    command.Parameters.AddWithValue("@NumeroEmpresa", cotizacion.NumeroEmpresa);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            // Registrar auditoría de guardado de cotización
            try
            {
                string descripcion = $"Se ha registrado una nueva cotización para el cliente con ID {idCliente}.";
                AuditoriaHelper.RegistrarAccion("Sistema", "Guardar Cotización", descripcion);
            }
            catch (Exception ex)
            {
                // Manejar la excepción adecuadamente (log, mensaje al usuario, etc.)
                throw new Exception("Error al registrar auditoría de guardado de cotización.", ex);
            }
        }

        // Método para obtener la lista de tipos de cotización desde la base de datos
        private List<SelectListItem> ObtenerListaTipoCotizacion()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT IdTipoCotizacion, Nombre FROM TiposCotizacion";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new SelectListItem
                        {
                            Value = reader["IdTipoCotizacion"].ToString(),
                            Text = reader["Nombre"].ToString()
                        });
                    }
                    reader.Close();
                }
            }
            return lista;
        }

        // Método para obtener la lista de productos (tipos de moto) desde la base de datos
        private List<SelectListItem> ObtenerListaProductos()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT IdProducto, Nombre FROM Productos WHERE Tipo = 'MOTO' OR Tipo = 'MOTOS ELECTRICAS' OR Tipo = 'CUADRACICLO'";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new SelectListItem
                        {
                            Value = reader["IdProducto"].ToString(),
                            Text = reader["Nombre"].ToString()
                        });
                    }
                    reader.Close();
                }
            }
            return lista;
        }

        // Método para obtener la lista de estados civiles
        private List<SelectListItem> ObtenerListaEstadoCivil()
        {
            var lista = new List<SelectListItem>
            {
                new SelectListItem { Value = "Soltero(a)", Text = "Soltero(a)" },
                new SelectListItem { Value = "Casado(a)", Text = "Casado(a)" },
                new SelectListItem { Value = "Divorciado(a)", Text = "Divorciado(a)" },
                new SelectListItem { Value = "Viudo(a)", Text = "Viudo(a)" },
                new SelectListItem { Value = "Separado(a)", Text = "Separado(a)" }
            };
            return lista;
        }

        // Método para obtener la lista de tipos de vivienda
        private List<SelectListItem> ObtenerListaTipoVivienda()
        {
            var lista = new List<SelectListItem>
            {
                new SelectListItem { Value = "Casa unifamiliar", Text = "Casa unifamiliar" },
                new SelectListItem { Value = "Apartamento", Text = "Apartamento" },
                new SelectListItem { Value = "Piso (Flat)", Text = "Piso (Flat)" },
                new SelectListItem { Value = "Casa móvil (Mobile Home)", Text = "Casa móvil (Mobile Home)" },
                new SelectListItem { Value = "Casa de campo (Cottage)", Text = "Casa de campo (Cottage)" }
            };
            return lista;
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
