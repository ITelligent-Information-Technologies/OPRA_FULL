using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    public class clsHorarios
    {
        /// <summary>
        /// Esta funcion recibe un horario laboral (incluido fechas especiales)
        /// y un schedule en tiempo de procesamiento. Y calcula las fechas yhoras
        /// de comienzo y final de cada operacion. POr un lado calcula la fecha
        /// y hora de comienzo y fin de la operacion. Pero tambien descompone
        /// en bloques ese intervalos, esto es si por ejemplo una operacion no acaba
        /// en el horario laboral obtendra un primer bloque dentro de ese dia desde
        /// el comienzo de la operacion hasta el final del horario laboral y otro bloque
        /// el siguiente dia laboral desde el comienzo del horario de ese dia hasta que 
        /// acabe la operacion si acaba en el segundo dia laboral, si no acabase sigue 
        /// metiendo bloques estos bloques se guardan en una lista dentro de un 
        /// diccionario: dicIdOperationListStartEnd
        /// </summary>
        /// <param name="cHorarios"></param>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        /// <param name="dtmStartDatetime"></param>
        /// <returns></returns>
        public (Dictionary<Int32, clsFechasStartEnd> dicIdOperationFechaStartEnd, Dictionary<Int32, List<clsFechasStartEnd>> dicIdOperationListStartEnd) ObtenerFechas(clsDatosHorarios cHorarios, clsDatosJobShop cData, clsDatosSchedule cSchedule, DateTime dtmStartDatetime)
        {
            Int32[] intIdOperacionOrdenadoStart = new Int32[cSchedule.dicIdOperationStartTime.Count];
            Int32[] intIdOperacionOrdenadoEnd = new Int32[cSchedule.dicIdOperationStartTime.Count];
            double[] dblStartTime = new double[cSchedule.dicIdOperationStartTime.Count];
            double[] dblEndTime = new double[cSchedule.dicIdOperationStartTime.Count];
            Dictionary<Int32, double> dicIdOperationStart = new Dictionary<int, double>();
            Dictionary<Int32, double> dicIdOperationEnd = new Dictionary<int, double>();

            Int32 intCount = 0;
            foreach (KeyValuePair<Int32, double> kvPair in cSchedule.dicIdOperationStartTime)
            {
                dblStartTime[intCount] = kvPair.Value;
                dblEndTime[intCount] = kvPair.Value + cData.dicIdOperationTime[kvPair.Key];
                intIdOperacionOrdenadoStart[intCount] = kvPair.Key;
                intIdOperacionOrdenadoEnd[intCount] = kvPair.Key;
                dicIdOperationStart.Add(kvPair.Key, dblStartTime[intCount]);
                dicIdOperationEnd.Add(kvPair.Key, dblEndTime[intCount]);
                intCount++;
            }
            // Ordena los operaciones por su comienzo y  obtiene las fechas y horas de inicio
            Array.Sort(dblStartTime, intIdOperacionOrdenadoStart);
            // Ordena los operaciones por su comienzo y  obtiene las fechas y horas de fin
            Array.Sort(dblEndTime, intIdOperacionOrdenadoEnd);
            return RecorrerHorarios(cData, intIdOperacionOrdenadoStart, intIdOperacionOrdenadoEnd, cHorarios, dtmStartDatetime, cSchedule);
        }

        /// <summary>
        /// A esta función se le pasa una fecha y obtiene el tiempo en minutos que corrsponde a dicha fecha
        /// teniendo en cuenta el horario laboral y la fecha y hora de comienzo del calendario.
        /// Por ejemplo si el horario comienza el 01/01/2020 09:00 (dtmStartHorarioTime)
        /// y el horario laboral es de 9:00 a 17:00 y el 01/01/2020 es laborable (cHorario)
        /// y sea un trabajo que llega a las 01/01/2020 14:00 (dicIdOperacionReleaseDate)
        /// Esta función obtiene el tiempo en minutos de dtmFecha con las condiciones del horario y
        /// fecha hora comienzo horario. En este caso sevolveria (14-9)*60=300 minutos, es decir 
        /// el trabajo llega en el minuto 300 desde el cominezo de la planificacion.
        /// </summary>
        /// <param name="dicIdOperacionReleaseDate"></param>
        /// <param name="cHorarios"></param>
        /// <param name="dtmStartHorarioDatetime"></param>
        public void ObtenerTiempoDesdeFecha(Dictionary <Int32, DateTime> dicIdOperacionReleaseDate, clsDatosHorarios cHorarios,  DateTime dtmStartCalenadarioDatetime)
        {
            // Primero ordena las operaciones por su fecha de release de anets a despues

            // Va recorriendo los horarios

        }

        private (Dictionary<Int32, clsFechasStartEnd> dicIdOperationFechaStartEnd, Dictionary<Int32, List<clsFechasStartEnd>> dicIdOperationListStartEnd) RecorrerHorarios(clsDatosJobShop cData, Int32[] intIdOperacionOrdenadoStart, Int32[] intIdOperacionOrdenadoEnd, clsDatosHorarios cHorarios, DateTime dtmStartDatetime, clsDatosSchedule cSchedule)
        {
            Dictionary<Int32, clsFechasStartEnd> dicIdOperationFechaStartEnd = new Dictionary<int, clsFechasStartEnd>();
            Dictionary<Int32, List<clsFechasStartEnd>> dicIdOperationListStartEnd = new Dictionary<int, List<clsFechasStartEnd>>();
            // Recorre el calendario y va encontrando los datos
            Boolean blnEnBucle = true;
            DateTime dtmFechaHoraStart = dtmStartDatetime;
            DateTime dtmFechaHoraEnd = dtmStartDatetime;
            DateTime dtmDiaEnCurso = dtmStartDatetime.Date;
            List<clsDatosHorariosHoras> lstHoras = new List<clsDatosHorariosHoras>();
            Dictionary<Int32, DateTime> dicIdOperationDate = new Dictionary<int, DateTime>();
            double dblTiempoContinuoInicio = 0;
            double dblTiempoContinuoFinal = 0;
            Int32 intIndexOrdenadoStart = 0;
            Int32 intIndexOrdenadoEnd = 0;
            HashSet<Int32> hsIdOperacionesNoFinalizadas = new HashSet<int>();
            // Va recorriendo en un bucle cada dia hasta que no queden mas operaciones por asignar
            while (blnEnBucle)
            {
                // Comprueba si el horario es especial
                if (cHorarios.dicFechasEspecialesHorarios.ContainsKey(dtmDiaEnCurso))
                    lstHoras = cHorarios.dicFechasEspecialesHorarios[dtmDiaEnCurso];
                else // Es horario normal (lunes 1, Martes 2, ...)
                {
                    Int32 intWeekDay = (Int32)dtmDiaEnCurso.DayOfWeek;
                    if (!cHorarios.dicIdDiasSemanaHorarios.ContainsKey(intWeekDay))
                        new Exception("No hay horario para el dia de la semana " + intWeekDay);
                    else
                        lstHoras = cHorarios.dicIdDiasSemanaHorarios[intWeekDay];
                }
                // Recorre la lista
                foreach (clsDatosHorariosHoras cHoras in lstHoras)
                {
                    // No hay actividad ese dia no hay nada
                    if (cHoras.blnSinActividad)
                    {
                        if (lstHoras.Count > 1)
                            new Exception("Si no hay actividad no puede haber otro horario ese dia");
                    }
                    else // Si hay actividad
                    {
                        // Actualiza la fecha y hora inicio
                        dtmFechaHoraStart = dtmDiaEnCurso.AddHours(cHoras.dblHoraDesde);
                        dtmFechaHoraEnd = dtmDiaEnCurso.AddHours(cHoras.dblHoraHasta);
                        // Abre las operaciones no acabadas en el horario anterior
                        foreach (Int32 intId in hsIdOperacionesNoFinalizadas)
                        {
                            clsFechasStartEnd cFSE = new clsFechasStartEnd();
                            cFSE.dtmFechaStart = dtmFechaHoraStart;
                            dicIdOperationListStartEnd[intId].Add(cFSE);
                        }
                        // Calcula este horario cuando acaba en tiempo continuo
                        dblTiempoContinuoFinal = dblTiempoContinuoInicio + (dtmFechaHoraEnd - dtmFechaHoraStart).TotalHours;
                        // Comprueba si alguna operacion se inicia en este intervalo y calcula su tiempo de inicio y lo asigna
                        Boolean blnEnBucleTmp = true;
                        while (blnEnBucleTmp && (intIndexOrdenadoStart < intIdOperacionOrdenadoStart.Length))
                        {
                            Int32 intIdOperacion = intIdOperacionOrdenadoStart[intIndexOrdenadoStart];
                            double dblStartTime = ConvertirTiemposAHoras(cSchedule.dicIdOperationStartTime[intIdOperacion], cData.enuTipoTiempo);
                            // El comienzo de la operacion tiene que estar entre TiempoContinuoIni y TiempoContinuoFinal
                            if (dblStartTime >= dblTiempoContinuoInicio && dblStartTime <= dblTiempoContinuoFinal)
                            {
                                // Señala la fecha y hora de inicio de la operacion
                                double dblHorasDiferencia = dblStartTime - dblTiempoContinuoInicio;
                                if (!dicIdOperationFechaStartEnd.ContainsKey(intIdOperacion))
                                {
                                    dicIdOperationListStartEnd.Add(intIdOperacion, new List<clsFechasStartEnd>() );
                                    dicIdOperationListStartEnd[intIdOperacion].Add(new clsFechasStartEnd());
                                    dicIdOperationFechaStartEnd.Add(intIdOperacion, new clsFechasStartEnd());
                                    hsIdOperacionesNoFinalizadas.Add(intIdOperacion);
                                }
                                dicIdOperationFechaStartEnd [intIdOperacion ].dtmFechaStart =dtmFechaHoraStart.AddHours(dblHorasDiferencia);
                                dicIdOperationListStartEnd [intIdOperacion ][dicIdOperationListStartEnd [intIdOperacion ].Count -1].dtmFechaStart = dtmFechaHoraStart.AddHours(dblHorasDiferencia);
                                intIndexOrdenadoStart++;
                            }
                            else
                                blnEnBucleTmp = false;
                        }
                        // Comprueba si alguna operacion se FINALIZA en este intervalo y calcula su tiempo de inicio y lo asigna
                        blnEnBucleTmp = true;
                        while (blnEnBucleTmp && (intIndexOrdenadoEnd < intIdOperacionOrdenadoEnd.Length))
                        {
                            Int32 intIdOperacion = intIdOperacionOrdenadoEnd[intIndexOrdenadoEnd];
                            double dblEndTime = ConvertirTiemposAHoras(cSchedule.dicIdOperationEndTime[intIdOperacion], cData.enuTipoTiempo);
                            // El comienzo de la operacion tiene que estar entre TiempoContinuoIni y TiempoContinuoFinal
                            if (dblEndTime >= dblTiempoContinuoInicio && dblEndTime <= dblTiempoContinuoFinal)
                            {
                                // Señala la fecha y hora de inicio de la operacion
                                double dblHorasDiferencia = dblEndTime - dblTiempoContinuoInicio;
                                dicIdOperationFechaStartEnd [intIdOperacion ].dtmFechaEnd =dtmFechaHoraStart.AddHours(dblHorasDiferencia);
                                dicIdOperationListStartEnd[intIdOperacion][dicIdOperationListStartEnd[intIdOperacion].Count - 1].dtmFechaEnd = dtmFechaHoraStart.AddHours(dblHorasDiferencia);
                                // La saca de no finalizadas
                                hsIdOperacionesNoFinalizadas.Remove(intIdOperacion);
                                intIndexOrdenadoEnd++;
                            }
                            else
                                blnEnBucleTmp = false;
                        }
                    }
                    dblTiempoContinuoInicio = dblTiempoContinuoFinal;
                    // Cierra las operaciones no acabadas en ese horario
                    foreach (Int32 intId in hsIdOperacionesNoFinalizadas  )
                    {
                        dicIdOperationListStartEnd[intId][dicIdOperationListStartEnd[intId].Count - 1].dtmFechaEnd = dtmFechaHoraEnd;
                    }
                } // Cierre bucle de los horarios de un dia
                dtmDiaEnCurso = dtmDiaEnCurso.AddDays(1);
                if (intIndexOrdenadoEnd >= intIdOperacionOrdenadoEnd.Length && intIndexOrdenadoStart >= intIdOperacionOrdenadoStart.Length)
                    blnEnBucle = false;
            }// Cierre bucle de los disas
            return (dicIdOperationFechaStartEnd , dicIdOperationListStartEnd) ;
        }

        /// <summary>
        /// Convierte un determinado tipo de tiempo en horas
        /// </summary>
        /// <param name="dblTiempo"></param>
        /// <param name="enuTipoTiempo"></param>
        /// <returns></returns>
        private double ConvertirTiemposAHoras(double dblTiempo, TipoTiempo enuTipoTiempo)
        {
            double dblTiempoEnHoras = 0;
            if (enuTipoTiempo == TipoTiempo.UnidadEs30Segundos)
                dblTiempoEnHoras = dblTiempo / 120;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs1Minuto)
                dblTiempoEnHoras = dblTiempo / 60;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs15Minutos)
                dblTiempoEnHoras = dblTiempo / 4;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs30Minutos)
                dblTiempoEnHoras = dblTiempo / 2;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs45Minutos)
                dblTiempoEnHoras = dblTiempo * 3 / 4;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs1Hora)
                dblTiempoEnHoras = dblTiempo;
            else
                new Exception("Camnbio de tipo tiempo no implementado");
            return dblTiempoEnHoras;
        }
    }
}

