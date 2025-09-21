# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

# ‰”Œ „·›«  «·„‘«—Ì⁄ (csproj) √Ê·«
COPY VehicleRegisterSystem.Web/VehicleRegisterSystem.Web.csproj VehicleRegisterSystem.Web/
COPY VehicleRegisterSystem.Application/VehicleRegisterSystem.Application.csproj VehicleRegisterSystem.Application/
COPY VehicleRegisterSystem.Infrastructure/VehicleRegisterSystem.Infrastructure.csproj VehicleRegisterSystem.Infrastructure/
COPY VehicleRegisterSystem.Domain/VehicleRegisterSystem.Domain.csproj VehicleRegisterSystem.Domain/
COPY VehicleRegisterSystem.sln ./

# «” ⁄«œ… «·Õ“„
RUN dotnet restore "VehicleRegisterSystem.sln"

# ‰”Œ »«ﬁÌ «·„·›« 
COPY . .

# «·œŒÊ· ⁄·Ï „Ã·œ Web project Ê»‰«¡Â
WORKDIR "/source/VehicleRegisterSystem.Web"
RUN dotnet build "VehicleRegisterSystem.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VehicleRegisterSystem.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "VehicleRegisterSystem.Web.dll"]
