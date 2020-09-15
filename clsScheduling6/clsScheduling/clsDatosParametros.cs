using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    public enum TiposFirmaTabuList
    {
        CaminoUaV, // Guarda el cambio de U a V antes del camnbio-> U,SU,...,AV,V
        CaminoUaVYPosicionMaquinas, // Ademas del camino de U a V tambien la posicion de cada operacion en maquina (vale la de la primera maquina)

    }

    public enum TiposVecindarios
    {
        N0, // Vencindario aleatorio dentro de un bloque critico cambia una operacion aleatoria por otra
        N1, // Vencidario N1
        N6, // Vencindario N6
        NingunVencindario // NO se utliza ningun vecindario
    }
    public class clsDatosParametros
    {
        public Int32 intMaxSegundos = 300; // Maximo de segundos si se alcanza finaliza (si se alcanza antes max iteraciones tambine finaliza)
        public Int32 intMaxIteraciones = 30000; // Iteraciones totales maximas
        public Int32 intMaxIteracionesPorBucle = 1000; // Numero maximo de iteraciones por bucle se pone a cero si se encuentra una solucion mejor
        public Int32 intMaxIteracionCalentamiento = 300; // Maximo de iteraciones inicial para calentamiento
        public Int32 intMaxStackBackTrack = 30; // Maximo de elementos en stack de back track
        public TiposFirmaTabuList enuGuardarTabuList = TiposFirmaTabuList.CaminoUaV; // Guarda la firma para representa una solucion en tabu
        public Int32 intTabuListMin; // Tamaño minimo busca tabu
        public Int32 intTabuListMax; // Tamaño maximo busca tabu
        public Boolean blnN6PermitirForbiddenNonProfitable = false; // Si en N6 permite los Forbidden Non Profitable (movimientos malos y prohibidos)
        public Boolean blnSiNoHayParaBacktrackUtilizaN1 = true; // Si no hay para backtrack y quedan iteraciones utiliza N1
        public Boolean blnConsiderarMultiplesCaminosCriticos = true; // Si hay mas de un camino critico ternlos en cuenta
        // Vecindarios
        public TiposVecindarios enuVecindarioPrincipal = TiposVecindarios.N6; // Tipo de vecindario principal para utilizar
        public TiposVecindarios enuSiListaMovimientosVaciaUtilizar = TiposVecindarios.N1; // Si no hay candidatos en la lista utiliza este vencindario
        public TiposVecindarios enuSiListaBacktrackVaciaUtilizar = TiposVecindarios.N1; // Si no hay mas elmeentos en la lista de backtrack utiliza este vecindario
        // Simulated Annealing
        public double dblSAProbabilidadAceptarSolucionInicial = 0.3; // Probabilidad de aceptar una solucion mala inicial tras el calentamiento
        public double dblSAProbabilidadAceptarSolucionFinal = 0.01; // Probabilidad de aceptar una solucion mala inicial tras el calentamiento
        public double dblSAIncrementoMakespanMedio = 10; // Cuanto se empeora de media el makespan
        public double dblSADecrementoTemperatura; // Ratio de decremento de temperatura (calculada)
        public double dblSATemperaturaInicial; // Temperatura inicial (calculada)
        public clsDatosParametros(clsDatosJobShop cData)
        {
            RecalcularParametrosTabuList(cData);
            RecalculaParametrosSimulatedAnnealing();
            
        }
        public void RecalcularParametrosTabuList(clsDatosJobShop cData)
        {
            // Se calcula el tamaño minimo y maximo de la lista tabu  segun paper de Zhang
            intTabuListMin = 10 + Convert.ToInt32((double)cData.intJobsCount / cData.intMachinesCount);
            intTabuListMax = intTabuListMin + 2;
        }
        public void RecalculaParametrosSimulatedAnnealing()
        {
            // Calcula los datos del simulated annealing
            Int32 intNumeroInteraciones = Convert.ToInt32(((double)intMaxIteraciones / intMaxIteracionesPorBucle));
            dblSATemperaturaInicial = (-dblSAIncrementoMakespanMedio) / System.Math.Log(dblSAProbabilidadAceptarSolucionInicial);
            dblSADecrementoTemperatura = ((-dblSAIncrementoMakespanMedio) / (System.Math.Log(dblSAProbabilidadAceptarSolucionFinal))) / dblSATemperaturaInicial;
            dblSADecrementoTemperatura = System.Math.Pow(dblSADecrementoTemperatura, ((double)1 / intNumeroInteraciones));
        }
    }
}
