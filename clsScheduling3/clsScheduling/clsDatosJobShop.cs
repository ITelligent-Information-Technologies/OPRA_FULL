using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    /// <summary>
    /// Tipos de tiempos. El sitema trabajo
    /// con tiempos en horas, pero si se da
    /// entrada a tiempos de procesamiento
    /// en otras unidades se indica y se
    /// convierten a horas
    /// </summary>
    [Serializable]
    public enum TipoTiempo
    {
        UnidadEs30Segundos,
        UnidadEs1Minuto,
        UnidadEs15Minutos,
        UnidadEs30Minutos,
        UnidadEs45Minutos,
        UnidadEs1Hora
    }

    /// <summary>
    /// Importante en esta clase se supone que el numero de las operaciones
    /// indican en el orden en cada trabajo. Es decir si hay 100 operaciones
    /// estarán numeradas del 1 al 100 y el trabajo 1 tendra por ejemplo
    /// de la 1 a la 20 y ese es el orden de procesamiento y el trabajo 2 tendra
    /// por ejemplo dela 21 a la 34 y ese es el orden de procesamiento.
    /// </summary>
    [Serializable]
    public class clsDatosJobShop
    {
        // Datos Generales
        public string strNombre = ""; // Nombre del problema en la literatura
        public Int32 intMachinesCount = -1; // Numero de maquinas las maquinas indexadas de 0 a cuenta-1
        public Int32 intJobsCount = -1; // Numero de trabajos los trabajos indexados de 0 a cuenta-1
        public double dblMakespanBest = -1; // Mejor makespan en la literatura
        public Boolean blnMakespanIsOptimal = false; // Si el makespan es optimo
        public double dblMakespanLowerBound; // LowerBound del makespan (normalmente no es posible alcanzarlo)
        public TipoTiempo enuTipoTiempo = TipoTiempo.UnidadEs1Minuto;//Tipo de unidad de tiempo del problema
        // Operacion
        public Dictionary<Int32, Int32> dicIdOperationIdMachine = new Dictionary<int, int>(); // Id de la operacion al Id de la maquina
        public Dictionary<Int32, Int32> dicIdOperationIdJob = new Dictionary<int, int>(); // Id de la operacion al Id del trabajo al que pertenece
        public Dictionary<Int32, Int32> dicIdOperationIdNextInJob = new Dictionary<int, int>(); // Id de la operacion siguente en el trabajo -1 si no hay
        public Dictionary<Int32, Int32> dicIdOperationIdPreviousInJob = new Dictionary<int, int>(); // Id de la operacion anterior en el trabajo -1 si no hay
        public Dictionary<Int32, double> dicIdOperationTime = new Dictionary<int, double>(); // Id de la opearcion a su tiempo de procesamiento
        public Dictionary<Int32, Int32> dicIdJobIdOperationFirst = new Dictionary<int, int>(); // Id de la primera operacion del trabajo
        public Dictionary<Int32, Int32> dicIdJobIdOperationLast = new Dictionary<int, int>(); // Id de la ultima operacion del trabajo       
    }

    /// <summary>
    /// Esta clase guarda la posicion de las operaciones en las maquinas,
    /// esto es el Schedule
    /// </summary>
    [Serializable]
    public class clsDatosSchedule
    {
        // Resultados
        public double dblMakespan; // Valor del makespan
        public Boolean blnEsOptima = false; // Si la solucion es optima (todas las operaciones camino critico misma maquian o mismo trabajo)
        public Boolean blnEsBucle = false; // Si la solucion contiene un bucle (No es una solucion factible)
        // Caminos crititos
        public string strFirmaCaminoCritico = ""; // contiene un string con todas las operacoines del camino critico (si hay varios de uno de ellos)
        public List<List<Int32>> lstCriticalBlocks = new List<List<int>>(); // lista con listas de bloque que son 3 operaciones seguidas en una misma maquina en el critial path
        public HashSet<Int32> hsIdOperationInCriticalPath = new HashSet<int>();// Hash con las operaciones en camino critico (si hay varios caminos se incluyen todas)
        // Maquinas
        public Dictionary<Int32, Int32> dicIdOperationIdNextInMachine = new Dictionary<int, int>(); // Siguiente operacion en la maquina, si no hay -1
        public Dictionary<Int32, Int32> dicIdOperationIdPreviousInMachine = new Dictionary<int, int>(); // Operacion previa en la maquina, si no hay -1
        public Dictionary<Int32, Int32> dicIdMachineIdOperationFirst = new Dictionary<int, int>(); // Para cada maquina la primera operacion en maquina
        public Dictionary<Int32, Int32> dicIdMachineIdOperationLast = new Dictionary<int, int>(); // Para cada maquina la ultima operacion en maquina
        public Dictionary<Int32, Int32> dicIdOperationPosicionInMachine = new Dictionary<int, int>(); // Indica la posicion en maquina de cada operacion siendo la primera posicion la 1
        // Resultados Bellman
        public Dictionary<Int32, Int32> dicIdOperationPreviousCritialPath = new Dictionary<int, Int32>(); // Operacoin previa en el critical path
        public Dictionary<Int32, double> dicIdOperationStartTime = new Dictionary<int, double>(); // Comienzo de la operacion tras bellman
        public Dictionary<Int32, double> dicIdOperationEndTime = new Dictionary<int, double>(); // Fin de la operacion tras bellman
  
        public void ReinicializarDatosBellman()
        {
            dicIdOperationPreviousCritialPath = new Dictionary<int, Int32>(); // Operacoin previa en el critical path
            dicIdOperationStartTime = new Dictionary<int, double>(); // Comienzo de la operacion tras bellman
            dicIdOperationEndTime = new Dictionary<int, double>(); // Fin de la operacion tras bellman
            hsIdOperationInCriticalPath =new HashSet<int> (); // Hash con las operaciones en los critcal paths
            dicIdOperationPosicionInMachine = new Dictionary<int, int>(); // IdOperacion a posicion en maquina
            lstCriticalBlocks = new List<List<int>>();
            blnEsOptima = false;
            blnEsBucle = false;
            strFirmaCaminoCritico = "";
        }

    }

    /// <summary>
    /// Esta clase una vez que se tiene un schedule lo resuelve utilizando
    /// bellman y devuelve los resultados
    /// </summary>
    public class clsDatosBellmanResultados2
    {
        public Dictionary<Int32, Int32> dicIdOperationPreviousCritialPath = new Dictionary<int, Int32>(); // Operacoin previa en el critical path
        public Dictionary<Int32, double> dicIdOperationStartTime = new Dictionary<int, double>(); // Comienzo de la operacion tras bellman
        public Dictionary<Int32, double> dicIdOperationEndTime = new Dictionary<int, double>(); // Fin de la operacion tras bellman
        public Dictionary<Int32, DateTime> dicIdOperationFechaStart = new Dictionary<int, DateTime>(); // Fecha de comienzo para la operacion
        public Dictionary<Int32, DateTime> dicIdOperationFechaEnd = new Dictionary<int, DateTime>(); // Fecha de fin para la operacion
                                                                                                     // public double dblMakespan; // Valor del makespan
        public List<Int32> lstIdOperationInCriticalPath = new List<int>(); // Lista de operaciones ordeandas de ultima a primera en critical path.
        //public Tuple<Int32, Int32> tupMove; // Movimiento seleccionado
    }




    /// <summary>
    /// Esta clase guarda los resultados de un schedule
    /// y los datos del problema, vienen a ser los datos
    /// finales
    /// </summary>
    [Serializable]
    public class clsDatosResultados
    {
        public clsDatosJobShop cData; // Datos del jobShop
        public Dictionary<Int32, DateTime> dicIdOperacionFechaStart2; // Fecha de comienzo de cada operacion
        public Dictionary<Int32, DateTime> dicIdOperacionFechaEnd2; // Fecha de comienzo de cada operacion11
        public Dictionary<Int32, clsFechasStartEnd> dicIdOperacionFechaStartEnd; // Id operacion y fecha start y end de la misma
        public Dictionary<Int32, List<clsFechasStartEnd>> dicIdOperacionListaStartEnd; // Para cada idOpercion la lista de comienzos y fin segun los festivos y horarios
    }
    [Serializable]
    public class clsFechasStartEnd
    {
        public DateTime dtmFechaStart; // FechaComienzo
        public DateTime dtmFechaEnd; // Fecha Fin
    }

    /// <summary>
    /// Esta clase tiene los horarios de trabajo
    /// o festivos. Basicamente tiene un diccionario
    /// con los horarios especiales (ej. fechas de festivos)
    /// y un diccionario con los horarios de cada dia de la 
    /// semana.
    /// Siempre tiene prioridad los horaios especiales que los
    /// horarios de trabajo.
    /// </summary>
    public class clsDatosHorarios
    {
        public Dictionary<DateTime, List<clsDatosHorariosHoras>> dicFechasEspecialesHorarios = new Dictionary<DateTime, List<clsDatosHorariosHoras>>(); // Horarios para fechas especiales (ej. festivos)
        public Dictionary<Int32, List<clsDatosHorariosHoras>> dicIdDiasSemanaHorarios = new Dictionary<int, List<clsDatosHorariosHoras>>(); // Horario dias de la semana (1 es Lunes)
    }

    /// <summary>
    /// Esta clase guarda los intervalos de horas que conmponen los horarios
    /// </summary>
    public class clsDatosHorariosHoras
    {
        public string strNombreHorario; // Nombre para el horario (ej. mañana)
        public double dblHoraDesde; // Hora en que empieza este horario (0,0 a 24,0)
        public double dblHoraHasta; // Hora en que acaba este horario (0,0 a 24,0)
        public Boolean blnSinActividad; // Si este horario es sin actividad  (ej. un festivo que no se trabaja se marcaria esto)
    }
}
