FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]

COPY ["src/Services/Cart/Cart.Domain/Cart.Domain.csproj", "src/Services/Cart/Cart.Domain/"]
COPY ["src/Services/Cart/Cart.Application/Cart.Application.csproj", "src/Services/Cart/Cart.Application/"]
COPY ["src/Services/Cart/Cart.Infrastructure/Cart.Infrastructure.csproj", "src/Services/Cart/Cart.Infrastructure/"]
COPY ["src/Services/Cart/Cart.API/Cart.API.csproj", "src/Services/Cart/Cart.API/"]

RUN dotnet restore "src/Services/Cart/Cart.API/Cart.API.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Cart/ src/Services/Cart/

WORKDIR "/src/src/Services/Cart/Cart.API"
RUN dotnet build "Cart.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cart.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cart.API.dll"]
