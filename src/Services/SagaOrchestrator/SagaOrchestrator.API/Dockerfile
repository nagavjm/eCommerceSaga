FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]

COPY ["src/Services/SagaOrchestrator/SagaOrchestrator.Domain/SagaOrchestrator.Domain.csproj", "src/Services/SagaOrchestrator/SagaOrchestrator.Domain/"]
COPY ["src/Services/SagaOrchestrator/SagaOrchestrator.Application/SagaOrchestrator.Application.csproj", "src/Services/SagaOrchestrator/SagaOrchestrator.Application/"]
COPY ["src/Services/SagaOrchestrator/SagaOrchestrator.Infrastructure/SagaOrchestrator.Infrastructure.csproj", "src/Services/SagaOrchestrator/SagaOrchestrator.Infrastructure/"]
COPY ["src/Services/SagaOrchestrator/SagaOrchestrator.API/SagaOrchestrator.API.csproj", "src/Services/SagaOrchestrator/SagaOrchestrator.API/"]

RUN dotnet restore "src/Services/SagaOrchestrator/SagaOrchestrator.API/SagaOrchestrator.API.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/SagaOrchestrator/ src/Services/SagaOrchestrator/

WORKDIR "/src/src/Services/SagaOrchestrator/SagaOrchestrator.API"
RUN dotnet build "SagaOrchestrator.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SagaOrchestrator.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SagaOrchestrator.API.dll"]
