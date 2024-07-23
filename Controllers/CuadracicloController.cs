using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace MotoRiders.CR.Controllers
{
    public class CuadracicloController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        // GET: Producto
        public ActionResult Index()
        {
            List<Producto> motos = new List<Producto>();

            // Consulta SQL para recuperar datos de la tabla Productos
            string query = "SELECT * FROM Productos WHERE tipo = 'CUADRACICLO'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Producto moto = new Producto();
                            moto.Id = Convert.ToInt32(reader["id"]);
                            moto.Img = reader["img"].ToString();
                            moto.Tipo = reader["tipo"].ToString();
                            moto.Nombre = reader["nombre"].ToString();
                            moto.Modelo = reader["modelo"].ToString();
                            moto.Color = reader["color"].ToString();
                            moto.Descripcion = reader["descripcion"].ToString();
                            moto.Caracteristicas = reader["caracteristicas"].ToString();
                            moto.PrecioVenta = Convert.ToDecimal(reader["precioVenta"]);
                            moto.PrecioAlquiler = Convert.ToDecimal(reader["precioAlquiler"]);
                            motos.Add(moto);
                        }
                    }
                }
            }

            // Pasar los datos a la vista
            return View(motos);
        }

        [HttpPost]
        public ActionResult AgregarProductosAlCarrito(List<int> productosSeleccionados)
        {
            if (productosSeleccionados != null && productosSeleccionados.Count > 0)
            {
                var clienteId = ObtenerIdClienteDesdeSesion();

                // Asegurar que el cliente tiene una orden en estado 'creado'
                int idOrden = ObtenerOcrearOrden(clienteId);

                foreach (var productoId in productosSeleccionados)
                {
                    // Verificar si el producto ya está en el carrito del cliente
                    string verificarExistenciaQuery = $"SELECT COUNT(*) FROM ProductoCarrito WHERE idOrden = {idOrden} AND idProducto = {productoId}";

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
                                                     $"VALUES ({idOrden}, {productoId}, 1, " +
                                                     $"(SELECT precioVenta FROM Productos WHERE id = {productoId}))";

                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                {
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Incrementar la cantidad del producto en el carrito del cliente
                                string updateQuery = $"UPDATE ProductoCarrito SET cantidad = cantidad + 1 " +
                                                     $"WHERE idOrden = {idOrden} AND idProducto = {productoId}";

                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                {
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }

            // Redirigir a la vista del carrito de compras
            return RedirectToAction("Index", "CarritoCompra");
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

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }

                    return -1;
                }
            }
        }

        private int ObtenerOcrearOrden(int clienteId)
        {
            string query = "SELECT idOrden FROM Orden WHERE idCliente = @ClienteId AND estadoOrden = 'creado'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClienteId", clienteId);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO Orden (idCliente, estadoOrden) OUTPUT INSERTED.idOrden VALUES (@ClienteId, 'creado')";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@ClienteId", clienteId);
                            return (int)insertCommand.ExecuteScalar();
                        }
                    }
                }
            }
        }
    }
}
