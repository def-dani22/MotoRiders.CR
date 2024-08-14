using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace MotoRiders.Controllers
{
    public class CarritoController : Controller
    {
        //private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";
        private static string connectionString = "Data Source=138.59.135.33\\MSSQLSERVER2019;Initial Catalog=motoridersbd;User ID=motoridersbd;Password=Cmotoridersbd*2024;";

        // Lista temporal para simular los elementos del carrito (debes reemplazar con tu lógica real)
        private static List<Producto> carrito = new List<Producto>();

        // Acción para mostrar el carrito de compras
        public ActionResult Index()
        {
            return View(carrito); // Devuelve la vista con la lista de elementos en el carrito
        }

        // Acción para agregar un elemento al carrito
        [HttpPost]
        public ActionResult AgregarAlCarrito(int id)
        {
            // Obtener los detalles del producto desde la base de datos
            Producto producto = ObtenerDetallesProducto(id);

            if (producto != null)
            {
                carrito.Add(producto); // Agregar el producto al carrito
                TempData["Mensaje"] = $"Se agregó {producto.Nombre} al carrito."; // Mensaje de éxito
            }
            else
            {
                TempData["Error"] = "No se pudo agregar el artículo al carrito."; // Mensaje de error
            }

            return RedirectToAction("Index"); // Redirigir al método Index para mostrar el carrito actualizado
        }

        // Método para obtener los detalles de un producto desde la base de datos
        private Producto ObtenerDetallesProducto(int id)
        {
            // Consulta SQL para recuperar datos de la tabla Productos filtrando por el id recibido
            string query = "SELECT * FROM Productos WHERE id = @id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id); // Agregar parámetro para evitar SQL Injection
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Producto producto = new Producto();
                            producto.Id = Convert.ToInt32(reader["id"]);
                            producto.Img = reader["img"].ToString();
                            producto.Tipo = reader["tipo"].ToString();
                            producto.Nombre = reader["nombre"].ToString();
                            producto.Modelo = reader["modelo"].ToString();
                            producto.Color = reader["color"].ToString();
                            producto.Descripcion = reader["descripcion"].ToString();
                            producto.Caracteristicas = reader["caracteristicas"].ToString();
                            producto.PrecioVenta = Convert.ToDecimal(reader["precioVenta"]);
                            producto.PrecioAlquiler = Convert.ToDecimal(reader["precioAlquiler"]);
                            return producto; // Devolver el objeto Producto encontrado
                        }
                    }
                }
            }

            return null; // Devolver null si no se encontró ningún producto con el id dado
        }
    }
}
