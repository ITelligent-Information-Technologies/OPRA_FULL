using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    /// <summary>
    /// Datos de un movimiento realizado
    /// segun neighborhood N6
    /// </summary>
    class clsDatosMovimiento
    {
        public Int32 intIdOperacionU; // Operacion U en el bloque
        public Int32 intIdOperacionV; // Operacion V en el bloque
        public Int32 intIdOperacionAnteriorU; //Operacoin anterior a U en maquina antes de cambio
        public Int32 intIdOperacionPosteriorV; //Operacion posterior a V en maquina antes de cambio
        public Int32 intIdOperacionAnteriorV; //Operacoin anterior a V en maquina antes de cambio
        public Int32 intIdOperacionPosteriorU; //Operacion posterior a U en maquina antes de cambio
        public Boolean blnEsForward; // Si el cambio es forward, si es false es backward
        public Boolean bleEsN6; // Di el cambio es N6 si no lo es entonces N1
        public double dblEstimatedMakespan; // Makespan estimado
    }
    /// <summary>
    /// Esta clase esta inspirada en el paper de Zhang
    /// A  very fast algorithm for the job shop scheduling problem
    /// e intenta resolver el JSP utilizando TabuSearch TS
    /// y Simulated Annealing SA
    /// </summary>
    public class clsTSSA
    {
        private Random _rnd = new Random(100);
        public clsDatosSchedule JobShop(clsDatosJobShop cData, clsDatosSchedule cSchedule, clsDatosParametros cParametros)
        {
            clsDatosSchedule cScheduleMin = new clsDatosSchedule();
            // Crea la clase de los Neighbourhood
            clsN6 cN6 = new clsN6();
            clsN1 cN1 = new clsN1();
            clsN0 cN0 = new clsN0();
            // Crea el stack para el backtrack
            clsMaxStack<clsDatosSchedule> cStackBackTrack = new clsMaxStack<clsDatosSchedule>(cParametros.intMaxStackBackTrack);
            // Genera la solucion Inicial
            // TODO
            // Clase de deteccio de ciclos
            clsCycle cCycle = new clsCycle(100, 2);
            // Calcula Bellman            
            clsBellman cBellman = new clsBellman();
            clsTabuList cTabuList = new clsTabuList(cParametros);
            clsFirmaTabuList cFirma = new clsFirmaTabuList(cParametros.enuGuardarTabuList);
            clsDatosMovimiento cMovimientoOld = new clsDatosMovimiento();
            double dblMakespanMin = double.MaxValue;
            // Parametros control bucles
            Boolean blnEnBucleBig = true;
            // Comienza a contar el tiempo e iteraciones
            DateTime dtmStart = DateTime.Now;
            Int32 intCuentaBucleBig = 0;
            clsSimulatedAnnealing cSA = new clsSimulatedAnnealing(cParametros);
            // Bucle Big (maximo iteraciones y tiempos)
            while (blnEnBucleBig)
            {
                Int32 intCuentaBucleSmall = 0;
                Boolean blnEnBucleSmall = true;
                // Bucle small (iteraciones pequeñas)
                while (blnEnBucleSmall)
                {
                    intCuentaBucleBig++;
                    intCuentaBucleSmall++;
                    Boolean blnEsCiclo = false;
                    // Calcula el makespan utilizando bellman
                    cBellman.CalcularBellman(cData, cSchedule, cParametros.blnConsiderarMultiplesCaminosCriticos);
                    // Si se ha producido un bucle (es decir en el grafo hay un bucle)
                    if (cSchedule.blnEsBucle)
                        new Exception("Error Bucle");
                    // Comprueba si hay un ciclo (se repiten los makespan con un patron)
                    blnEsCiclo = cCycle.CheckCycleAndAdd(cSchedule.dblMakespan);
                    if (blnEsCiclo)
                        System.Diagnostics.Debug.WriteLine("********************************* CICLO CICLO CICLO");
                    if (!blnEsCiclo)
                    {
                        // Compruba si ha mejorado
                        if (cSchedule.dblMakespan < dblMakespanMin || (cSchedule.dblMakespan > dblMakespanMin && cSA.Aceptar(cSchedule.dblMakespan, dblMakespanMin)))
                        {
                            if (cSchedule.dblMakespan < dblMakespanMin)
                            {
                                dblMakespanMin = cSchedule.dblMakespan;
                                // Guarda el minimo hasta el momento
                                cScheduleMin = clsObjectCopy.Clone<clsDatosSchedule>(cSchedule);
                            }
                            else
                                System.Diagnostics.Debug.WriteLine("****************************Aceptada INFERIOR");
                            // Hace una copia y lo guarda en backtrack
                            clsDatosSchedule cScheduleNew = clsObjectCopy.Clone<clsDatosSchedule>(cSchedule);
                            cStackBackTrack.Push(cScheduleNew);
                            intCuentaBucleSmall = 0;
                        }
                        // Calcula Bellman desde el final
                        Dictionary<Int32, double> dicIdOperacionTiempoHastaFin = cBellman.CalcularBellmanFromEnd(cData, cSchedule);
                        // Genera los movimientos
                        clsDatosCambio cCambio = new clsDatosCambio();
                        // Cambio es tipo N6
                        if (cParametros.enuVecindarioPrincipal == TiposVecindarios.N6)
                            cCambio = cN6.SeleccionarMovimiento(cParametros, cSchedule.strFirmaCaminoCritico, cTabuList, cData, cSchedule, dicIdOperacionTiempoHastaFin, dblMakespanMin);
                        else if (cParametros.enuVecindarioPrincipal == TiposVecindarios.N1)
                            cCambio = cN1.SeleccionarMovimiento(cData, cSchedule);
                        else
                            new Exception("Error Movimiento no definido");
                        // Hay un cambio potencial para realizar
                        if (cCambio.intIdOperacionU > -1)
                        {
                            if (cParametros.enuVecindarioPrincipal == TiposVecindarios.N6)
                            {
                                if (cCambio.blnEsForward)
                                {
                                    cTabuList.Add(cSchedule, cCambio);
                                    cMovimientoOld = cN6.RealizarMovimientoForward(cData, cCambio.intIdOperacionU, cCambio.intIdOperacionV, cSchedule);
                                }
                                else
                                {
                                    cTabuList.Add(cSchedule, cCambio);
                                    cMovimientoOld = cN6.RealizarMovimientoBackward(cData, cCambio.intIdOperacionU, cCambio.intIdOperacionV, cSchedule);
                                }
                            }
                            else if (cParametros.enuVecindarioPrincipal == TiposVecindarios.N1)
                            {
                                cTabuList.Add(cSchedule, cCambio);
                                cMovimientoOld = cN1.RealizarMovimiento(cCambio, cData, cSchedule);
                            }
                            else
                                new Exception("Error Movimiento no definido");
                            cMovimientoOld.dblEstimatedMakespan = cCambio.dblMakespanEstimado;
                            System.Diagnostics.Debug.WriteLine(intCuentaBucleBig + "->" + cSchedule.dblMakespan + "->" + dblMakespanMin);
                        }
                        else
                        {
                            //Se utiliza N1
                            if (cParametros.enuSiListaMovimientosVaciaUtilizar == TiposVecindarios.N1)
                            {
                                clsDatosCambio cCamb = cN1.SeleccionarMovimiento(cData, cSchedule);
                                cMovimientoOld = cN1.RealizarMovimiento(cCamb, cData, cSchedule);
                            }
                            else if (cParametros.enuSiListaMovimientosVaciaUtilizar == TiposVecindarios.N0)
                                cMovimientoOld = cN0.SeleccionarYRealizarMovimiento(cData, cSchedule);
                            else if (cParametros.enuSiListaMovimientosVaciaUtilizar == TiposVecindarios.NingunVencindario)
                                blnEnBucleSmall = false;
                            else
                                new Exception("El vecindario definido no se encuentra");
                            System.Diagnostics.Debug.WriteLine("*************************N1");
                        }
                    }// no es blnEsCiclo
                     // Comprueba si debe salir
                    if (blnEsCiclo || intCuentaBucleSmall > cParametros.intMaxIteracionesPorBucle || intCuentaBucleBig > cParametros.intMaxIteraciones)
                        blnEnBucleSmall = false;
                } // Bucle Small
                  // Siguiente en el stack
                  // Decrementa temperatura en cada iteracion
                cSA.DecrementarTemperatura();
                if (cStackBackTrack.Count > 0)
                    cSchedule = cStackBackTrack.Pop();
                else
                {
                    if (cParametros.enuSiListaBacktrackVaciaUtilizar == TiposVecindarios.N1)
                    {
                        clsDatosCambio cCamb = cN1.SeleccionarMovimiento(cData, cSchedule);
                        cMovimientoOld = cN1.RealizarMovimiento(cCamb, cData, cSchedule);
                    }
                    else if (cParametros.enuSiListaBacktrackVaciaUtilizar == TiposVecindarios.N0)
                        cMovimientoOld = cN0.SeleccionarYRealizarMovimiento(cData, cSchedule);
                    else if (cParametros.enuSiListaBacktrackVaciaUtilizar == TiposVecindarios.NingunVencindario)
                        blnEnBucleBig = false;
                    else
                        new Exception("El vecindario definido no se encuentra");

                    System.Diagnostics.Debug.WriteLine("*************************N1 BACKTRACK");
                }

                cTabuList = new clsTabuList(cParametros);
                cCycle.Clear();
                // Comprueba si debe finalizar por maximo de iteraciones o por tiempo
                if (intCuentaBucleBig > cParametros.intMaxIteraciones || (DateTime.Now - dtmStart).TotalSeconds > cParametros.intMaxSegundos)
                    blnEnBucleBig = false;
            } // Bucle Big
            double dblRE = (dblMakespanMin - cData.dblMakespanBest) / cData.dblMakespanBest;
            double dblSeconds = (DateTime.Now - dtmStart).TotalSeconds;
            System.Diagnostics.Debug.WriteLine("Makespan Obtenido: " + dblMakespanMin);
            System.Diagnostics.Debug.WriteLine("Tiempo (segundos): " + dblSeconds);
            System.Diagnostics.Debug.WriteLine("Resultado Respecto Mejor(%): " + (1 - dblRE) * 100);
            return cScheduleMin;
        }



    }
}
