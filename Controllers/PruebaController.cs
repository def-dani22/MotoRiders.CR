﻿using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace MotoRiders.CR.Controllers
{
    public class PruebaController : Controller
    {
        //private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";
        private static string connectionString = "Data Source=138.59.135.33\\MSSQLSERVER2019;Initial Catalog=motoridersbd;User ID=motoridersbd;Password=Cmotoridersbd*2024;";

        public ActionResult Nueva()
        {
            ViewBag.TipoCotizacionList = ObtenerListaTipoCotizacion();
            ViewBag.ProductoList = ObtenerListaProductos();
            ViewBag.EstadoCivilList = ObtenerListaEstadoCivil();
            ViewBag.TipoViviendaList = ObtenerListaTipoVivienda();

            var modelo = new Cotizacion();
            return View(modelo);
        }


        public ActionResult Confirmacion()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }


        [HttpPost]
        public ActionResult Index(Cotizacion cotizacion)
        {
            try
            {
                int idCliente = ObtenerIdClienteDesdeSesion();

                // Verifica si se encontró un idCliente válido
                if (idCliente <= 0)
                {
                    ViewBag.ErrorMessage = "No se pudo obtener el cliente asociado al usuario actual. Por favor, inténtelo de nuevo más tarde.";
                    ViewBag.TipoCotizacionList = ObtenerListaTipoCotizacion();
                    ViewBag.ProductoList = ObtenerListaProductos();
                    ViewBag.EstadoCivilList = ObtenerListaEstadoCivil();
                    ViewBag.TipoViviendaList = ObtenerListaTipoVivienda();
                    return View("Nueva", cotizacion);
                }

                GuardarCotizacion(cotizacion, idCliente);

                TempData["SuccessMessage"] = "Su cotización ha sido recibida con éxito. Pronto uno de nuestros asesores se contactará con usted.";
                return RedirectToAction("Confirmacion");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al procesar su solicitud. Por favor, inténtelo de nuevo más tarde.";
                ViewBag.ExceptionMessage = ex.Message;
                ViewBag.TipoCotizacionList = ObtenerListaTipoCotizacion();
                ViewBag.ProductoList = ObtenerListaProductos();
                ViewBag.EstadoCivilList = ObtenerListaEstadoCivil();
                ViewBag.TipoViviendaList = ObtenerListaTipoVivienda();
                return View("Nueva", cotizacion);
            }
        }



        // Método para guardar la cotización en la base de datos
        private void GuardarCotizacion(Cotizacion cotizacion, int idCliente)
        {
            string query = "INSERT INTO Cotizaciones (idTipoCotizacion, idCliente, idProducto, estadoCivil, tipoVivienda, tiempoDomicilio, empresaTrabaja, tiempoEmpresa, salarioMensualAproximado, numeroEmpresa) " +
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




        private int ObtenerIdClienteDesdeSesion()
        {
            string email = User.Identity.Name;
            string query = "SELECT idCliente FROM Clientes WHERE email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    return -1; // Cambia este valor por uno que indique que no se encontró el cliente
                }
            }
        }


        private List<SelectListItem> ObtenerListaTipoCotizacion()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT idTipoCotizacion, nombreTipoCotizacion FROM TipoCotizacion";
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
                            Value = reader["idTipoCotizacion"].ToString(),
                            Text = reader["nombreTipoCotizacion"].ToString()
                        });
                    }
                    reader.Close();
                }
            }
            return lista;
        }

        private List<SelectListItem> ObtenerListaProductos()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT id, nombre FROM Productos WHERE tipo = 'MOTO' OR tipo = 'MOTOS ELECTRICAS' OR tipo = 'CUADRACICLO'";
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
                            Value = reader["id"].ToString(),
                            Text = reader["nombre"].ToString()
                        });
                    }
                    reader.Close();
                }
            }
            return lista;
        }

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
    }
}
