FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]

COPY ["src/Services/Order/Order.Domain/Order.Domain.csproj", "src/Services/Order/Order.Domain/"]
COPY ["src/Services/Order/Order.Application/Order.Application.csproj", "src/Services/Order/Order.Application/"]
COPY ["src/Services/Order/Order.Infrastructure/Order.Infrastructure.csproj", "src/Services/Order/Order.Infrastructure/"]
COPY ["src/Services/Order/Order.API/Order.API.csproj", "src/Services/Order/Order.API/"]

RUN dotnet restore "src/Services/Order/Order.API/Order.API.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Order/ src/Services/Order/

WORKDIR "/src/src/Services/Order/Order.API"
RUN dotnet build "Order.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.API.dll"]
