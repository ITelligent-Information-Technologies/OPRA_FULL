using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ITelligent.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using netIndustrial.Models;
using Newtonsoft.Json;
using opra.itelligent.es.Models;
using opra.itelligent.es.Services;
using opra.itelligent.es.Util;

namespace opra.itelligent.es.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SchedulerApiController : ControllerBase
    {
        private readonly ProblemasService _problemasService;

        public SchedulerApiController(ProblemasService problemasService)
        {
            _problemasService = problemasService;
        }

        [HttpPost("refresh/{guid}")]
        public async Task<IActionResult> refresh(int problema, string guid, [FromBody] Dictionary<int,List<int>> machines)
        {
            clsDatosJobShop datosJobShop = await _problemasService.GetDatosProblema(problema);

            clsDatosSchedule cSchedule = _problemasService.GetPlanificacion(guid);

            if(datosJobShop == null || cSchedule == null)
            {
                return NotFound();
            }

            clsDatosResultados cResultados = new clsDatosResultados()
            {
                cData = datosJobShop
            };

            foreach (var machine in machines)
            {
                List<int> ordenOperaciones = machine.Value.Distinct().ToList();

                cSchedule.dicIdMachineIdOperationFirst[machine.Key] = ordenOperaciones.First();
                cSchedule.dicIdMachineIdOperationLast[machine.Key] = ordenOperaciones.Last();

                for (int i = 0; i < ordenOperaciones.Count(); i++)
                {
                    if (i == 0)
                    {
                        cSchedule.dicIdOperationIdPreviousInMachine[ordenOperaciones[i]] = -1;
                    }
                    else
                    {
                        cSchedule.dicIdOperationIdPreviousInMachine[ordenOperaciones[i]] = ordenOperaciones[i - 1];
                    }

                    if (i == ordenOperaciones.Count() - 1)
                    {
                        cSchedule.dicIdOperationIdNextInMachine[ordenOperaciones[i]] = -1;
                    }
                    else
                    {
                        cSchedule.dicIdOperationIdNextInMachine[ordenOperaciones[i]] = ordenOperaciones[i + 1];

                    }
                }
            }

            // Calcula el bellman para la solucion actual
            clsBellman cBellman = new clsBellman();
            cBellman.CalcularBellman(cResultados.cData, cSchedule);

            if (cSchedule.blnEsBucle)
            {
                return BadRequest("La planificación es errónea porque se produce un bucle");
            }

            DateTime dtmStartDate = DateTime.Today;

            // Pruba a genera el schedule con un horario
            _problemasService.GenerarHorario(cResultados, cSchedule, dtmStartDate);

            RespuestaScheduler resp = SchedulerUtil.GenerarRespuesta(problema, guid, cResultados, cSchedule.hsIdOperationInCriticalPath, dtmStartDate, cSchedule.dblMakespan);

            return Ok(resp);
        }

        //[HttpPost("optimizar")]
        //public async Task<IActionResult> optimizar()
        //{
        //    DateTime dtmStarDate = DateTime.Today;

        //    clsDatosJobShop datosJobShop = await _cache.GetOrCreateAsync<clsDatosJobShop>("taillard_20_15", async entry =>
        //    {
        //        using (GetObjectResponse responseS3 = await _s3Client.GetObjectAsync("opra-itelligent", "problemas/tai20_15_1.json"))
        //        using (Stream stream = responseS3.ResponseStream)
        //        using (StreamReader reader = new StreamReader(stream))
        //        {
        //            return JsonConvert.DeserializeObject<clsDatosJobShop>(reader.ReadToEnd());
        //        }
        //    });

        //    clsDatosHorarios cDatosHorarios = new clsDatosHorarios();
        //    List<clsDatosHorariosHoras> lstHoras = new List<clsDatosHorariosHoras>();
        //    clsDatosHorariosHoras cHoras = new clsDatosHorariosHoras();
        //    cHoras.dblHoraDesde = 9;
        //    cHoras.dblHoraHasta = 17;
        //    lstHoras.Add(cHoras);
        //    cDatosHorarios.dicIdDiasSemanaHorarios.Add(1, lstHoras);
        //    cDatosHorarios.dicIdDiasSemanaHorarios.Add(2, lstHoras);
        //    cDatosHorarios.dicIdDiasSemanaHorarios.Add(3, lstHoras);
        //    cDatosHorarios.dicIdDiasSemanaHorarios.Add(4, lstHoras);
        //    cDatosHorarios.dicIdDiasSemanaHorarios.Add(5, lstHoras);
        //    clsDatosHorariosHoras cHorariosFiesta = new clsDatosHorariosHoras();
        //    cHorariosFiesta.blnSinActividad = true;
        //    List<clsDatosHorariosHoras> lstHorasFinSemana = new List<clsDatosHorariosHoras>();
        //    lstHorasFinSemana.Add(cHorariosFiesta);
        //    cDatosHorarios.dicIdDiasSemanaHorarios.Add(6, lstHorasFinSemana);
        //    cDatosHorarios.dicIdDiasSemanaHorarios.Add(7, lstHorasFinSemana);
        //    // Mete un festivo
        //    cDatosHorarios.dicFechasEspecialesHorarios.Add(Convert.ToDateTime("02/01/2020"), lstHorasFinSemana);

        //    clsDatosResultados cResultados = new clsDatosResultados()
        //    {
        //        cData = datosJobShop
        //    };

        //    clsSolucionInicial cSInicial = new clsSolucionInicial();
        //    clsDatosSchedule cSchedule = cSInicial.OrdenarPorLongitudTrabajos(cResultados.cData);

        //    clsDatosParametros cParametros = new clsDatosParametros(cResultados.cData);
        //    //1386
        //    cParametros.intMaxStackBackTrack = 30; //30
        //    cParametros.intMaxIteraciones = 100000; //100000
        //    cParametros.intMaxIteracionesPorBucle = 2500; //2500
        //    cParametros.intTabuListMin = 300; //300
        //    cParametros.intTabuListMax = cParametros.intTabuListMin;
        //    cParametros.enuGuardarTabuList = TiposFirmaTabuList.SoloParUV; // SoloParUV
        //    clsTSSA cTssa = new clsTSSA();

        //    clsDatosSchedule cScheduleMin = cTssa.JobShop(cResultados.cData, cSchedule, cParametros);
        //    // Obtiene la fechas para el Schedule minmimo
        //    clsHorarios cHorarios = new clsHorarios();
        //    (cResultados.dicIdOperacionFechaStartEnd, cResultados.dicIdOperacionListaStartEnd) = cHorarios.ObtenerFechas(cDatosHorarios, cResultados.cData, cSchedule, dtmStarDate);

        //    RespuestaScheduler resp = SchedulerUtil.GenerarRespuesta(cResultados, cScheduleMin.hsIdOperationInCriticalPath, dtmStarDate, cScheduleMin.dblMakespan);

        //    return Ok(resp);
        //}

        [HttpPost("procesar")]
        public async Task<IActionResult> procesarSchedule(int problema, [FromBody] clsDatosSchedule datos)
        {
            DateTime dtmStartDate = DateTime.Today;

            clsDatosJobShop datosJobShop = await _problemasService.GetDatosProblema(problema);

            clsDatosResultados cResultados = new clsDatosResultados()
            {
                cData = datosJobShop
            };

            _problemasService.GenerarHorario(cResultados, datos, dtmStartDate);
            RespuestaScheduler resp = SchedulerUtil.GenerarRespuesta(problema, null, cResultados, datos.hsIdOperationInCriticalPath, dtmStartDate, datos.dblMakespan);

            return Ok(resp);
        }
    }
}