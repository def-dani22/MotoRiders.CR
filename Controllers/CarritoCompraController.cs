using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
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
            var clienteId = ObtenerIdClienteDesdeSesion();

            string query = $"SELECT PC.*, P.* FROM ProductoCarrito PC " +
                           $"INNER JOIN Productos P ON PC.idProducto = P.id " +
                           $"WHERE PC.idOrden IN (SELECT idOrden FROM Orden WHERE idCliente = {clienteId} AND estadoOrden = 'creado')";

            List<ProductoCarrito> productosCarrito = new List<ProductoCarrito>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
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

            var carritoCompra = new CarritoCompra
            {
                ProductosCarrito = productosCarrito
            };

            return View(carritoCompra);
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
        public ActionResult RealizarCompra(CarritoCompra carritoCompra)
        {
            var clienteId = ObtenerIdClienteDesdeSesion();

            // Validar los datos de la tarjeta
            bool tarjetaValida = ValidarDatosTarjeta(carritoCompra);

            if (!tarjetaValida)
            {
                ModelState.AddModelError("", "Los datos de la tarjeta no son válidos.");
                return RedirectToAction("Index");
            }

            // Obtener el id de la orden en estado 'creado' del cliente
            int idOrden = ObtenerIdOrdenCreada(clienteId);

            // Obtener todos los productos en el carrito del cliente
            List<ProductoCarrito> productosCarrito = ObtenerProductosEnCarrito(idOrden);

            // Insertar el pago en la tabla Pagos
            int tipoPagoId = ObtenerTipoPagoIdPorNombre("Tarjeta"); // Ajusta según tus tipos de pago
            InsertarPago(tipoPagoId, carritoCompra.NombreTarjeta, carritoCompra.NumeroTarjeta, carritoCompra.FechaExpiracion, carritoCompra.CVV);

            // Lógica para procesar el pago
            ProcesarPago(idOrden, carritoCompra);

            // Eliminar los productos del carrito después de realizar la compra
            EliminarProductosDelCarrito(clienteId);

            return RedirectToAction("Index", "Home");
        }


        // Método para procesar el pago y realizar las acciones necesarias
        private void ProcesarPago(int idOrden, CarritoCompra carritoCompra)
        {
            // Validar los datos de la tarjeta con los datos existentes en la base de datos
            if (ValidarDatosTarjeta(carritoCompra))
            {
                // Actualizar el estado de la orden a "pagado"
                ActualizarEstadoOrden(idOrden, "pagado");

                // Guardar los datos de la compra en la tabla de registro de compras
                GuardarRegistroCompra(idOrden, carritoCompra.ClienteId);
            }
            else
            {
                throw new Exception("Los datos de la tarjeta no son válidos.");
            }
        }

        // Método para validar los datos de la tarjeta con los datos existentes en la base de datos
        private bool ValidarDatosTarjeta(CarritoCompra carritoCompra)
{
    string query = "SELECT COUNT(*) FROM Pagos WHERE " +
                   "numeroTarjeta = @NumeroTarjeta " +
                   "AND cvv = @CVV " +
                   "AND nombreTarjeta = @NombreTarjeta " +
                   "AND fechaExpiracion = @FechaExpiracion " +
                   "AND numeroCuenta = @NumeroCuenta"; // Añadimos número de cuenta

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@NumeroTarjeta", carritoCompra.NumeroTarjeta);
            command.Parameters.AddWithValue("@CVV", carritoCompra.CVV);
            command.Parameters.AddWithValue("@NombreTarjeta", carritoCompra.NombreTarjeta);
            command.Parameters.AddWithValue("@FechaExpiracion", carritoCompra.FechaExpiracion);
            command.Parameters.AddWithValue("@NumeroCuenta", carritoCompra.NumeroCuenta); // Añadimos número de cuenta

            connection.Open();
            int count = (int)command.ExecuteScalar();

            return count > 0;
        }
    }
}

        // Método para guardar los datos de la compra en una tabla de registro
        private void GuardarRegistroCompra(int idOrden, int clienteId)
        {
            string insertQuery = "INSERT INTO RegistroCompras (idOrden, idCliente, fechaCompra) " +
                                 "VALUES (@IdOrden, @IdCliente, GETDATE())";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@IdOrden", idOrden);
                    command.Parameters.AddWithValue("@IdCliente", clienteId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método para actualizar el estado de la orden
        private void ActualizarEstadoOrden(int idOrden, string nuevoEstado)
        {
            string updateQuery = $"UPDATE Orden SET estadoOrden = '{nuevoEstado}' WHERE idOrden = {idOrden}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método para validar los datos de la tarjeta
        private bool ValidarTarjeta(string nombreTarjeta, string numeroTarjeta, string fechaExpiracion, string cvv)
        {
            // Validaciones básicas
            if (string.IsNullOrEmpty(nombreTarjeta) || nombreTarjeta.Length > 100)
            {
                return false;
            }

            if (string.IsNullOrEmpty(numeroTarjeta) || numeroTarjeta.Length != 16 || !EsNumero(numeroTarjeta))
            {
                return false;
            }

            // Validar la fecha de expiración (formato MM/YY)
            if (!Regex.IsMatch(fechaExpiracion, @"^(0[1-9]|1[0-2])\/[0-9]{2}$"))
            {
                return false;
            }

            // CVV debe ser un número de 3 o 4 dígitos
            if (string.IsNullOrEmpty(cvv) || (cvv.Length != 3 && cvv.Length != 4) || !EsNumero(cvv))
            {
                return false;
            }

            return true;
        }

        // Método auxiliar para verificar si una cadena es numérica
        private bool EsNumero(string str)
        {
            return int.TryParse(str, out _);
        }

        // Método para insertar un registro de pago en la tabla Pagos
        private void InsertarPago(int tipoPagoId, string nombreTarjeta, string numeroTarjeta, string fechaExpiracion, string cvv)
        {
            string insertQuery = $"INSERT INTO PagosL (idTipoPagos, nombreTarjeta, numeroTarjeta, fechaExpiracion, cvv) " +
                                 $"VALUES (@TipoPagoId, @NombreTarjeta, @NumeroTarjeta, @FechaExpiracion, @CVV)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@TipoPagoId", tipoPagoId);
                    command.Parameters.AddWithValue("@NombreTarjeta", nombreTarjeta);
                    command.Parameters.AddWithValue("@NumeroTarjeta", numeroTarjeta);
                    command.Parameters.AddWithValue("@FechaExpiracion", fechaExpiracion);
                    command.Parameters.AddWithValue("@CVV", cvv);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método para obtener el ID del tipo de pago por nombre
        private int ObtenerTipoPagoIdPorNombre(string nombreTipoPago)
        {
            string query = "SELECT idTipoPagos FROM TipoPagos WHERE nombreTipoPagos = @NombreTipoPago";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NombreTipoPago", nombreTipoPago);

                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }

                    throw new Exception("Tipo de pago no encontrado."); // Puedes ajustar el manejo de errores según tus necesidades
                }
            }
        }

        // Método para obtener el ID del cliente a partir del email almacenado en la sesión
        private int ObtenerIdClienteDesdeSesion()
        {
            // Obtener el email del usuario autenticado
            string email = User.Identity.Name;

            // Query para obtener el ID del cliente basado en el email
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

                    throw new Exception("Cliente no encontrado."); // Puedes ajustar el manejo de errores según tus necesidades
                }
            }
        }

        // Método para obtener el ID de la orden en estado 'creado' del cliente
        private int ObtenerIdOrdenCreada(int clienteId)
        {
            string query = $"SELECT idOrden FROM Orden WHERE idCliente = {clienteId} AND estadoOrden = 'creado'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }

                    throw new Exception("Orden no encontrada."); // Puedes ajustar el manejo de errores según tus necesidades
                }
            }
        }

        // Método para obtener todos los productos en el carrito de la orden especificada
        private List<ProductoCarrito> ObtenerProductosEnCarrito(int idOrden)
        {
            string query = $"SELECT PC.*, P.* FROM ProductoCarrito PC " +
                           $"INNER JOIN Productos P ON PC.idProducto = P.id " +
                           $"WHERE PC.idOrden = {idOrden}";

            List<ProductoCarrito> productosCarrito = new List<ProductoCarrito>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
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

            return productosCarrito;
        }


        public ActionResult EliminarDelCarrito(int id)
        {
            var clienteId = ObtenerIdClienteDesdeSesion();

            // Query para eliminar el producto del carrito
            string deleteQuery = $"DELETE FROM ProductoCarrito WHERE idProductoCarrito = @Id AND idOrden IN (SELECT idOrden FROM Orden WHERE idCliente = @ClienteId AND estadoOrden = 'creado')";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ClienteId", clienteId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }




        // Método para eliminar todos los productos del carrito de compras del cliente
        private void EliminarProductosDelCarrito(int clienteId)
        {
            string deleteQuery = $"DELETE FROM ProductoCarrito WHERE idOrden IN (SELECT idOrden FROM Orden WHERE idCliente = {clienteId} AND estadoOrden = 'creado')";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
