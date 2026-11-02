# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ZakiSAASERP/ZakiSAASERP.Web/ZakiSAASERP.Web.csproj", "ZakiSAASERP/ZakiSAASERP.Web/"]
COPY ["ZakiSAASERP.Repo.PSQL/ZakiSAASERP.Repo.PSQL.csproj", "ZakiSAASERP.Repo.PSQL/"]
COPY ["ZakiSAASERP.Interface/ZakiSAASERP.Interface.csproj", "ZakiSAASERP.Interface/"]
COPY ["ZakiSAASERP.Models/ZakiSAASERP.Models.csproj", "ZakiSAASERP.Models/"]
COPY ["ZakiSAASERP.ZatcaEngine/ZakiSAASERP.ZatcaEngine.csproj", "ZakiSAASERP.ZatcaEngine/"]
COPY ["ZakiSAASERP/ZakiSAASERP.Web.Client/ZakiSAASERP.Web.Client.csproj", "ZakiSAASERP/ZakiSAASERP.Web.Client/"]
COPY ["ZakiSAASERP/ZakiSAASERP.Shared/ZakiSAASERP.Shared.csproj", "ZakiSAASERP/ZakiSAASERP.Shared/"]

RUN dotnet restore "ZakiSAASERP/ZakiSAASERP.Web/ZakiSAASERP.Web.csproj"

COPY . .
WORKDIR "/src/ZakiSAASERP/ZakiSAASERP.Web"

RUN dotnet publish "ZakiSAASERP.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ZakiSAASERP.Web.dll"]