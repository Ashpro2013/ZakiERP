# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AshproSAASERP/AshproSAASERP.Web/AshproSAASERP.Web.csproj", "AshproSAASERP/AshproSAASERP.Web/"]
COPY ["AshproSAASERP.Repo.PSQL/AshproSAASERP.Repo.PSQL.csproj", "AshproSAASERP.Repo.PSQL/"]
COPY ["AshproSAASERP.Interface/AshproSAASERP.Interface.csproj", "AshproSAASERP.Interface/"]
COPY ["AshproSAASERP.Models/AshproSAASERP.Models.csproj", "AshproSAASERP.Models/"]
COPY ["AshproSAASERP.ZatcaEngine/AshproSAASERP.ZatcaEngine.csproj", "AshproSAASERP.ZatcaEngine/"]
COPY ["AshproSAASERP/AshproSAASERP.Web.Client/AshproSAASERP.Web.Client.csproj", "AshproSAASERP/AshproSAASERP.Web.Client/"]
COPY ["AshproSAASERP/AshproSAASERP.Shared/AshproSAASERP.Shared.csproj", "AshproSAASERP/AshproSAASERP.Shared/"]

RUN dotnet restore "AshproSAASERP/AshproSAASERP.Web/AshproSAASERP.Web.csproj"

COPY . .
WORKDIR "/src/AshproSAASERP/AshproSAASERP.Web"

RUN dotnet publish "AshproSAASERP.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AshproSAASERP.Web.dll"]