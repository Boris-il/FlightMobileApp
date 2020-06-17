using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FlightMobileApp.Models
{
    public enum Result { Ok, NotOk};

    public class Command
    {
        // Controlling preperties.
        [JsonProperty(PropertyName = "elevator")]
        public double Elevator { get; set; }
        [JsonProperty(PropertyName = "rudder")]
        public double Rudder { get; set; }
        [JsonProperty(PropertyName = "throttle")]
        public double Throttle { set; get; }
        [JsonProperty(PropertyName = "aileron")]
        public double Aileron { set; get; }


        public string ParseElevatorToString()
        {
            return "set /controls/flight/elevator " + this.Elevator + "\n";
        }

        public string ParseRudderToString()
        {
            return "set /controls/flight/rudder " + this.Rudder + "\n";
        }

        public string ParseThrottleToString()
        {
            return "set /controls/engines/current-engine/throttle " + this.Throttle + "\n";
        }

        public string ParseAileronToString()
        {
            return "set /controls/flight/aileron " + this.Aileron + "\n";
        }
    }
}
