using MotoRiders.CR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MotoRiders.CR.Controllers
{
    public class IndicadoresEconomicosBCCRController : Controller
    {
        // GET: IndicadoresEconomicosBCCR
        public ActionResult Index()
        {
            // Llamar al método para obtener indicadores económicos
            var model = ObtenerIndicadoresEconomicos().Result;
            return View(model);
        }
        public async Task<ActionResult> ObtenerIndicadoresEconomicos()
        {
            try
            {
                var cliente = new IndicadoresEconomicos_BCCR.wsindicadoreseconomicosSoapClient();

                string indicador = "317";
                string fechaInicio = "18/07/2024";
                string fechaFinal = "18/07/2024";
                string nombre = "Esteban Gómez Rivera";
                string subNiveles = "N";
                string correoElectronico = "ESTEBAN.GOMEZ.RIVERA@cuc.cr";
                string token = "ZS2AMSPT1N";

                var respuesta = await cliente.ObtenerIndicadoresEconomicosAsync(indicador, fechaInicio, fechaFinal, nombre, subNiveles, correoElectronico, token);

                List<TipoCambioDolarModel> listaTipoCambio = new List<TipoCambioDolarModel>();

                if (respuesta != null && respuesta.Tables.Count > 0 && respuesta.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in respuesta.Tables[0].Rows)
                    {
                        var valorString = row["NUM_VALOR"].ToString();

                        if (decimal.TryParse(valorString, out decimal valor))
                        {
                            listaTipoCambio.Add(new TipoCambioDolarModel { Valor = valor });
                        }
                        else
                        {
                            ViewBag.Error = "Error al convertir el valor";
                            return View("Index", listaTipoCambio);
                        }
                    }

                    return View("Index", listaTipoCambio);
                }
                else
                {
                    ViewBag.Error = "Error al obtener los indicadores económicos";
                    return View("Index", new List<TipoCambioDolarModel>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al llamar al servicio web: " + ex.Message;
                return View("Index", new List<TipoCambioDolarModel>());
            }
        }
    }
}
