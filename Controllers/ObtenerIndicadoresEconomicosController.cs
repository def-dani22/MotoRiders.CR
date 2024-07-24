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
        public async Task<ActionResult> Index()
        {
            // Llamar al método para obtener indicadores económicos
            var model = await ObtenerIndicadoresEconomicos();
            return View(model);
        }

        public async Task<List<TipoCambioDolarModel>> ObtenerIndicadoresEconomicos()
        {
            List<TipoCambioDolarModel> listaTipoCambio = new List<TipoCambioDolarModel>();

            try
            {
                var cliente = new IndicadoresEconomicos_BCCR.wsindicadoreseconomicosSoapClient("wsindicadoreseconomicosSoap");
                listaTipoCambio = await LlamarServicio(cliente);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al llamar al primer endpoint: " + ex.Message;
            }

            return listaTipoCambio;
        }

        private async Task<List<TipoCambioDolarModel>> LlamarServicio(IndicadoresEconomicos_BCCR.wsindicadoreseconomicosSoapClient cliente)
        {
            List<TipoCambioDolarModel> listaTipoCambio = new List<TipoCambioDolarModel>();

            string indicador = "317";
            string fechaInicio = DateTime.Now.ToString("23/07/2024");
            string fechaFinal = DateTime.Now.ToString("23/07/2024");
            string nombre = "Esteban Gómez Rivera";
            string subNiveles = "N";
            string correoElectronico = "ESTEBAN.GOMEZ.RIVERA@cuc.cr";
            string token = "ZS2AMSPT1N";

            var respuesta = await cliente.ObtenerIndicadoresEconomicosAsync(indicador, fechaInicio, fechaFinal, nombre, subNiveles, correoElectronico, token);

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
                        return new List<TipoCambioDolarModel>();
                    }
                }

                return listaTipoCambio;
            }
            else
            {
                ViewBag.Error = "Error al obtener los indicadores económicos";
                return new List<TipoCambioDolarModel>();
            }
        }
    }
}
