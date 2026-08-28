# Build from repo root (needs Directory.Build.props / Directory.Packages.props):
#   docker build -f src/Simando.Api/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Manifests only first, so layer caches until a dependency actually changes
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Simando.Domain/*.csproj          src/Simando.Domain/
COPY src/Simando.Application/*.csproj     src/Simando.Application/
COPY src/Simando.Infrastructure/*.csproj  src/Simando.Infrastructure/
COPY src/Simando.Api/*.csproj             src/Simando.Api/
RUN dotnet restore src/Simando.Api/Simando.Api.csproj

COPY src/ src/
RUN dotnet publish src/Simando.Api/Simando.Api.csproj \
        -c Release -o /app --no-restore

# -extra carries ICU. id-ID formats "." thousands / "," decimal, printed on signed
# docs -- plain alpine has no ICU, culture would silently fall back to invariant.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine-extra AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    TZ=Asia/Jakarta

COPY --from=build /app ./

ARG APP_UID
USER $APP_UID

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget --spider -q -T 5 http://localhost:8080/scalar/v1 || exit 1
ENTRYPOINT ["dotnet", "Simando.Api.dll"]
