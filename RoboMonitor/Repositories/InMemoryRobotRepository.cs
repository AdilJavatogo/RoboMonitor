using RoboMonitor.Models;
using System.Collections.Concurrent;

namespace RoboMonitor.Repositories
{
    public class InMemoryRobotRepository : IRobotRepository
    {
        // ConcurrentDictionary håndterer uden problemer at Python spammer den med data
        private readonly ConcurrentDictionary<int, Robot> _robots = new();
        public IEnumerable<Robot> GetAllRobots()
        {
            return _robots.Values;
        }

        public void UpsertRobot(Robot robot)
        {
            // Upsert: Hvis robotten allerede findes, opdateres den, ellers tilføjes den
            _robots.AddOrUpdate(robot.RobotId, robot, (id, exsistingRobot) => robot);
        }
    }
}
