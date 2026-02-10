using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//var grafana = builder.AddGrafana("grafana");

var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
       .WithBindMount("../prometheus.yml", "/etc/prometheus/prometheus.yml")
       .WithHttpEndpoint(port: 9090, targetPort: 9090);

var grafana = builder.AddContainer("grafana", "grafana/grafana")
       .WithHttpEndpoint(port: 3000, targetPort: 3000)
       .WithExternalHttpEndpoints();

var api = builder.AddProject<Projects.RoboMonitor>("RobotMonitor")
    .WithReference(prometheus);

builder.Build().Run();
