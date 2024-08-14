using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace MotoRiders.CR.Controllers
{
    public class TalleresController : Controller
    {
        //private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";
        private static string connectionString = "Data Source=138.59.135.33\\MSSQLSERVER2019;Initial Catalog=motoridersbd;User ID=motoridersbd;Password=Cmotoridersbd*2024;";

        // GET: Talleres
        public ActionResult Index()
        {
            List<Talleres> talleres = new List<Talleres>();

            // Consulta SQL para recuperar datos de la tabla Talleres
            string query = "SELECT * FROM Talleres"; // Puedes ajustar la consulta según tus necesidades

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Talleres taller = new Talleres();
                            taller.Id = Convert.ToInt32(reader["id"]);
                            taller.Nombre = reader["nombre"].ToString();
                            taller.Ubicacion = reader["ubicacion"].ToString();
                            taller.Horarios = reader["horarios"].ToString();
                            taller.Telefono = reader["telefono"].ToString();
                            taller.Latitude = reader["latitude"] != DBNull.Value ? Convert.ToSingle(reader["latitude"]) : 0;
                            taller.Longitude = reader["longitude"] != DBNull.Value ? Convert.ToSingle(reader["longitude"]) : 0;
                            talleres.Add(taller);
                        }
                    }
                }
            }

            // Pasar los datos a la vista
            return View(talleres);
        }
    }
}
