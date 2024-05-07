# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Ensure you are copying the csproj file from the correct subdirectory
COPY ["TmkMordorGate.csproj", "./"]
RUN dotnet restore "TmkMordorGate.csproj"

# Copy the rest of your source code from the TmkMordorGate directory
COPY TmkMordorGate/ ./
RUN dotnet build "TmkMordorGate.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TmkMordorGate.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TmkMordorGate.dll"]
