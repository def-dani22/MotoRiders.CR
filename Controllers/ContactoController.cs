using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace MotoRiders.CR.Controllers
{
    public class ContactoController : Controller
    {
        //private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";
        private static string connectionString = "Data Source=138.59.135.33\\MSSQLSERVER2019;Initial Catalog=motoridersbd;User ID=motoridersbd;Password=Cmotoridersbd*2024;";

        // GET: Contactos
        public ActionResult Index()
        {
            List<Contacto> contactos = new List<Contacto>();

            // Consulta SQL para recuperar datos de la tabla Contacto
            string query = "SELECT * FROM Contacto";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Contacto contacto = new Contacto();
                            contacto.Id = Convert.ToInt32(reader["id"]);
                            contacto.Provincia = reader["provincia"].ToString();
                            contacto.Telefono = reader["telefono"].ToString();
                            contacto.Whatsapp = reader["whatsapp"].ToString();
                            contacto.Correo = reader["correo"].ToString();
                            contacto.Direccion = reader["direccion"].ToString();
                            contacto.Latitude = reader["latitude"] != DBNull.Value ? Convert.ToSingle(reader["latitude"]) : 0;
                            contacto.Longitude = reader["longitude"] != DBNull.Value ? Convert.ToSingle(reader["longitude"]) : 0;
                            contactos.Add(contacto);
                        }
                    }
                }
            }

            // Pasar los datos a la vista
            return View(contactos);
        }
    }
}
