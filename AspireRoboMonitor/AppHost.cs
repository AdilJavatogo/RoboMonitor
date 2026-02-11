using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.RoboMonitor>("robotmonitor")
    .WithHttpEndpoint(port: 5280, targetPort: 8080, name: "api-http");

// Vi navngiver endpointet "api-endpoint" for at undgå forvirring
var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
       .WithBindMount("./prometheus.yml", "/etc/prometheus/prometheus.yml")
       .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "prom-http")
       .WaitFor(api);

var grafana = builder.AddContainer("grafana", "grafana/grafana")
       .WithBindMount("./grafana-datasource.yaml", "/etc/grafana/provisioning/datasources/datasource.yaml")
       .WithHttpEndpoint(port: 3000, targetPort: 3000)
       .WithExternalHttpEndpoints()
       //.WithReference(prometheus) // Dette er korrekt - Grafana skal bruge Prometheus
       .WaitFor(prometheus); // Vent på Prometheus


builder.Build().Run();