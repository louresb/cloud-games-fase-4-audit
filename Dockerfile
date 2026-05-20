FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./src/Fiap.CloudGames.Audit.Api/Fiap.CloudGames.Audit.Api.csproj", "src/Fiap.CloudGames.Audit.Api/"]
COPY ["./src/Fiap.CloudGames.Audit.Application/Fiap.CloudGames.Audit.Application.csproj", "src/Fiap.CloudGames.Audit.Application/"]
COPY ["./src/Fiap.CloudGames.Audit.Domain/Fiap.CloudGames.Audit.Domain.csproj", "src/Fiap.CloudGames.Audit.Domain/"]
COPY ["./src/Fiap.CloudGames.Audit.Infrastructure/Fiap.CloudGames.Audit.Infrastructure.csproj", "src/Fiap.CloudGames.Audit.Infrastructure/"]
RUN dotnet restore "./src/Fiap.CloudGames.Audit.Api/Fiap.CloudGames.Audit.Api.csproj"
COPY . .
WORKDIR "/src/src/Fiap.CloudGames.Audit.Api"
RUN dotnet build "./Fiap.CloudGames.Audit.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fiap.CloudGames.Audit.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.CloudGames.Audit.Api.dll"]
