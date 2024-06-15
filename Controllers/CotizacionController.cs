//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using MotoRiders.CR.Models;
//using System.Data.SqlClient;


//namespace MotoRiders.CR.Controllers
//{
//    public class CotizacionController : Controller
//    {
//        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

//        // GET: Cotizacion/Create
//        public ActionResult Create()
//        {
//            var model = new PruebaManejo();

//            // Populate TipoCotizacion dropdown
//            model.TiposCotizacion = GetTiposCotizacion();

//            // Populate Producto dropdown
//            model.Productos = GetProductos();

//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Create(PruebaManejo model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Get the logged-in client's ID
//                int idCliente = GetLoggedClientId();

//                // Insert the new Cotizacion record
//                using (SqlConnection connection = new SqlConnection(connectionString))
//                {
//                    string query = @"INSERT INTO Cotizaciones 
//                                    (idTipoCotizacion, idCliente, idProducto, estadoCivil, tipoVivienda, tiempoDomicilio, empresaTrabaja, tiempoEmpresa, salarioMensualAproximado, numeroEmpresa)
//                                    VALUES (@idTipoCotizacion, @idCliente, @idProducto, @estadoCivil, @tipoVivienda, @tiempoDomicilio, @empresaTrabaja, @tiempoEmpresa, @salarioMensualAproximado, @numeroEmpresa)";

//                    using (SqlCommand command = new SqlCommand(query, connection))
//                    {
//                        command.Parameters.AddWithValue("@idTipoCotizacion", model.IdTipoCotizacion);
//                        command.Parameters.AddWithValue("@idCliente", idCliente);
//                        command.Parameters.AddWithValue("@idProducto", model.IdProducto);
//                        command.Parameters.AddWithValue("@estadoCivil", model.EstadoCivil);
//                        command.Parameters.AddWithValue("@tipoVivienda", model.TipoVivienda);
//                        command.Parameters.AddWithValue("@tiempoDomicilio", model.TiempoDomicilio);
//                        command.Parameters.AddWithValue("@empresaTrabaja", model.EmpresaTrabaja);
//                        command.Parameters.AddWithValue("@tiempoEmpresa", model.TiempoEmpresa);
//                        command.Parameters.AddWithValue("@salarioMensualAproximado", model.SalarioMensualAproximado);
//                        command.Parameters.AddWithValue("@numeroEmpresa", model.NumeroEmpresa);

//                        connection.Open();
//                        command.ExecuteNonQuery();
//                    }
//                }

//                return RedirectToAction("Index", "Home");
//            }

//            // Repopulate dropdown lists if the model state is invalid
//            model.TiposCotizacion = GetTiposCotizacion();
//            model.Productos = GetProductos();

//            return View(model);
//        }

//        private List<SelectListItem> GetTiposCotizacion()
//        {
//            var tiposCotizacion = new List<SelectListItem>();
//            using (SqlConnection connection = new SqlConnection(connectionString))
//            {
//                string query = "SELECT idTipoCotizacion, nombreTipoCotizacion FROM TipoCotizacion";
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    connection.Open();
//                    using (SqlDataReader reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            tiposCotizacion.Add(new SelectListItem
//                            {
//                                Value = reader["idTipoCotizacion"].ToString(),
//                                Text = reader["nombreTipoCotizacion"].ToString()
//                            });
//                        }
//                    }
//                }
//            }
//            return tiposCotizacion;
//        }

//        private List<SelectListItem> GetProductos()
//        {
//            var productos = new List<SelectListItem>();
//            using (SqlConnection connection = new SqlConnection(connectionString))
//            {
//                string query = @"SELECT id, nombre FROM Productos 
//                                 WHERE tipo IN ('REPUESTOS', 'MOTOS ELECTRICAS', 'CUADRACICLO', 'MOTO')";
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    connection.Open();
//                    using (SqlDataReader reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            productos.Add(new SelectListItem
//                            {
//                                Value = reader["id"].ToString(),
//                                Text = reader["nombre"].ToString()
//                            });
//                        }
//                    }
//                }
//            }
//            return productos;
//        }

//        private int GetLoggedClientId()
//        {
//            // Suponiendo que tienes una manera de obtener el ID del cliente logueado.
//            // Puedes obtenerlo del contexto de la sesión o como lo manejes en tu aplicación.
//            return Convert.ToInt32(Session["idCliente"]);
//        }
//    }
//}
