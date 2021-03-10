using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiServer.Models
{
    public struct TemperatureMeasurement
    {
        public DateTime Time { get; set; }
        public float Temperature { get; set; }
        public int SensorId { get; set; }
    }
}
