using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.Mvc;

namespace MotoRiders.CR.Controllers
{
    public class ProductoController : Controller
    {


        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        // GET: Producto
        public ActionResult Index()
        {

            List<Producto> motos = new List<Producto>();

            // Consulta SQL para recuperar datos de la tabla Motos
            string query = "SELECT * FROM Productos WHERE tipo = 'MOTO'";

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

        // GET: Home/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Home/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Home/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Home/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Home/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Home/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Home/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
