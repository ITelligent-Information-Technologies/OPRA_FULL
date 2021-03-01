using Amazon.S3;
using Amazon.S3.Model;
using ITelligent.Scheduling;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using opra.itelligent.es.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace opra.itelligent.es.Services
{
    public class ProblemasService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IMemoryCache _cache;
        private readonly BdOpraContext _context;

        public ProblemasService(IAmazonS3 s3Client, IMemoryCache cache, BdOpraContext context)
        {
            _s3Client = s3Client;
            _cache = cache;
            _context = context;
        }

        public Task<clsDatosJobShop> GetDatosProblema(int problema)
        {
           return _cache.GetOrCreateAsync(problema, async entry =>
            {
                TblMaestraProblema datosProblema = _context.TblMaestraProblema.FirstOrDefault(x => x.IntId == problema);

                using (GetObjectResponse responseS3 = await _s3Client.GetObjectAsync(datosProblema.StrBucketS3, datosProblema.StrKeyS3))
                using (Stream stream = responseS3.ResponseStream)
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<clsDatosJobShop>(reader.ReadToEnd());
                }
            });
        }

        public clsDatosSchedule GetPlanificacion(string guid)
        {
            return _cache.Get<clsDatosSchedule>($"schedule_{guid}");
        }

        public clsDatosSchedule GuardarPlanificacion(clsDatosJobShop datosJobShop, string guid)
        {
            return _cache.GetOrCreate($"schedule_{guid}", entry =>
            {
                clsSolucionInicial cSInicial = new clsSolucionInicial();
                clsDatosSchedule cSchedule = cSInicial.OrdenarPorLongitudTrabajos(datosJobShop);

                // Calcula el bellman para la solucion actual
                clsBellman cBellman = new clsBellman();
                cBellman.CalcularBellman(datosJobShop, cSchedule);

                return cSchedule;
            });
        }

        public void GenerarHorario(clsDatosResultados cResultados, clsDatosSchedule cSchedule, DateTime dtmStartDate)
        {
            clsDatosHorarios cDatosHorarios = new clsDatosHorarios();
            List<clsDatosHorariosHoras> lstHoras = new List<clsDatosHorariosHoras>();
            clsDatosHorariosHoras cHoras = new clsDatosHorariosHoras();
            cHoras.dblHoraDesde = 9;
            cHoras.dblHoraHasta = 17;
            lstHoras.Add(cHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(1, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(2, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(3, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(4, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(5, lstHoras);
            clsDatosHorariosHoras cHorariosFiesta = new clsDatosHorariosHoras();
            cHorariosFiesta.blnSinActividad = true;
            List<clsDatosHorariosHoras> lstHorasFinSemana = new List<clsDatosHorariosHoras>();
            lstHorasFinSemana.Add(cHorariosFiesta);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(6, lstHorasFinSemana);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(7, lstHorasFinSemana);
            // Mete un festivo
            cDatosHorarios.dicFechasEspecialesHorarios.Add(Convert.ToDateTime("02/01/2020"), lstHorasFinSemana);

            clsHorarios cHorarios = new clsHorarios();
            (cResultados.dicIdOperacionFechaStartEnd, cResultados.dicIdOperacionListaStartEnd) = cHorarios.ObtenerFechas(cDatosHorarios, cResultados.cData, cSchedule, dtmStartDate);
        }
    }
}
