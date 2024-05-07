# Base image
# DockerFile needs to be checked
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TmkMordorGate.csproj", "./"]
RUN dotnet restore "TmkMordorGate.csproj"
COPY . .
RUN dotnet build "TmkMordorGate.csproj" -c Release -o /app/build

ARG ENVIRONMENT=Production
COPY ["appsettings.${ENVIRONMENT}.json", "/app/appsettings.json"]

FROM build AS publish
RUN dotnet publish "TmkMordorGate.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TmkMordorGate.dll"]
