# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# 42WASD Community Web — multi-stage build.
#
# The server project (Community.Web.Server) is an ASP.NET Core hosted Blazor
# WebAssembly app: it serves the compiled client assets (dotnet publish pulls
# the Client project in via ProjectReference) AND hosts the Bolero remoting
# API + data under src/Community.Web.Server/data.
#
# Build stage:  SDK   -> dotnet publish the server project (Release)
# Runtime stage: aspnet -> run the published DLL as a non-root user on :8080
# ---------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0.111 AS build
WORKDIR /src

# Restore with the solution first (caches NuGet layers).
COPY Community.Web.sln ./
COPY global.json ./
COPY src/Community.Web.Shared/Community.Web.Shared.fsproj src/Community.Web.Shared/
COPY src/Community.Web.Client/Community.Web.Client.fsproj src/Community.Web.Client/
COPY src/Community.Web.Server/Community.Web.Server.fsproj src/Community.Web.Server/
RUN dotnet restore src/Community.Web.Server/Community.Web.Server.fsproj

# Copy the remainder and publish (Release, framework-dependent).
COPY src/ src/
RUN dotnet publish src/Community.Web.Server/Community.Web.Server.fsproj \
    -c Release \
    -o /app \
    --no-restore \
    --no-self-contained

# ---------------------------------------------------------------------------
# Runtime
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Non-root user for K8s PodSecurity standards (allowPrivilegeEscalation:false).
USER app

# Kestrel listens on this port; the K8s Service/Ingress target it.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app ./
# The data JSON files are already in the publish output (CopyToOutputDirectory).
ENTRYPOINT ["dotnet", "Community.Web.Server.dll"]