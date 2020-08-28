using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    /// <summary>
    /// Esta clase contiene el motor de decision de
    /// Simulated Annealing
    /// </summary>
    class clsSimulatedAnnealing
    {
        private Random _rnd = new Random(100);
        private double _dblT; // Temperatura actual
        private double _dblTo; // Temperatura inicial
        private double _dblDecrementoTemperatura; // Decrementos de la temperatura
        public clsSimulatedAnnealing(clsDatosParametros cParametros)
        {
            _dblTo = cParametros.dblSATemperaturaInicial;
            _dblDecrementoTemperatura = cParametros.dblSADecrementoTemperatura;
            _dblT = _dblTo;
        }

        public void DecrementarTemperatura()
        {
            _dblT = _dblDecrementoTemperatura * _dblT;
        }
        public Boolean Aceptar(double dblMakespan, double dblMakespanMin)
        {
            double dblRandom = _rnd.NextDouble();
            double dblUmbral = Math.Exp(-(dblMakespan - dblMakespanMin) / _dblT);
            if (dblRandom < dblUmbral)
                return true;
            return false;
        }
    }
}
