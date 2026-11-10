FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]

COPY ["src/Services/Payment/Payment.Domain/Payment.Domain.csproj", "src/Services/Payment/Payment.Domain/"]
COPY ["src/Services/Payment/Payment.Application/Payment.Application.csproj", "src/Services/Payment/Payment.Application/"]
COPY ["src/Services/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj", "src/Services/Payment/Payment.Infrastructure/"]
COPY ["src/Services/Payment/Payment.API/Payment.API.csproj", "src/Services/Payment/Payment.API/"]

RUN dotnet restore "src/Services/Payment/Payment.API/Payment.API.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Payment/ src/Services/Payment/

WORKDIR "/src/src/Services/Payment/Payment.API"
RUN dotnet build "Payment.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payment.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.API.dll"]
