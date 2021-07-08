using ITelligent.Scheduling;
using netIndustrial.Models;
using opra.itelligent.es.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace opra.itelligent.es.Util
{
    public class SchedulerUtil
    {
        public static RespuestaScheduler GenerarRespuesta(int problema, string guid, clsDatosResultados cResultados, IEnumerable<int> lstIdOperationInCriticalPath, DateTime dtmStarDate, double dblMakespan)
        {
            RespuestaScheduler resp = new RespuestaScheduler();

            List<EventData> lstOperacionesPorMaquina = new List<EventData>();
            List<EventData> lstOperacionesPorJob = new List<EventData>();
            Dictionary<int, ResourceData> dicMaquinas = new Dictionary<int, ResourceData>();
            Dictionary<int, ResourceData> dicJobs = new Dictionary<int, ResourceData>();
            DateTime dtmEndDate = cResultados.dicIdOperacionFechaStartEnd.Values.Max(x => x.dtmFechaEnd);

            foreach (var operacion in cResultados.cData.dicIdOperationIdMachine)
            {
                int intIdOperacion = operacion.Key;
                int intIdMachine = operacion.Value;
                int intIdJob = cResultados.cData.dicIdOperationIdJob[intIdOperacion];

                if (!dicMaquinas.ContainsKey(intIdMachine))
                {
                    dicMaquinas.Add(intIdMachine, new ResourceData()
                    {
                        orden = intIdMachine,
                        id = intIdMachine.ToString(),
                        name = "Máquina " + intIdMachine
                    });
                }

                if (!dicJobs.ContainsKey(intIdJob))
                {
                    dicJobs.Add(intIdJob, new ResourceData()
                    {
                        orden = intIdJob,
                        id = intIdJob.ToString(),
                        name = "JOB " + intIdJob
                    });
                }

                foreach (clsFechasStartEnd bloque in cResultados.dicIdOperacionListaStartEnd[intIdOperacion])
                {
                    EventData eventoMaquina = new EventData()
                    {
                        id = intIdOperacion.ToString(),
                        id2 = intIdJob.ToString(),
                        resource = intIdMachine.ToString(),
                        startDate = bloque.dtmFechaStart,
                        endDate = bloque.dtmFechaEnd,
                        text = "J - " + intIdJob + " Operacion - " + intIdOperacion

                    };
                    eventoMaquina.bubbleHtml = $"<H6>Operación {intIdOperacion} </H6><ul><li><b>Job:</b> {intIdJob}</li><li><b>Fecha Inicio:</b> {eventoMaquina.startDate.ToString("dd/MM/yyyy HH:mm")}</li><li><b>Fecha Fin:</b>{eventoMaquina.endDate.ToString("dd/MM/yyyy HH:mm")}</li></lu>";

                    EventData eventoJob = new EventData()
                    {
                        id = intIdOperacion.ToString(),
                        id2 = intIdMachine.ToString(),
                        resource = intIdJob.ToString(),
                        startDate = bloque.dtmFechaStart,
                        endDate = bloque.dtmFechaEnd,
                        text = "M - " + intIdMachine.ToString() + " Operación - " + intIdOperacion,

                    };
                    eventoJob.bubbleHtml = $"<H6>Operación {intIdOperacion} </H6><ul><li><b>Maquina:</b> {intIdMachine}</li><li><b>Fecha Inicio:</b> {eventoJob.startDate.ToString("dd/MM/yyyy HH:mm")}</li><li><b>Fecha Fin:</b>{eventoJob.endDate.ToString("dd/MM/yyyy HH:mm")}</li></lu>";

                    if (lstIdOperationInCriticalPath.Contains(intIdOperacion))
                    {
                        eventoMaquina.borderColor = eventoJob.borderColor = "red";
                    }

                    lstOperacionesPorMaquina.Add(eventoMaquina);
                    lstOperacionesPorJob.Add(eventoJob);
                }
            }

            List<ResourceData> lstMaquinas = dicMaquinas.Values.OrderBy(x => x.orden).ToList();
            List<ResourceData> lstJobs = dicJobs.Values.OrderBy(x => x.orden).ToList();

            resp.IdProblema = problema;
            resp.Guid = guid;
            resp.StartDate = dtmStarDate;
            resp.Days = Math.Ceiling((dtmEndDate - dtmStarDate).TotalDays);
            resp.EventsMachine = lstOperacionesPorMaquina;
            resp.EventsJob = lstOperacionesPorJob;
            resp.ResourcesMachine = lstMaquinas;
            resp.ResourcesJob = lstJobs;
            resp.BestMakespan = cResultados.cData.dblMakespanBest;

            resp.Makespan = dblMakespan;

            return resp;
        }
    }
}
