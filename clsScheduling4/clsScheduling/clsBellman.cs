using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    public class clsBellman
    {
        Random _rnd = new Random(100);

        /// <summary>
        /// Esta funcion realiza un bellman desde el final hacia el comienzo
        /// obteniendo el tiempo que le queda a una operacion por llegar al final
        /// por su camino mas largo. Esto es para la operacion k la funcion devuelve
        /// el tiempo desde k (incluido su propio tiempo de procesamiento) hasta el final
        /// por el camino mas largo. Así si k pertenece al caminio critico y el makespan
        /// es M entonces el tiempo de comienzo de K + el tiempo obtenido por esta funcion
        /// sera igual al valor M.
        /// </summary>
        /// <param name="cDatos"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public Dictionary<Int32, double> CalcularBellmanFromEnd(clsDatosJobShop cDatos, clsDatosSchedule cSchedule)
        {
            // Inicializa diccionarios auxiliares
            Dictionary<Int32, double> dicIdOperacionListaTrabajo = new Dictionary<int, double>(); // Tiempo de la operacion por trabajo
            Dictionary<Int32, double> dicIdOperacionListaMaquina = new Dictionary<int, double>(); // Tiempo de la operacion por maquina
            Dictionary<Int32, double> dicIdOperacionTiempoHastaFin = new Dictionary<int, double>();// Tiempo desde el comienzo operacion hasta el final 
            Queue<Int32> queExplotacion = new Queue<int>(); // Lista de operaciones que estan listas pero no hemmos mirado sus siguientes
            Int32 intIteraciones = 0;
            // Toma las semillas que son las ULTIMAS operaciones en maquinas y que no tengan anteriores en trabajo
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationLast)
            {
                // Comprueba si la ULTIMA operacion la siguiente en maquina etsa vacia
                Int32 intIdOperacion = kvPair.Value;
                dicIdOperacionListaMaquina.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);
                if (cDatos.dicIdOperationIdNextInJob[intIdOperacion] == -1)
                {
                    queExplotacion.Enqueue(intIdOperacion);
                    dicIdOperacionListaTrabajo.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);
                    dicIdOperacionTiempoHastaFin.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);
                }
                else
                {

                }
            }
            // Explota las operaciones en cola
            while (queExplotacion.Count() > 0)
            {
                Int32 intIdOperation = queExplotacion.Dequeue();
                intIteraciones++;
                // Comprueba si la anterior en  job esta lista
                Int32 intIdOperationPreviousJob = cDatos.dicIdOperationIdPreviousInJob[intIdOperation];
                if (intIdOperationPreviousJob > -1)
                {
                    dicIdOperacionListaTrabajo.Add(intIdOperationPreviousJob, dicIdOperacionTiempoHastaFin[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationPreviousJob]);
                    // Comprueba si ya esta lista por maquina ya que es la ultima de la maquina
                    if (cSchedule.dicIdOperationIdNextInMachine[intIdOperationPreviousJob] == -1)
                    {
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousJob, dicIdOperacionListaTrabajo[intIdOperationPreviousJob]);
                        queExplotacion.Enqueue(intIdOperationPreviousJob);
                    }
                    else if (dicIdOperacionListaMaquina.ContainsKey(intIdOperationPreviousJob))
                    {
                        // Comprueba si ya esta lista por maquina no siendo la primera por maquina
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousJob, Math.Max(dicIdOperacionListaMaquina[intIdOperationPreviousJob], dicIdOperacionListaTrabajo[intIdOperationPreviousJob]));
                        queExplotacion.Enqueue(intIdOperationPreviousJob);
                    }
                }
                // Comprueba si la anterior en  machine esta lista
                Int32 intIdOperationPreviousMachine = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperation];
                if (intIdOperationPreviousMachine > -1)
                {
                    dicIdOperacionListaMaquina.Add(intIdOperationPreviousMachine, dicIdOperacionTiempoHastaFin[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationPreviousMachine]);
                    // Comprueba si ya esta lista por trabajo ya que es la ultima del trabajo
                    if (cDatos.dicIdOperationIdNextInJob[intIdOperationPreviousMachine] == -1)
                    {
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousMachine, dicIdOperacionListaMaquina[intIdOperationPreviousMachine]);
                        queExplotacion.Enqueue(intIdOperationPreviousMachine);
                    }
                    else if (dicIdOperacionListaTrabajo.ContainsKey(intIdOperationPreviousMachine))
                    {
                        // Comprueba si ya esta lista por maquina no siendo la primera por maquina
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousMachine, Math.Max(dicIdOperacionListaMaquina[intIdOperationPreviousMachine], dicIdOperacionListaTrabajo[intIdOperationPreviousMachine]));
                        queExplotacion.Enqueue(intIdOperationPreviousMachine);
                    }
                }
            }
            return dicIdOperacionTiempoHastaFin;
        }

        /// <summary>
        /// Este es el mismo bellman que el calcular bellman
        /// pero comprueba si hay variso critical path
        /// los guarda y tambien sus bloque asociados
        /// </summary>
        /// <param name="cDatos"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public void CalcularBellman(clsDatosJobShop cDatos, clsDatosSchedule cSchedule, Boolean blnMulticriticalPath)
        {
            // Pone a cero los valores de Bellman
            cSchedule.ReinicializarDatosBellman();
            // Comienza
            Dictionary<Int32, Int32> dicIdMachineNextLstPosition = new Dictionary<int, int>();
            // Explota las operaciones en cola y obtiene comienzos y final de cada operacion
            (double dblMakespanMax, Int32 intIdOperationMax) = CalcularBellmanObtenerTiempos(cDatos, cSchedule);
            // Si no ha recorrido todas hay BUCLE
            if (cSchedule.dicIdOperationStartTime.Count != cDatos.dicIdOperationTime.Count)
            {
                cSchedule.blnEsBucle = true;
                return;
            }
            cSchedule.dblMakespan = dblMakespanMax;
            // TODO AQUI SE PODRIA QUITAR EL CALCULO DEL MAKESPAN Y HACERLO EN EL BUCLE DE BELLMAN END
            // Obtiene el camino critico
            ObtenerCaminoCritico(intIdOperationMax, cDatos, cSchedule, blnMulticriticalPath );
            cSchedule.blnEsBucle = false;
        }

        /// <summary>
        /// Esta funcion obtiene el camnio critico en caso de que haya
        /// multiples los obtiene todos
        /// </summary>
        /// <param name="intIdOperation"></param>
        /// <param name="cDatos"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        private void ObtenerCaminoCritico(Int32 intIdOperation, clsDatosJobShop cDatos, clsDatosSchedule cSchedule, Boolean blnMulticriticalPath)
        {
            List<Int32> lstCriticalPath = new List<int>();
            Queue<Int32> queIdOperationToExplore = new Queue<int>(); // Otras operaciones no exploradas
            StringBuilder sbFirma = new StringBuilder();
            string strFirma = "";
            Boolean blnEsOptimoMachine = true;
            Boolean blnEsOptimoJob = true;
            Int32 intIdMachineFirst = cDatos.dicIdOperationIdMachine[intIdOperation];
            Int32 intIdJobFirst = cDatos.dicIdOperationIdJob[intIdOperation];
            queIdOperationToExplore.Enqueue(intIdOperation);
            // Va explorando las operaciones incluyendo otros camnios criticos
            Boolean blnEnBucleMulticritical = true;
            while (queIdOperationToExplore.Count > 0 && blnEnBucleMulticritical)
            {
                Int32 intIdMachineOld = -1;
                Int32 intIdMachine;
                Boolean blnEnBucle = true;
                intIdOperation = queIdOperationToExplore.Dequeue();
                lstCriticalPath.Add(intIdOperation);
                cSchedule.hsIdOperationInCriticalPath.Add(intIdOperation);
                List<Int32> lstBlock = new List<int>();
                while (blnEnBucle)
                {
                    // Guarda el primer camino critico como firma
                    if (strFirma == "")
                        sbFirma.Append("_" + intIdOperation);
                    intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperation];
                    // Chequea optimalidad en maquina todas las operaciones en el camino critico en la misma maquina
                    if (intIdMachine != intIdMachineFirst)
                        blnEsOptimoMachine = false;
                    // Chequea optimalidad en trabajo, todo el camino critico el mismo trabajo
                    if (cDatos.dicIdOperationIdJob[intIdOperation] != intIdJobFirst)
                        blnEsOptimoJob = false;
                    // Si ha cambiado la maquina respecto a la opearcion anterior guarda el bloque si al menos tiene 2 operaciones
                    if (intIdMachineOld > -1 && intIdMachine != intIdMachineOld)
                    {
                        if (lstBlock.Count > 1)
                            cSchedule.lstCriticalBlocks.Add(lstBlock);
                        lstBlock = new List<int>();
                    }
                    lstBlock.Add(intIdOperation);
                    // Obtiene las operaciones anteriores
                    Int32 intIdPreviousJob = cDatos.dicIdOperationIdPreviousInJob[intIdOperation];
                    Int32 intIdPreviousMachine = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperation];
                    // Comprueba si el tiempo de finalizacion de ambos es el mismo
                    if (intIdPreviousJob > -1 && intIdPreviousMachine > -1 && cSchedule.dicIdOperationEndTime[intIdPreviousJob] == cSchedule.dicIdOperationEndTime[intIdPreviousMachine])
                    {
                        // Sigue explorando por maquina
                        lstCriticalPath.Add(intIdPreviousMachine);
                        cSchedule.hsIdOperationInCriticalPath.Add(intIdPreviousMachine);
                        intIdOperation = intIdPreviousMachine;
                        // Guarda por explorar por job
                        queIdOperationToExplore.Enqueue(intIdPreviousJob);
                    }
                    else if (intIdPreviousJob > -1 && intIdPreviousMachine > -1 && cSchedule.dicIdOperationEndTime[intIdPreviousJob] > cSchedule.dicIdOperationEndTime[intIdPreviousMachine])
                    {
                        // Critico por trabajo
                        lstCriticalPath.Add(intIdPreviousJob);
                        cSchedule.hsIdOperationInCriticalPath.Add(intIdPreviousJob);
                        intIdOperation = intIdPreviousJob;
                    }
                    else if (intIdPreviousJob > -1 && intIdPreviousMachine > -1) // && cSchedule.dicIdOperationEndTime[intIdPreviousJob] < cSchedule.dicIdOperationEndTime[intIdPreviousMachine])
                    {
                        // Critico por maquina
                        lstCriticalPath.Add(intIdPreviousMachine);
                        cSchedule.hsIdOperationInCriticalPath.Add(intIdPreviousMachine);
                        intIdOperation = intIdPreviousMachine;
                    }
                    else if (intIdPreviousJob < 0 && intIdPreviousMachine > -1)
                    {
                        // Critico por maquina
                        lstCriticalPath.Add(intIdPreviousMachine);
                        cSchedule.hsIdOperationInCriticalPath.Add(intIdPreviousMachine);
                        intIdOperation = intIdPreviousMachine;
                    }
                    else if (intIdPreviousJob > -1 && intIdPreviousMachine < 0)
                    {
                        // Critico por maquina
                        lstCriticalPath.Add(intIdPreviousJob);
                        cSchedule.hsIdOperationInCriticalPath.Add(intIdPreviousJob);
                        intIdOperation = intIdPreviousJob;
                    }
                    else
                        blnEnBucle = false; // En este caso no hay previo por ninguno de los dos
                    intIdMachineOld = intIdMachine;
                } // Fin while en bucle
                if (lstBlock.Count > 1)
                    cSchedule.lstCriticalBlocks.Add(lstBlock);
                if (strFirma == "")
                    strFirma = sbFirma.ToString();
                // Determina si lo debe hacer una vez o muchas (multicritical)
                blnEnBucleMulticritical = blnMulticriticalPath;
            }
            cSchedule.blnEsOptima = blnEsOptimoJob || blnEsOptimoMachine;
            cSchedule.strFirmaCaminoCritico = strFirma;
        }

        /// <summary>
        /// Esta funcion obtiene los tiempos de comienzo y de fin
        /// de cada actividad utlizando bellman
        /// </summary>
        /// <param name="cDatos"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        private (double dblMakespanMax, Int32 intIdOperationMax) CalcularBellmanObtenerTiempos(clsDatosJobShop cDatos, clsDatosSchedule cSchedule)
        {
            Queue<Int32> queExplotacion = new Queue<int>(); // Lista de operaciones que estan listas pero no hemmos mirado sus siguientes
            HashSet<Int32> hsIdOperacionesReady = new HashSet<int>(); // Hash con las operaciones que estan listas (tienen las anteriores calculadas)
            Int32 intIteraciones = 0;
            double dblMakespanMax = -1;
            Int32 intIdOperationMax = -1;
            // Este es un diccionario para calcular la posicion de cada operacion en su maquina
            Dictionary<Int32, List<Int32>> dicIdMachineToIdOperationList = new Dictionary<int, List<int>>();
            // Toma las semillas que son las primeras operaciones en maquinas y que no tengan anteriores en trabajo
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationFirst)
            {
                                Int32 intIdOperacion = kvPair.Value;
                // Comprueba si la operacion esta lista                
                if (cDatos.dicIdOperationIdPreviousInJob[intIdOperacion] == -1)
                {
                    Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperacion];
                    if (!dicIdMachineToIdOperationList.ContainsKey(intIdMachine))
                        dicIdMachineToIdOperationList.Add(intIdMachine, new List<Int32>());
                    dicIdMachineToIdOperationList[intIdMachine].Add(intIdOperacion);
                    cSchedule.dicIdOperationPosicionInMachine.Add(intIdOperacion, dicIdMachineToIdOperationList[intIdMachine].Count);

                    queExplotacion.Enqueue(intIdOperacion);
                    hsIdOperacionesReady.Add(intIdOperacion);
                    cSchedule.dicIdOperationStartTime.Add(intIdOperacion, 0);
                    cSchedule.dicIdOperationEndTime.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);

                    cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperacion, -1);
                    if (cSchedule.dicIdOperationEndTime[intIdOperacion] > dblMakespanMax)
                    {
                        dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperacion];
                        intIdOperationMax = intIdOperacion;
                    }
                }
            }
            while (queExplotacion.Count() > 0)
            {
                Int32 intIdOperation = queExplotacion.Dequeue();
                intIteraciones++;
                // Comprueba si la siguiente en  job esta lista
                Int32 intIdOperationNextJob = cDatos.dicIdOperationIdNextInJob[intIdOperation];
                if (intIdOperationNextJob != -1)
                {
                    Int32 intIdPreviousInMachine = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperationNextJob];
                    if (!hsIdOperacionesReady.Contains(intIdOperationNextJob) && (intIdPreviousInMachine == -1 || hsIdOperacionesReady.Contains(intIdPreviousInMachine)))
                    {
                        Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperationNextJob];
                        if (!dicIdMachineToIdOperationList.ContainsKey(intIdMachine))
                            dicIdMachineToIdOperationList.Add(intIdMachine, new List<Int32>());
                        dicIdMachineToIdOperationList[intIdMachine].Add(intIdOperationNextJob);
                        cSchedule.dicIdOperationPosicionInMachine.Add(intIdOperationNextJob, dicIdMachineToIdOperationList[intIdMachine].Count);

                        queExplotacion.Enqueue(intIdOperationNextJob);
                        hsIdOperacionesReady.Add(intIdOperationNextJob);
                        // Finaliza mas tarde la anterior por Job
                        if (intIdPreviousInMachine == -1 || cSchedule.dicIdOperationEndTime[intIdOperation] > cSchedule.dicIdOperationEndTime[intIdPreviousInMachine])
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdOperation]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationNextJob]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextJob, intIdOperation);
                        }
                        else // Finaliz mas tarde por maquina
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdPreviousInMachine]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdPreviousInMachine] + cDatos.dicIdOperationTime[intIdOperationNextJob]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextJob, intIdPreviousInMachine);
                        }
                        // Obtiene la operacion con mayor makespan
                        if (cSchedule.dicIdOperationEndTime[intIdOperationNextJob] > dblMakespanMax)
                        {
                            dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperationNextJob];
                            intIdOperationMax = intIdOperationNextJob;
                        }
                    }
                }
                // Comprueba si la siguiente en maquina esta lista
                Int32 intIdOperationNextMachine = cSchedule.dicIdOperationIdNextInMachine[intIdOperation];
                if (intIdOperationNextMachine != -1)
                {
                    Int32 intIdPreviousInJob = cDatos.dicIdOperationIdPreviousInJob[intIdOperationNextMachine];
                    if (!hsIdOperacionesReady.Contains(intIdOperationNextMachine) && (intIdPreviousInJob == -1 || hsIdOperacionesReady.Contains(intIdPreviousInJob)))
                    {
                        Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperationNextMachine];
                        if (!dicIdMachineToIdOperationList.ContainsKey(intIdMachine))
                            dicIdMachineToIdOperationList.Add(intIdMachine, new List<Int32>());
                        dicIdMachineToIdOperationList[intIdMachine].Add(intIdOperationNextMachine);
                        cSchedule.dicIdOperationPosicionInMachine.Add(intIdOperationNextMachine, dicIdMachineToIdOperationList[intIdMachine].Count);

                        queExplotacion.Enqueue(intIdOperationNextMachine);
                        hsIdOperacionesReady.Add(intIdOperationNextMachine);
                        // Finaliza mas tarde la anterior por maquina
                        if (intIdPreviousInJob == -1 || cSchedule.dicIdOperationEndTime[intIdOperation] > cSchedule.dicIdOperationEndTime[intIdPreviousInJob])
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdOperation]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationNextMachine]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextMachine, intIdOperation);
                        }
                        else // Finaliz mas tarde por job
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdPreviousInJob]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdPreviousInJob] + cDatos.dicIdOperationTime[intIdOperationNextMachine]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextMachine, intIdPreviousInJob);
                        }
                        if (cSchedule.dicIdOperationEndTime[intIdOperationNextMachine] > dblMakespanMax)
                        {
                            dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperationNextMachine];
                            intIdOperationMax = intIdOperationNextMachine;
                        }
                    }
                }
            }
            return (dblMakespanMax, intIdOperationMax);
        }

        /// <summary>
        /// Esta funcio calcula los tiempos de comienzo y final 
        /// para cada operacion a partir de un Schedule pasado
        /// utiliza la ecuacion de Bellman O(n) siendo n el numero
        /// de operaciones.
        /// Devuelve un string con las operaciones del camino critico.
        /// En caso de que la solucion sea optima devuele un booleano
        /// indicandolo. La solucion es optima si todas las operaciones
        /// del camino critico se realizan en una misma maquina o si 
        /// el camino critico son todas operaciones de un solo trabajo.
        /// Puede haber soluciones optimas que no cumplan lo anterior.
        /// Tambien detecta si hay un bucle, es decir hay una operacion
        /// que tiene una operacion precedente en maquina que es al 
        /// mismo tiempo subsiguiente a dicha operacion
        /// </summary>
        /// <param name="cDatos"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public void CalcularBellmanOld(clsDatosJobShop cDatos, clsDatosSchedule cSchedule)
        {
            Boolean blnEsOptimo = false;
            //StringBuilder Para Signature Tabu list
            StringBuilder sbSignature = new StringBuilder();
            // Pone a cero los valores de Bellman
            cSchedule.ReinicializarDatosBellman();
            // Comienza
            Dictionary<Int32, Int32> dicIdMachineNextLstPosition = new Dictionary<int, int>();
            Queue<Int32> queExplotacion = new Queue<int>(); // Lista de operaciones que estan listas pero no hemmos mirado sus siguientes
            HashSet<Int32> hsIdOperacionesReady = new HashSet<int>(); // Hash con las operaciones que estan listas (tienen las anteriores calculadas)
            Int32 intIteraciones = 0;
            double dblMakespanMax = -1;
            Int32 intIdOperationMax = -1;
            // Toma las semillas que son las primeras operaciones en maquinas y que no tengan anteriores en trabajo
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationFirst)
            {
                // Comprueba si la operacion esta lista
                Int32 intIdOperacion = kvPair.Value;
                if (cDatos.dicIdOperationIdPreviousInJob[intIdOperacion] == -1)
                {
                    queExplotacion.Enqueue(intIdOperacion);
                    hsIdOperacionesReady.Add(intIdOperacion);
                    cSchedule.dicIdOperationStartTime.Add(intIdOperacion, 0);
                    cSchedule.dicIdOperationEndTime.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);

                    cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperacion, -1);
                    if (cSchedule.dicIdOperationEndTime[intIdOperacion] > dblMakespanMax)
                    {
                        dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperacion];
                        intIdOperationMax = intIdOperacion;
                    }
                }
            }
            // Explota las operaciones en cola
            while (queExplotacion.Count() > 0)
            {
                Int32 intIdOperation = queExplotacion.Dequeue();
                intIteraciones++;
                // Comprueba si la siguiente en  job esta lista
                Int32 intIdOperationNextJob = cDatos.dicIdOperationIdNextInJob[intIdOperation];
                if (intIdOperationNextJob != -1)
                {
                    Int32 intIdPreviousInMachine = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperationNextJob];
                    if (!hsIdOperacionesReady.Contains(intIdOperationNextJob) && (intIdPreviousInMachine == -1 || hsIdOperacionesReady.Contains(intIdPreviousInMachine)))
                    {
                        queExplotacion.Enqueue(intIdOperationNextJob);
                        hsIdOperacionesReady.Add(intIdOperationNextJob);
                        // Finaliza mas tarde la anterior por Job
                        if (intIdPreviousInMachine == -1 || cSchedule.dicIdOperationEndTime[intIdOperation] > cSchedule.dicIdOperationEndTime[intIdPreviousInMachine])
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdOperation]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationNextJob]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextJob, intIdOperation);
                        }
                        else // Finaliz mas tarde por maquina
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdPreviousInMachine]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdPreviousInMachine] + cDatos.dicIdOperationTime[intIdOperationNextJob]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextJob, intIdPreviousInMachine);
                        }
                        if (cSchedule.dicIdOperationEndTime[intIdOperationNextJob] > dblMakespanMax)
                        {
                            dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperationNextJob];
                            intIdOperationMax = intIdOperationNextJob;
                        }
                    }
                }
                // Comprueba si la siguiente en maquina esta lista
                Int32 intIdOperationNextMachine = cSchedule.dicIdOperationIdNextInMachine[intIdOperation];
                if (intIdOperationNextMachine != -1)
                {
                    Int32 intIdPreviousInJob = cDatos.dicIdOperationIdPreviousInJob[intIdOperationNextMachine];
                    if (!hsIdOperacionesReady.Contains(intIdOperationNextMachine) && (intIdPreviousInJob == -1 || hsIdOperacionesReady.Contains(intIdPreviousInJob)))
                    {
                        queExplotacion.Enqueue(intIdOperationNextMachine);
                        hsIdOperacionesReady.Add(intIdOperationNextMachine);
                        // Finaliza mas tarde la anterior por maquina
                        if (intIdPreviousInJob == -1 || cSchedule.dicIdOperationEndTime[intIdOperation] > cSchedule.dicIdOperationEndTime[intIdPreviousInJob])
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdOperation]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationNextMachine]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextMachine, intIdOperation);
                        }
                        else // Finaliz mas tarde por job
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdPreviousInJob]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdPreviousInJob] + cDatos.dicIdOperationTime[intIdOperationNextMachine]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextMachine, intIdPreviousInJob);
                        }
                        if (cSchedule.dicIdOperationEndTime[intIdOperationNextMachine] > dblMakespanMax)
                        {
                            dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperationNextMachine];
                            intIdOperationMax = intIdOperationNextMachine;
                        }
                    }
                }
            }
            // Si no ha recorrido todas hay BUCLE
            if (cSchedule.dicIdOperationStartTime.Count != cDatos.dicIdOperationTime.Count)
            {
                cSchedule.blnEsBucle = true;
                return;
            }
            cSchedule.dblMakespan = dblMakespanMax;
            // TODO AQUI SE PODRIA QUITAR EL CALCULO DEL MAKESPAN Y HACERLO EN EL BUCLE DE BELLMAN END
            // Obtiene el camino critico
            Boolean blnEnGranBucle = true;
            cSchedule.hsIdOperationInCriticalPath.Add(intIdOperationMax);
            List<Int32> lstBlock = new List<int>();
            Int32 intIdMachineOld = -1;
            Int32 intMaximoOperacionEnBloques = Int32.MinValue;
            blnEsOptimo = true;
            Int32 intCheckMaquina = -1;
            Int32 intCheckTrabajo = -1;
            while (blnEnGranBucle)
            {
                Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperationMax];
                // Chequea condiciones de optimalidad
                if (intCheckTrabajo == -1)
                    intCheckTrabajo = cDatos.dicIdOperationIdJob[intIdOperationMax];
                else
                {
                    // Si el camino critico pasa por mas de un trabajo no puede ser optimo
                    if (intCheckTrabajo != cDatos.dicIdOperationIdJob[intIdOperationMax])
                        blnEsOptimo = false;
                }
                if (intCheckMaquina == -1)
                    intCheckMaquina = intIdMachine;
                else
                {
                    // Si el camino critico pasa por mas de una maquina no puede ser optimo
                    if (intCheckMaquina != intIdMachine)
                        blnEsOptimo = false;
                }
                // Añade al critical block
                if (intIdMachine == intIdMachineOld || intIdMachineOld == -1)
                    lstBlock.Add(intIdOperationMax);
                else
                {
                    // Guarda el maximo de operaciones que tiene el bloque mas grande
                    if (lstBlock.Count > intMaximoOperacionEnBloques)
                        intMaximoOperacionEnBloques = lstBlock.Count;
                    // Si el bloque tiene al menos dos operaciones lo guarda.
                    if (lstBlock.Count >= 2)
                        cSchedule.lstCriticalBlocks.Add(lstBlock);
                    lstBlock = new List<int>();
                    lstBlock.Add(intIdOperationMax);
                }
                // Pasa a la operacion anterior
                if (cSchedule.dicIdOperationPreviousCritialPath[intIdOperationMax] == -1)
                    blnEnGranBucle = false;
                else
                {
                    intIdOperationMax = cSchedule.dicIdOperationPreviousCritialPath[intIdOperationMax];
                    cSchedule.hsIdOperationInCriticalPath.Add(intIdOperationMax);
                    sbSignature.Append("_" + intIdOperationMax);
                }
                intIdMachineOld = intIdMachine;
            }
            if (lstBlock.Count > 2)
                cSchedule.lstCriticalBlocks.Add(lstBlock);
            cSchedule.blnEsBucle = false;
            cSchedule.blnEsOptima = blnEsOptimo;
            cSchedule.strFirmaCaminoCritico = sbSignature.ToString();            
        }

        
    }
}
