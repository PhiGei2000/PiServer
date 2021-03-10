using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiServer.Models
{
    public struct ClimateData
    {
        public Sensor sensor;
        public IEnumerable<TemperatureMeasurement> data;
    }
}
