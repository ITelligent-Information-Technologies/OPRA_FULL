using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    class clsN1
    {
        private Random _rnd = new Random(75);

        /// <summary>
        /// Seleciona el movimiento N1 para ello se toma uno de lo bloques
        /// criticos al azar y despues:
        /// -Si es el primer bloque del camino critico se cambia la ultima opracion del bloque
        /// por la anterior.
        /// -Si es el ultimo bloque del camino critica se cambia la primera operacion por la siguiente
        /// -Si es otro bloque se cambia la primera operacion por la siguiente o la ultima por la anterior
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public clsDatosCambio SeleccionarMovimiento(clsDatosJobShop cData, clsDatosSchedule cSchedule)
        {
            // Selecciona el bucle critico
            Int32 intCriticalBlockIndex = _rnd.Next(0, cSchedule.lstCriticalBlocks.Count - 1);
            if (cSchedule.lstCriticalBlocks[intCriticalBlockIndex].Count < 2)
                new Exception("Error numero en bloque critico menor que 2");
            // Si es un bloque intermedio
            Boolean blnPrimeraOperacion = true;
            Int32 intIdOperacionUltimaBloque = cSchedule.lstCriticalBlocks[intCriticalBlockIndex][0];
            Int32 intIdOperacionPrimeraBloque = cSchedule.lstCriticalBlocks[intCriticalBlockIndex][cSchedule.lstCriticalBlocks[intCriticalBlockIndex].Count - 1];
            // Si es primer bloque del camino critico
            if (cSchedule.dicIdOperationIdPreviousInMachine[intIdOperacionPrimeraBloque] == -1)
                blnPrimeraOperacion = true;
            else if (cSchedule.dicIdOperationIdNextInMachine[intIdOperacionUltimaBloque] == -1)
                blnPrimeraOperacion = false;
            else
            {
                if (_rnd.NextDouble() > 0.5)
                    blnPrimeraOperacion = true;
                else
                    blnPrimeraOperacion = false;
            }
            // Se supone que se cambia la ultima por la anterior
            Int32 intIdV = intIdOperacionUltimaBloque;
            Int32 intIdU = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperacionUltimaBloque];
            // Realmente se cambia la primera operacion del bloque por la siguiente
            if (blnPrimeraOperacion)
            {
                intIdU = intIdOperacionPrimeraBloque;
                intIdV = cSchedule.dicIdOperationIdNextInMachine[intIdOperacionPrimeraBloque];
            }
            //Sustituye una operacion por la otra la U va antes que la V
            clsDatosCambio cCambio = new clsDatosCambio();
            cCambio.intIdOperacionU = intIdU;
            cCambio.intIdOperacionV = intIdV;
            cCambio.strPathUaV = intIdU + "_" + intIdV;
            cCambio.intPosicionPrimeraMaquina = cSchedule.dicIdOperationPosicionInMachine[intIdV];
            return cCambio;
        }

        /// <summary>
        /// Realiza el movimiento N1 que se le pasa en cMovimiento
        /// criticos al azar y despues:
        /// -Si es el primer bloque del camino critico se cambia la ultima opracion del bloque
        /// por la anterior.
        /// -Si es el ultimo bloque del camino critica se cambia la primera operacion por la siguiente
        /// -Si es otro bloque se cambia la primera operacion por la siguiente o la ultima por la anterior
        /// </summary>
        /// <param name="cMovimiento"></param>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        public clsDatosMovimiento  RealizarMovimiento(clsDatosCambio cCambio, clsDatosJobShop cData, clsDatosSchedule cSchedule)
        {
            Int32 intIdU = cCambio.intIdOperacionU;
            Int32 intIdV = cCambio.intIdOperacionV;
            Int32 intIdOperacionPosteriorV = cSchedule.dicIdOperationIdNextInMachine[intIdV];
            Int32 intIdOperacionAnteriorU = cSchedule.dicIdOperationIdPreviousInMachine[intIdU];
            // Realiza el cambio de la primera y ultima en maquina
            Int32 intIdMachine = cData.dicIdOperationIdMachine[intIdU];
            if (intIdOperacionPosteriorV == -1)
                cSchedule.dicIdMachineIdOperationLast[intIdMachine] = intIdU;
            if (intIdOperacionAnteriorU == -1)
                cSchedule.dicIdMachineIdOperationFirst[intIdMachine] = intIdV;
            // Realiza el cambio en AU y V
            if (intIdOperacionAnteriorU > -1)
                cSchedule.dicIdOperationIdNextInMachine[intIdOperacionAnteriorU] = intIdV;
            cSchedule.dicIdOperationIdPreviousInMachine[intIdV] = intIdOperacionAnteriorU;
            // Realiza el cambio PV y U
            cSchedule.dicIdOperationIdNextInMachine[intIdU] = intIdOperacionPosteriorV;
            if (intIdOperacionPosteriorV > -1)
                cSchedule.dicIdOperationIdPreviousInMachine[intIdOperacionPosteriorV] = intIdU;
            // Realiza el cambio de U y V
            cSchedule.dicIdOperationIdNextInMachine[intIdV] = intIdU;
            cSchedule.dicIdOperationIdPreviousInMachine[intIdU] = intIdV;
            // Guarda los cambios realizados
            clsDatosMovimiento cMovimiento = new clsDatosMovimiento();
            cMovimiento.bleEsN6 = false;
            cMovimiento.intIdOperacionU = intIdU;
            cMovimiento.intIdOperacionAnteriorU = cSchedule.dicIdOperationIdPreviousInMachine[intIdU];
            cMovimiento.intIdOperacionPosteriorU = cSchedule.dicIdOperationIdNextInMachine[intIdU];
            cMovimiento.intIdOperacionV = intIdV;
            cMovimiento.intIdOperacionAnteriorV = cSchedule.dicIdOperationIdPreviousInMachine[intIdV];
            cMovimiento.intIdOperacionPosteriorV = cSchedule.dicIdOperationIdNextInMachine[intIdV];
            return cMovimiento;
        }
    }
}
