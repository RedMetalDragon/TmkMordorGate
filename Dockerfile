# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TmkMordorGate/TmkMordorGate/TmkMordorGate.csproj", "TmkMordorGate/"]
RUN dotnet restore "TmkMordorGate/TmkMordorGate.csproj"

COPY ["TmkMordorGate/TmkMordorGate/", "TmkMordorGate/"]
RUN dotnet build "TmkMordorGate/TmkMordorGate.csproj" -c Release -o /app/build




# Publish the application
FROM build AS publish
RUN dotnet publish "TmkMordorGate/TmkMordorGate.csproj" -c Release -o /app/publish

# Final stage / runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TmkMordorGate.dll"]
