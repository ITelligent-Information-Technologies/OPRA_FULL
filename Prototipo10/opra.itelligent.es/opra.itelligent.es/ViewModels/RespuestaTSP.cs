using System.Collections.Generic;

namespace opra.itelligent.es.ViewModels
{
    public class RespuestaTSP
    {
        public IEnumerable<PointData> Puntos { get; set; }
        public double Coste { get; set; }
        public double Tiempo { get; set; }
    }
}
