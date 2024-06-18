using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MotoRiders.CR.Models;

namespace MotoRiders.CR.Controllers
{
    public class CarritoCompraController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        // GET: CarritoCompra
        public ActionResult Index()
        {
            List<Producto> productosEnCarrito = new List<Producto>();

            // Obtener el carrito de compras del usuario actual
            var clienteId = ObtenerIdClienteDesdeSesion(); // Implementa esta función según la lógica de tu aplicación

            string query = $"SELECT P.* FROM Productos P " +
                           $"INNER JOIN ProductoCarrito PC ON P.id = PC.idProducto " +
                           $"INNER JOIN Orden O ON PC.idOrden = O.idOrden " +
                           $"WHERE O.idCliente = {clienteId}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
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
                            producto.PrecioAlquiler = reader["precioAlquiler"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["precioAlquiler"]);
                            productosEnCarrito.Add(producto);
                        }
                    }
                }
            }

            // Pasar los datos a la vista
            return View(productosEnCarrito);
        }

        [HttpPost]
        public ActionResult AgregarAlCarrito(int id)
        {
            var clienteId = ObtenerIdClienteDesdeSesion();

            // Verificar si el producto ya está en el carrito del cliente
            string verificarExistenciaQuery = $"SELECT COUNT(*) FROM ProductoCarrito WHERE idOrden IN (SELECT idOrden FROM Orden WHERE idCliente = {clienteId}) AND idProducto = {id}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(verificarExistenciaQuery, connection))
                {
                    int count = (int)command.ExecuteScalar();

                    if (count == 0)
                    {
                        // Agregar el producto al carrito del cliente
                        string insertQuery = $"INSERT INTO ProductoCarrito (idOrden, idProducto, cantidad, precioUnitario) " +
                                             $"VALUES ((SELECT idOrden FROM Orden WHERE idCliente = {clienteId} AND estadoOrden = 'creado'), {id}, 1, " +
                                             $"(SELECT precioVenta FROM Productos WHERE id = {id}))";

                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Incrementar la cantidad del producto en el carrito del cliente
                        string updateQuery = $"UPDATE ProductoCarrito SET cantidad = cantidad + 1 " +
                                             $"WHERE idOrden IN (SELECT idOrden FROM Orden WHERE idCliente = {clienteId} AND estadoOrden = 'creado') " +
                                             $"AND idProducto = {id}";

                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RealizarCompra()
        {
            var clienteId = ObtenerIdClienteDesdeSesion();

            // Obtener el id de la orden en estado 'creado' del cliente
            string obtenerOrdenQuery = $"SELECT idOrden FROM Orden WHERE idCliente = {clienteId} AND estadoOrden = 'creado'";

            int idOrden;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(obtenerOrdenQuery, connection))
                {
                    idOrden = (int)command.ExecuteScalar();
                }
            }

            // Obtener todos los productos en el carrito del cliente
            string obtenerProductosQuery = $"SELECT PC.*, P.* FROM ProductoCarrito PC " +
                                           $"INNER JOIN Productos P ON PC.idProducto = P.id " +
                                           $"WHERE PC.idOrden = {idOrden}";

            List<ProductoCarrito> productosCarrito = new List<ProductoCarrito>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(obtenerProductosQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductoCarrito productoCarrito = new ProductoCarrito();
                            productoCarrito.Id = Convert.ToInt32(reader["idProductoCarrito"]);
                            productoCarrito.ProductoId = Convert.ToInt32(reader["idProducto"]);
                            productoCarrito.Cantidad = Convert.ToInt32(reader["cantidad"]);

                            Producto producto = new Producto();
                            producto.Id = Convert.ToInt32(reader["idProducto"]);
                            producto.Img = reader["img"].ToString();
                            producto.Tipo = reader["tipo"].ToString();
                            producto.Nombre = reader["nombre"].ToString();
                            producto.Modelo = reader["modelo"].ToString();
                            producto.Color = reader["color"].ToString();
                            producto.Descripcion = reader["descripcion"].ToString();
                            producto.Caracteristicas = reader["caracteristicas"].ToString();
                            producto.PrecioVenta = Convert.ToDecimal(reader["precioVenta"]);
                            producto.PrecioAlquiler = reader["precioAlquiler"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["precioAlquiler"]);

                            productoCarrito.Producto = producto;

                            productosCarrito.Add(productoCarrito);
                        }
                    }
                }
            }

            // Lógica para generar la orden y procesar el pago (no implementada aquí)

            // Vaciar el carrito del cliente después de realizar la compra
            string vaciarCarritoQuery = $"DELETE FROM ProductoCarrito WHERE idOrden = {idOrden}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(vaciarCarritoQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index", "Home");
        }

        // Método para obtener el ID del cliente a partir del email almacenado en la sesión
        private int ObtenerIdClienteDesdeSesion()
        {
            // Obtener el email del usuario autenticado
            string email = User.Identity.Name;

            // Query para obtener el ID del cliente basado en el email
            string query = "SELECT idCliente FROM Clientes WHERE email = @Email";

            // Inicializar la conexión a la base de datos utilizando la cadena de conexión
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Crear un comando SQL con la consulta y la conexión
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Agregar el parámetro para el email
                    command.Parameters.AddWithValue("@Email", email);

                    // Abrir la conexión
                    connection.Open();

                    // Ejecutar el comando y obtener el resultado (ID del cliente)
                    object result = command.ExecuteScalar();

                    // Verificar si se encontró el cliente
                    if (result != null)
                    {
                        return Convert.ToInt32(result); // Devolver el ID del cliente encontrado
                    }

                    // Manejar el caso donde no se encontró el cliente (puedes manejarlo según tu lógica específica)
                    return -1; // Otra opción podría ser lanzar una excepción o devolver un valor especial
                }
            }
        }

    }
}
