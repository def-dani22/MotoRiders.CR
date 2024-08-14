using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace MotoRiders.CR.Controllers
{
    public class PruebaManejoController : Controller
    {
        //private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";
        private static string connectionString = "Data Source=138.59.135.33\\MSSQLSERVER2019;Initial Catalog=motoridersbd;User ID=motoridersbd;Password=Cmotoridersbd*2024;";

        public ActionResult Index()
        {
            ViewBag.ProductoList = ObtenerListaProductos();
            ViewBag.ContactoList = ObtenerListaContactos();
            return View(new PruebaManejo());
        }

        public ActionResult Confirmacion()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        [HttpPost]
        public ActionResult Index(PruebaManejo pruebaManejo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductoList = ObtenerListaProductos();
                ViewBag.ContactoList = ObtenerListaContactos();
                return View(pruebaManejo);
            }

            try
            {
                int idCliente = ObtenerIdClienteDesdeSesion();

                if (idCliente <= 0)
                {
                    ViewBag.ErrorMessage = "No se pudo obtener el cliente asociado al usuario actual. Por favor, inténtelo de nuevo más tarde.";
                    ViewBag.ProductoList = ObtenerListaProductos();
                    ViewBag.ContactoList = ObtenerListaContactos();
                    return View(pruebaManejo);
                }

                pruebaManejo.IdCliente = idCliente;
                GuardarPruebaManejo(pruebaManejo);

                TempData["SuccessMessage"] = "Su cotización ha sido recibida con éxito. Pronto uno de nuestros asesores se contactará con usted.";
                return RedirectToAction("Confirmacion");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al procesar su solicitud. Por favor, inténtelo de nuevo más tarde.";
                ViewBag.ExceptionMessage = ex.Message;
                ViewBag.ProductoList = ObtenerListaProductos();
                ViewBag.ContactoList = ObtenerListaContactos();
                return View(pruebaManejo);
            }
        }

        private void GuardarPruebaManejo(PruebaManejo pruebaManejo)
        {
            string query = "INSERT INTO PruebaManejo (IdCliente, IdProducto, IdContacto, Fecha, Hora) VALUES (@IdCliente, @IdProducto, @IdContacto, @Fecha, @Hora)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCliente", pruebaManejo.IdCliente);
                    command.Parameters.AddWithValue("@IdProducto", pruebaManejo.IdProducto);
                    command.Parameters.AddWithValue("@IdContacto", pruebaManejo.IdContacto);
                    command.Parameters.AddWithValue("@Fecha", pruebaManejo.Fecha);
                    command.Parameters.AddWithValue("@Hora", pruebaManejo.Hora);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
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
                    return -1;
                }
            }
        }

        private List<SelectListItem> ObtenerListaProductos()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT id, nombre FROM Productos WHERE tipo IN ('MOTO', 'MOTOS ELECTRICAS', 'CUADRACICLO')";
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

        private List<SelectListItem> ObtenerListaContactos()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT id, provincia FROM Contacto";
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
                            Text = reader["provincia"].ToString()
                        });
                    }
                    reader.Close();
                }
            }
            return lista;
        }
    }
}
