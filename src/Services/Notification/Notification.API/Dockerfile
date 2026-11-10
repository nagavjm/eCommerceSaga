FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]

COPY ["src/Services/Notification/Notification.Domain/Notification.Domain.csproj", "src/Services/Notification/Notification.Domain/"]
COPY ["src/Services/Notification/Notification.Application/Notification.Application.csproj", "src/Services/Notification/Notification.Application/"]
COPY ["src/Services/Notification/Notification.Infrastructure/Notification.Infrastructure.csproj", "src/Services/Notification/Notification.Infrastructure/"]
COPY ["src/Services/Notification/Notification.API/Notification.API.csproj", "src/Services/Notification/Notification.API/"]

RUN dotnet restore "src/Services/Notification/Notification.API/Notification.API.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Notification/ src/Services/Notification/

WORKDIR "/src/src/Services/Notification/Notification.API"
RUN dotnet build "Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
