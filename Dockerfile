# syntax=docker/dockerfile:1
#
# Portic gateway — community runtime image.
# Multi-stage: SDK builds/publishes; a chiseled, non-root ASP.NET runtime serves it.
# NOTE (ADR-0004): container packaging is a Fabric concern. This is a minimal community image, not a
# local packaging platform — see docs/adr/0004-container-packaging-is-a-fabric-concern.md.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore against the project graph first, so the (slow) restore layer caches across source changes.
COPY global.json Directory.Build.props ./
COPY src/Portic.Core/Portic.Core.csproj src/Portic.Core/
COPY src/Portic.Providers.Stub/Portic.Providers.Stub.csproj src/Portic.Providers.Stub/
COPY src/Portic.Gateway/Portic.Gateway.csproj src/Portic.Gateway/
RUN dotnet restore src/Portic.Gateway/Portic.Gateway.csproj

# Build + publish the host (analyzers + warnings-as-errors still apply).
COPY src/ src/
RUN dotnet publish src/Portic.Gateway/Portic.Gateway.csproj -c Release -o /app --no-restore

# ---- runtime ----
# Chiseled image: no shell/package manager, minimal surface, runs as non-root (UID 1654) and binds
# :8080 via ASPNETCORE_HTTP_PORTS by default.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "Portic.Gateway.dll"]
