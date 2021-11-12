using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ITelligent.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using netIndustrial.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using opra.itelligent.es.Models;
using opra.itelligent.es.Services;
using opra.itelligent.es.Util;

namespace opra.itelligent.es.Controllers
{
    public class SchedulerController : Controller
    {
        private readonly ProblemasService _problemasService;

        public SchedulerController(ProblemasService problemasService)
        {
            _problemasService = problemasService;
        }

        public async Task<IActionResult> Index(int problema)
        {
            clsDatosJobShop datosJobShop = await _problemasService.GetDatosProblema(problema);

            string guid = Guid.NewGuid().ToString();

            clsDatosSchedule cSchedule = _problemasService.GuardarPlanificacion(datosJobShop, guid);

            clsDatosResultados cResultados = new clsDatosResultados()
            {
                cData = datosJobShop
            };

            DateTime dtmStartDate = DateTime.Today;

            // Pruba a genera el schedule con un horario
            _problemasService.GenerarHorario(cResultados, cSchedule, dtmStartDate);
           
            RespuestaScheduler resp = SchedulerUtil.GenerarRespuesta(problema, guid, cResultados, cSchedule.hsIdOperationInCriticalPath, dtmStartDate, cSchedule.dblMakespan);

            return View(resp);
        }
    }
}