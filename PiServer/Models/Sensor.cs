using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PiServer.Models
{
    public struct Sensor
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public IPAddress IPAddress { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Sensor sensor &&
                   ID == sensor.ID &&
                   Name == sensor.Name &&
                   EqualityComparer<IPAddress>.Default.Equals(IPAddress, sensor.IPAddress);
        }

        public static bool operator ==(Sensor left, Sensor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Sensor left, Sensor right)
        {
            return !(left == right);
        }
    }
}
