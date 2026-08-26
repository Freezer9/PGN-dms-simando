# Build from repo root (needs Directory.Build.props / Directory.Packages.props):
#   docker build -f src/Simando.Web/Dockerfile .

# Styles/app.css -> wwwroot/app.css. Own stage: SDK image has no JS runtime.
# --frozen-lockfile: drifted bun.lock fails build, not silently re-resolves.
FROM oven/bun:1-alpine AS css
WORKDIR /css
COPY src/Simando.Web/package.json src/Simando.Web/bun.lock ./
RUN bun install --frozen-lockfile
COPY src/Simando.Web/Styles/     Styles/
COPY src/Simando.Web/Components/ Components/
RUN bun run build:css

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# manifests only first, so layer caches until a dependency actually changes
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Simando.Domain/*.csproj          src/Simando.Domain/
COPY src/Simando.Application/*.csproj     src/Simando.Application/
COPY src/Simando.Infrastructure/*.csproj  src/Simando.Infrastructure/
COPY src/Simando.Web/*.csproj             src/Simando.Web/
# SimandoBuildCss=false: app.css already built above, this image has no bun/pnpm/npm
RUN dotnet restore src/Simando.Web/Simando.Web.csproj -p:SimandoBuildCss=false

COPY src/ src/
COPY --from=css /css/wwwroot/app.css src/Simando.Web/wwwroot/app.css
RUN dotnet publish src/Simando.Web/Simando.Web.csproj \
        -c Release -o /app --no-restore -p:SimandoBuildCss=false

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
    CMD wget --spider -q -T 5 http://localhost:8080/ || exit 1
ENTRYPOINT ["dotnet", "Simando.Web.dll"]
