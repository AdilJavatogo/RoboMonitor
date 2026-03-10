FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RoboMonitor/RoboMonitor.csproj", "RoboMonitor/"]
COPY ["RoboMonitor.ServiceDefaults/RoboMonitor.ServiceDefaults.csproj", "RoboMonitor.ServiceDefaults/"]
RUN dotnet restore "./RoboMonitor/RoboMonitor.csproj"
COPY . .
WORKDIR "/src/RoboMonitor"
RUN dotnet build "./RoboMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RoboMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoboMonitor.dll"]