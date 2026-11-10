FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]
COPY ["src/ApiGateway/OcelotGateway/OcelotGateway.csproj", "src/ApiGateway/OcelotGateway/"]

RUN dotnet restore "src/ApiGateway/OcelotGateway/OcelotGateway.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/ApiGateway/OcelotGateway/ src/ApiGateway/OcelotGateway/

WORKDIR "/src/src/ApiGateway/OcelotGateway"
RUN dotnet build "OcelotGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OcelotGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcelotGateway.dll"]
