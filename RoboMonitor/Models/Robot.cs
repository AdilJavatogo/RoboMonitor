namespace RoboMonitor.Models
{
    public class Robot
    {
        public int RobotId { get; set; }

        public string Hospital { get; set; }

        public string Department { get; set; }

        public int BatteryLevel { get; set; }

        public int Distance { get; set; }
        // med og uden seng
        // beregning til "Vaskning" og beregning til "Levering"

        public string SensorStatus { get; set; }

        public int CPUTemperature { get; set; }

        public string RobotState { get; set; }

        public string RobotTask { get; set; }

        public string RobotStatus { get; set; }

        public int ChargingTime { get; set; }

        public int Lift { get; set; }

        public bool EStop { get; set; }

        public int BreakCount { get; set; }

    }

    public enum RobotTask
    {
        Vaskning,
        Levering,
        Inspektion
    }

    public enum RobotState{
        Ledig,
        Kører,
        Oplader,
        Fejl // skal måske være forskellig
    }

    public enum RobotStatus
    {
        Online,
        Oplader,
        Offline
    }

    public enum SensorStatus
    {
        OK,
        Advarsel,
        Fejl // skal måske være forskellig
    }

    public enum EStopStatus
    {
        Aktiveret,
        Deaktiveret
    }


}
