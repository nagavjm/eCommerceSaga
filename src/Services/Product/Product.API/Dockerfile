FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore caching
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/BuildingBlocks.Observability/"]

COPY ["src/Services/Product/Product.Domain/Product.Domain.csproj", "src/Services/Product/Product.Domain/"]
COPY ["src/Services/Product/Product.Application/Product.Application.csproj", "src/Services/Product/Product.Application/"]
COPY ["src/Services/Product/Product.Infrastructure/Product.Infrastructure.csproj", "src/Services/Product/Product.Infrastructure/"]
COPY ["src/Services/Product/Product.API/Product.API.csproj", "src/Services/Product/Product.API/"]

RUN dotnet restore "src/Services/Product/Product.API/Product.API.csproj"

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Product/ src/Services/Product/

WORKDIR "/src/src/Services/Product/Product.API"
RUN dotnet build "Product.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Product.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Product.API.dll"]
