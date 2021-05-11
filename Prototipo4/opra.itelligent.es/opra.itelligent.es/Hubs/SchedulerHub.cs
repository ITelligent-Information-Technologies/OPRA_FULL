using Amazon.S3;
using Amazon.S3.Model;
using ITelligent.Scheduling;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using opra.itelligent.es.Models;
using opra.itelligent.es.Services;
using opra.itelligent.es.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace opra.itelligent.es.Hubs
{
    public class SchedulerHub : Hub
    {
        private readonly ProblemasService _problemasService;

        public SchedulerHub(ProblemasService problemasService)
        {
            _problemasService = problemasService;
        } 

        public async Task<ChannelReader<MessageSchedule>> OptimizarGuid(int problema, string guid, CancellationToken cancellationToken) {
            clsDatosSchedule cSchedule = _problemasService.GetPlanificacion(guid);

            return await Optimizar(problema, cSchedule, cancellationToken);
        }

        public async Task<ChannelReader<MessageSchedule>> Optimizar(int problema, clsDatosSchedule cSchedule, CancellationToken cancellationToken)
        {

            DateTime dtmStarDate = DateTime.Today;

            clsDatosJobShop datosJobShop = await _problemasService.GetDatosProblema(problema);

            //clsDatosHorarios cDatosHorarios = new clsDatosHorarios();
            //List<clsDatosHorariosHoras> lstHoras = new List<clsDatosHorariosHoras>();
            //clsDatosHorariosHoras cHoras = new clsDatosHorariosHoras();
            //cHoras.dblHoraDesde = 9;
            //cHoras.dblHoraHasta = 17;
            //lstHoras.Add(cHoras);
            //cDatosHorarios.dicIdDiasSemanaHorarios.Add(1, lstHoras);
            //cDatosHorarios.dicIdDiasSemanaHorarios.Add(2, lstHoras);
            //cDatosHorarios.dicIdDiasSemanaHorarios.Add(3, lstHoras);
            //cDatosHorarios.dicIdDiasSemanaHorarios.Add(4, lstHoras);
            //cDatosHorarios.dicIdDiasSemanaHorarios.Add(5, lstHoras);
            //clsDatosHorariosHoras cHorariosFiesta = new clsDatosHorariosHoras();
            //cHorariosFiesta.blnSinActividad = true;
            //List<clsDatosHorariosHoras> lstHorasFinSemana = new List<clsDatosHorariosHoras>();
            //lstHorasFinSemana.Add(cHorariosFiesta);
            //cDatosHorarios.dicIdDiasSemanaHorarios.Add(6, lstHorasFinSemana);
            //cDatosHorarios.dicIdDiasSemanaHorarios.Add(7, lstHorasFinSemana);
            //// Mete un festivo
            //cDatosHorarios.dicFechasEspecialesHorarios.Add(Convert.ToDateTime("02/01/2020"), lstHorasFinSemana);

            clsDatosResultados cResultados = new clsDatosResultados()
            {
                cData = datosJobShop
            };


            CancellationToken token = Context.ConnectionAborted;


            clsDatosParametros cParametros = new clsDatosParametros(cResultados.cData);
            //1386
            cParametros.intMaxSegundos = 6;
            cParametros.intMaxStackBackTrack = 30; //30
            cParametros.intMaxIteraciones = 100000; //100000
            cParametros.intMaxIteracionesPorBucle = 2500; //2500
            cParametros.intTabuListMin = 300; //300
            cParametros.intTabuListMax = cParametros.intTabuListMin;
            cParametros.enuGuardarTabuList = TiposFirmaTabuList.SoloParUV; // SoloParUV
            clsTSSA cTssa = new clsTSSA();

            var channel = Channel.CreateUnbounded<MessageSchedule>();

            _ = cTssa.JobShopAsync(cResultados.cData, cSchedule, cParametros, channel.Writer/*, cancellationToken*/);
            //_ = WriteItemsAsync(channel.Writer, 10000, 500, cancellationToken);

            // Obtiene la fechas para el Schedule minmimo
            //_cache.Set<clsDatosSchedule>($"schedule_{ guid}", cScheduleMin);

            //clsHorarios cHorarios = new clsHorarios();
            //(cResultados.dicIdOperacionFechaStartEnd, cResultados.dicIdOperacionListaStartEnd) = cHorarios.ObtenerFechas(cDatosHorarios, cResultados.cData, cScheduleMin, dtmStarDate);

            //RespuestaScheduler resp = SchedulerUtil.GenerarRespuesta(cResultados, cScheduleMin.hsIdOperationInCriticalPath, dtmStarDate, cScheduleMin.dblMakespan);
            return channel.Reader;
        }

        
    }
}
