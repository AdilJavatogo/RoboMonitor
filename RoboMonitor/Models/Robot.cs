namespace RoboMonitor.Models
{
    public class Robot
    {
        public int RobotId { get; set; }

        public string Hospital { get; set; }

        public string Department { get; set; }

        public int BatteryLevel { get; set; } // Batteriniveau i procent

        public int Distance { get; set; } // Afstand i meter
        // med og uden seng
        // beregning til "Vaskning" og beregning til "Levering"

        public string SensorStatus { get; set; } // Sensorstatus (OK, Warning, Error)

        public int CPUTemperature { get; set; } // CPU-temperatur i grader Celsius

        public string RobotState { get; set; } // Robottilstand (Idle, Moving, Charging, Error)

        public string RobotTask { get; set; } // Robotopgaver ("Vaskning", "Levering", "Inspektion")

        public string RobotStatus { get; set; } // Robotstatus (Grøn = Kører, Gul = Oplader, Rød = Fejl/Offline)
        
        public int ChargingTime { get; set; } // Ladetid i minutter

        public int Lift { get; set; }

        // E stop - historisk data
        public bool EStop { get; set; }

        // bremse aktivering, 500 bremseaktiveringer
        public int BreakCount { get; set; }

    }
}
