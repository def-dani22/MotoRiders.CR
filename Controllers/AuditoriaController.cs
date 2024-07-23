//using MotoRiders.CR.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace MotoRiders.CR.Controllers
//{
//    public class AuditoriaController : Controller
//    {
//        // GET: Auditoria
//        public ActionResult Index()
//        {
//            return View();
//        }
//    }
//}





using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;


namespace MotoRiders.CR.Controllers
{

    public class AuditoriaController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-KNSONQV\\PUBLICADOR;Initial Catalog=motoriders;Integrated Security=True;";

        // GET: Repuestos
        public ActionResult Index()
        {
            List<Auditoria> auditorias = new List<Auditoria>();

            // Consulta SQL para recuperar datos de la tabla Talleres
            string query = "SELECT * FROM Auditoria"; // Puedes ajustar la consulta según tus necesidades

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Auditoria auditoria = new Auditoria();
                            auditoria.Id = Convert.ToInt32(reader["id"]);
                            auditoria.Usuario = reader["usuario"].ToString();
                            auditoria.Accion = reader["accion"].ToString();
                            auditoria.Detalles = reader["detalles"].ToString();
                            auditoria.FechaHora = Convert.ToDateTime(reader["fechahora"]);
                            auditoria.IPAddress = reader["ipaddress"].ToString();
                            auditorias.Add(auditoria);


                        }
                    }
                }
            }

            // Pasar los datos a la vista
            return View(auditorias);
        }
    }
}
