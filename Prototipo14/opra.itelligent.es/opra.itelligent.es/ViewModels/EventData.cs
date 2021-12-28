using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netIndustrial.Models
{
    public class EventData
    {

        public string id { get; set; }
        public string id2 { get; set; }
        public string text { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string resource { get; set; }
        public string backColor { get; set; }
        public bool barHidden { get; set; }
        public string bubbleHtml { get; set; }
        public string borderColor { get; set; }

        public string start { get { return startDate.ToString("yyyy-MM-dd'T'HH:mm:ss"); } }
        public string end { get { return endDate.ToString("yyyy-MM-dd'T'HH:mm:ss"); } }
    }
}
