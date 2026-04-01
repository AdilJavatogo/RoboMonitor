using RoboMonitor.Models;
using RoboMonitor.Repositories;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace RoboMonitor.Metrics
{
    public class RobotMetrics
    {
        private readonly Meter _robotMeter;
        private readonly IRobotRepository _repository;

        public RobotMetrics(IMeterFactory meterFactory, IRobotRepository repository)
        {
            _repository = repository;
            _robotMeter = meterFactory.Create("RoboMonitor.Robots");

            // Opret målingerne som kigger på det Rigtige repository
            _robotMeter.CreateObservableGauge("robotfleet", () =>
            {
                return _repository.GetAllRobots().Select(robot => new Measurement<int>(
                    robot.BatteryLevel,
                    new TagList {
                        { "robot_id", robot.RobotId },
                        { "hospital", robot.Hospital },
                        { "department", robot.Department },
                        { "status_text", robot.RobotStatus.ToString() },
                        { "state", robot.RobotState.ToString() },
                        { "task", robot.RobotTask.ToString() },
                        { "sensor", robot.SensorStatus.ToString() },
                        { "temperature", robot.CPUTemperature },
                        { "lift", robot.Lift },
                        { "estop", robot.EStop },
                        { "charging_time", robot.ChargingTime },
                        { "break_count", robot.BreakCount }
                    }));
            });

            // Fordi vi nu bruger rigtige enums, kan vi bare caste dem til (int)
            _robotMeter.CreateObservableGauge("robot_status_code", () =>
                _repository.GetAllRobots().Select(r => new Measurement<int>((int)r.RobotStatus, GetCommonTags(r))));

            _robotMeter.CreateObservableGauge("robot_state_code", () =>
                _repository.GetAllRobots().Select(r => new Measurement<int>((int)r.RobotState, GetCommonTags(r))));

            _robotMeter.CreateObservableGauge("robot_sensor_code", () =>
                _repository.GetAllRobots().Select(r => new Measurement<int>((int)r.SensorStatus, GetCommonTags(r))));

            // task
            _robotMeter.CreateObservableGauge("robot_task_code", () =>
                _repository.GetAllRobots().Select(r => new Measurement<int>((int)r.RobotTask, GetCommonTags(r))));

            // estop
            _robotMeter.CreateObservableGauge("robot_estop_code", () =>
                _repository.GetAllRobots().Select(r => new Measurement<int>(r.EStop ? 1 : 0, GetCommonTags(r))));
        }

        private static IEnumerable<KeyValuePair<string, object?>> GetCommonTags(Robot robot)
        {
            return new KeyValuePair<string, object?>[]
            {
                new("robot_id", robot.RobotId),
                new("hospital", robot.Hospital),
                new("department", robot.Department)
            };
        }

    }
}
