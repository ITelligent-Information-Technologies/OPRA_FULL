using System.Collections.Generic;

namespace opra.itelligent.es.ViewModels
{
    public class VehicleRoutingModel
    {
        public int Problema { get; set; }
        public IEnumerable<int> SolucionOptima { get; set; }
        public IEnumerable<PointData> Puntos { get; set; }
    }
}
