using RoboMonitor.Models;

namespace RoboMonitor.Repositories
{
    public interface IRobotRepository
    {
        IEnumerable<Robot> GetAllRobots();
        void UpsertRobot(Robot robot); // Upsert = Update or Insert
    }
}
