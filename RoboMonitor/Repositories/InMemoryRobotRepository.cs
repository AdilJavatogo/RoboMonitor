using RoboMonitor.Models;
using System.Collections.Concurrent;

namespace RoboMonitor.Repositories
{
    public class InMemoryRobotRepository : IRobotRepository
    {
        private readonly ConcurrentDictionary<int, Robot> _robots = new();
        public IEnumerable<Robot> GetAllRobots()
        {
            return _robots.Values;
        }

        public void UpsertRobot(Robot robot)
        {
            _robots.AddOrUpdate(robot.RobotId, robot, (id, exsistingRobot) => robot);
        }
    }
}
