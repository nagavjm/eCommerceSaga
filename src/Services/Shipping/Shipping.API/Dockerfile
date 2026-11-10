FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]

COPY ["src/Services/Shipping/Shipping.Domain/Shipping.Domain.csproj", "src/Services/Shipping/Shipping.Domain/"]
COPY ["src/Services/Shipping/Shipping.Application/Shipping.Application.csproj", "src/Services/Shipping/Shipping.Application/"]
COPY ["src/Services/Shipping/Shipping.Infrastructure/Shipping.Infrastructure.csproj", "src/Services/Shipping/Shipping.Infrastructure/"]
COPY ["src/Services/Shipping/Shipping.API/Shipping.API.csproj", "src/Services/Shipping/Shipping.API/"]

RUN dotnet restore "src/Services/Shipping/Shipping.API/Shipping.API.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Shipping/ src/Services/Shipping/

WORKDIR "/src/src/Services/Shipping/Shipping.API"
RUN dotnet build "Shipping.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shipping.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shipping.API.dll"]
