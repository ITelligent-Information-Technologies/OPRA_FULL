using netIndustrial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace opra.itelligent.es.Models
{
    public class RespuestaScheduler
    {
        public int IdProblema { get; set; }
        public string Guid { get; set; }
        public double Makespan { get; set; }
        public double BestMakespan { get; set; }
        public DateTime StartDate { get; set; }
        public double Days { get; set; }
        public List<ResourceData> ResourcesMachine { get; set; }
        public List<ResourceData> ResourcesJob { get; set; }
        public List<EventData> EventsMachine { get; set; }
        public List<EventData> EventsJob { get; set; }
    }
}
