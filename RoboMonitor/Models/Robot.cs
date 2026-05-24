using System.Text.Json.Serialization;

namespace RoboMonitor.Models
{
    public class Robot
    {
        public int RobotId { get; set; } 

        public string Hospital { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public int BatteryLevel { get; set; }

        public int Distance { get; set; }
        // med og uden seng
        // beregning til "Vaskning" og beregning til "Levering"  

        public int CPUTemperature { get; set; }

        public int ChargingTime { get; set; }

        public int Lift { get; set; }

        public bool EStop { get; set; }

        public int BreakCount { get; set; }


        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SensorStatus SensorStatus { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RobotState RobotState { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RobotTask RobotTask { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RobotStatus RobotStatus { get; set; }

    }

    public enum RobotTask { Ingen = 0, Vaskning = 1, Levering = 2, Inspektion = 3 }
    public enum RobotState { Ledig = 1, Kører = 2, Oplader = 3, Fejl = 4, Nødstop = 5 }
    public enum RobotStatus { Offline = 0, Online = 1, Oplader = 2 }
    public enum SensorStatus { OK = 1, Advarsel = 2, Fejl = 3 }


}
